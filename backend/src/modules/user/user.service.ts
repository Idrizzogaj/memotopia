import * as crypto from 'crypto';
import { join } from 'path';

import {
    ConflictException,
    Injectable,
    InternalServerErrorException,
    NotFoundException,
} from '@nestjs/common';
import * as bcrypt from 'bcryptjs';
import { Not, Raw } from 'typeorm';
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

        const usernameExists = await this.userRepository.getByUsernameCaseInsensitive(username);
        const emailExists = await this.userRepository.getByEmailCaseInsensitive(email);

        if (usernameExists.length > 0) throw new ConflictException('USERNAME_OR_EMAIL_EXISTS');

        if (emailExists.length > 0) throw new ConflictException('USERNAME_OR_EMAIL_EXISTS');

        try {
            const newUser: User = await this.userRepository.save(user);
            await this.userStatisticsService.create({ level: 0, xp: 0, user });
            await this.restrictionsService.create(newUser);
            const userToReturn = await this.getUserEntityById(newUser.id);
            const accessToken: string = this.authService.generateAccessToken(user);

            return { accessToken, user: userToReturn };
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
        const users = await this.userRepository.find({
            where: {
                id: Not(user.id),
                username: Raw(alias => `LOWER(${alias}) Like '${search.toLowerCase()}%'`),
            },
            relations: ['userStatistics'],
        });

        return users;
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

    async forgotPassword(forgotPasswordReqDto: ForgotPasswordReqDto, host: string): Promise<void> {
        const { email } = forgotPasswordReqDto;
        const user = await this.userRepository.findOne({ email });

        if (!user) {
            const error = new NotFoundException(`User with this email ${email} not found!`);
            logEvent(
                'sendMessage',
                { ...error },
                { exception: error, severity: Severity.Error, resource: UserService.name },
            );
            throw error;
        }

        // update forgot password fildes
        const token = crypto.randomBytes(20).toString('hex');
        const date = new Date();
        date.setMinutes(date.getMinutes() + 60);

        user.resetPasswordToken = token;
        user.resetPasswordExpiresAt = date;

        await this.userRepository.save(user);

        // // send forgot password email
        await this.mailService.forgotPassword(user, host, token);
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
