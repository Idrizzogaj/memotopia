import * as crypto from 'crypto';
import { join } from 'path';

import {
    ConflictException,
    Injectable,
    InternalServerErrorException,
    NotFoundException,
    UnauthorizedException,
} from '@nestjs/common';
import * as bcrypt from 'bcryptjs';
import { Response } from 'express';
import { Severity } from '@sentry/node';

import { UserProvider } from '~common/constants/enums/user-provider.enum';
import { User } from '~common/db/entities/user.entity';
import { AuthService } from '~modules/auth/auth.service';
import { MailService } from '~modules/mail/mail.service';
import { RestrictionsService } from '~modules/restrictions/restrictions.service';
import { UserStatisticsService } from '~modules/user-statistics/user-statistics.service';
import { logEvent } from '~modules/logger/logger.utils';

import { CreateUserResDto } from './dto/create-user-res.dto';
import { ForgotPasswordReqDto } from './dto/forgot-password-req.dto';
import { PaymentStatusReqDto } from './dto/payment-status-req.dto';
import { UpdateAccountReqDto } from './dto/update-account-req.dto';
import { UserReqDto } from './dto/user-req.dto';
import { UserResDto } from './dto/user-res.dto';
import { UserRepository } from './user.repository';
import { ResetPasswordReqDto } from './dto/reset-password-req';
import { DeleteAccountReqDto } from './dto/delete-account-req.dto';

const VERIFICATION_TTL_MS = 24 * 60 * 60 * 1000;
const RESEND_COOLDOWN_MS = 60 * 1000;

@Injectable()
export class UserService {
    constructor(
        private userRepository: UserRepository,
        private authService: AuthService,
        private userStatisticsService: UserStatisticsService,
        private restrictionsService: RestrictionsService,
        private mailService: MailService,
    ) {}

    async getById(id: number): Promise<UserResDto> {
        const user = await this.userRepository.findOne(id);

        if (!user) throw new NotFoundException(`User with ID ${id} not found!`, '404');

        return user;
    }

    async getRandom(user: User): Promise<UserResDto> {
        const date = new Date()
        const userIds = (await this.userRepository.getUsersThatPlayedThisMonth(date.getMonth() + 1, date.getFullYear())).map(userIdsObj => userIdsObj.userId);
        let randomUserId = user.id;

        if(userIds.length >= 2) {
            while (user.id === randomUserId) {
                randomUserId = userIds[ Math.floor(Math.random() * userIds.length) ];
            }
        } else {
            const maxId = await this.userRepository.getMaxId();
    
            while (user.id === randomUserId) {
                randomUserId = Math.floor(Math.random() * maxId) + 1;
            }
        }

        const randomUser = await this.getById(randomUserId);
        return randomUser;
    }

    async create(userReqDto: UserReqDto): Promise<CreateUserResDto> {
        const { username, email, password } = userReqDto;
        const user = new User();
        user.username = username;
        user.email = email;
        user.provider = UserProvider.MEMOTOPIA;

        //password
        user.salt = await bcrypt.genSalt();
        user.password = await bcrypt.hash(password, user.salt);

        user.email = email.trim();
        user.emailVerificationPending = true;

        const existing = await this.userRepository.findByUsernameOrEmailCaseInsensitive(
            username,
            user.email,
        );
        const expiryCutoff = Date.now() - VERIFICATION_TTL_MS;
        let blocked = false;
        for (const other of existing) {
            const stale =
                other.emailVerificationPending &&
                (!other.emailVerificationSentAt ||
                    other.emailVerificationSentAt.getTime() < expiryCutoff);
            // An abandoned, never-confirmed registration must not block the real owner of the
            // address (or username) forever; it holds no data, so it is released after the token expired.
            if (stale) await this.userRepository.delete(other.id);
            else blocked = true;
        }

        if (blocked) throw new ConflictException('USERNAME_OR_EMAIL_EXISTS');

        try {
            const newUser: User = await this.userRepository.save(user);
            await this.userStatisticsService.create({ level: 0, xp: 0, user });
            await this.restrictionsService.create(newUser);

            let mailSent = true;
            try {
                await this.issueVerification(newUser);
            } catch (mailError) {
                mailSent = false;
                logEvent(
                    'verificationMailFailed',
                    { userId: newUser.id },
                    { exception: mailError, severity: Severity.Error, resource: UserService.name },
                );
            }

            // No token is issued until the address has been confirmed.
            return { verificationRequired: true, email: newUser.email, mailSent };
        } catch (error) {
            if (error.code === '23505') {
                throw new ConflictException('USERNAME_OR_EMAIL_EXISTS');
            } else {
                throw new InternalServerErrorException('While creating user or user statistics.');
            }
        }
    }

    async updateAccount(id: number, updateAccountReqDto: UpdateAccountReqDto): Promise<UserResDto> {
        const { newUsername, avatar } = updateAccountReqDto;
        const user = await this.getUserEntityById(id);

        const userExists = await this.userRepository.getByUsernameCaseInsensitive(newUsername);

        if (userExists.length > 1) throw new ConflictException('USERNAME_EXISTS');

        if (avatar) user.avatar = avatar;

        if (newUsername) user.username = newUsername;

        await this.userRepository.save(user);
        return user;
    }

    async getUserEntityById(id: number): Promise<User> {
        const user = await this.userRepository.findOne({
            where: { id },
            relations: ['userStatistics'],
        });

        if (!user) throw new NotFoundException(`User with ID ${id} not found!`, '404');

        return user;
    }

    async search(search: string, user: User): Promise<UserResDto[]> {
        return await this.userRepository.searchByUsernamePrefix(search, user.id);
    }

    private hashToken(token: string): string {
        return crypto
            .createHash('sha256')
            .update(token)
            .digest('hex');
    }

    private async issueVerification(user: User): Promise<void> {
        const token = crypto.randomBytes(32).toString('hex');
        const now = new Date();
        user.emailVerificationTokenHash = this.hashToken(token);
        user.emailVerificationSentAt = now;
        user.emailVerificationExpiresAt = new Date(now.getTime() + VERIFICATION_TTL_MS);
        await this.userRepository.save(user);
        await this.mailService.verifyEmail(user, token);
    }

    /** Consumes a one-time token atomically. Returns true only for the single request that wins. */
    async verifyEmail(token: string): Promise<boolean> {
        if (!/^[0-9a-f]{64}$/.test(token || '')) return false;
        const now = new Date();
        const result = await this.userRepository
            .createQueryBuilder()
            .update(User)
            .set({
                emailVerificationPending: false,
                emailVerifiedAt: now,
                emailVerificationTokenHash: null,
                emailVerificationExpiresAt: null,
            })
            .where(
                '"emailVerificationTokenHash" = :hash AND "emailVerificationExpiresAt" > :now AND "emailVerificationPending" = true',
                { hash: this.hashToken(token), now },
            )
            .execute();
        return result.affected === 1;
    }

    /** Always completes silently so the endpoint cannot be used to discover which addresses exist. */
    async resendVerification(identifier: string): Promise<void> {
        const id = identifier.trim();
        const user = id.includes('@')
            ? await this.userRepository.findByEmailCaseInsensitive(id)
            : (await this.userRepository.findByUsernameOrEmailCaseInsensitive(id, id))[0];
        if (!user || !user.emailVerificationPending) return;
        if (
            user.emailVerificationSentAt &&
            Date.now() - user.emailVerificationSentAt.getTime() < RESEND_COOLDOWN_MS
        )
            return;
        try {
            await this.issueVerification(user);
        } catch (error) {
            logEvent(
                'verificationMailFailed',
                { userId: user.id },
                { exception: error, severity: Severity.Error, resource: UserService.name },
            );
        }
    }

    /**
     * Permanently deletes the account and everything owned by it. All dependent tables cascade from
     * "user" except favorites, which is removed explicitly (both directions) in the same transaction.
     * Other players' data (their own levels, statistics, challenges they take part in) is untouched.
     */
    async deleteAccount(userId: number, dto: DeleteAccountReqDto): Promise<void> {
        const user = await this.userRepository.findOne(userId);
        if (!user) throw new NotFoundException('User not found');

        if (user.provider === UserProvider.MEMOTOPIA) {
            const ok = !!dto.password && !!user.password && (await bcrypt.compare(dto.password, user.password));
            if (!ok) throw new UnauthorizedException('INVALID_CREDENTIALS');
        }

        await this.userRepository.manager.transaction(async manager => {
            await manager.query('DELETE FROM "favorites" WHERE "requesterId" = $1 OR "favoritesId" = $1', [
                userId,
            ]);
            await manager.delete(User, userId);
        });

        logEvent('accountDeleted', { userId }, { severity: Severity.Info, resource: UserService.name });
    }

    async updatePaymentStatus(
        id: number,
        paymentStatusReq: PaymentStatusReqDto,
    ): Promise<UserResDto> {
        const { paymentStatus } = paymentStatusReq;
        const user = await this.getUserEntityById(id);

        if (paymentStatus) user.paymentStatus = paymentStatus;

        await this.userRepository.save(user);
        return user;
    }

    async forgotPassword(forgotPasswordReqDto: ForgotPasswordReqDto): Promise<void> {
        const email = forgotPasswordReqDto.email.trim().toLowerCase();
        const user = await this.userRepository.findByEmailCaseInsensitive(email);

        // Same response whether or not the address exists.
        if (!user) return;

        // update forgot password fildes
        const token = crypto.randomBytes(20).toString('hex');
        const date = new Date();
        date.setMinutes(date.getMinutes() + 60);

        user.resetPasswordToken = token;
        user.resetPasswordExpiresAt = date;

        await this.userRepository.save(user);

        // // send forgot password email
        await this.mailService.forgotPassword(user, token);
    }

    async getResetHtml(res: Response, resetPasswordToken: string): Promise<void> {
        const user = await this.userRepository.findOne({ resetPasswordToken });

        if (!user) {
            res.send('<h1>Password reset token is invalid or has expired.</h1>');
            return;
        }

        if (new Date() > user.resetPasswordExpiresAt) {
            res.send('<h1>Password reset token is invalid or has expired.</h1>');
            return;
        }

        res.sendFile(join(__dirname, '../../../../public/reset.html'));
    }

    async reset(
        resetPasswordToken: string,
        resetPasswordReqDto: ResetPasswordReqDto,
    ): Promise<User> {
        const { password } = resetPasswordReqDto;
        const user = await this.userRepository.findOne({ resetPasswordToken });

        if (!user) {
            throw new NotFoundException(
                `User with this password reset token ${resetPasswordToken} not found!`,
            );
        }

        if (new Date() > user.resetPasswordExpiresAt) {
            throw new ConflictException('Password reset token has expired.');
        }

        //password
        user.salt = await bcrypt.genSalt();
        user.password = await bcrypt.hash(password, user.salt);
        user.resetPasswordToken = null;

        return await this.userRepository.save(user);
    }

    // async getAll(): Promise<User[]> {
    //     return await this.userRepository.find();
    // }
}
