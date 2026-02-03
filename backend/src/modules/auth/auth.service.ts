import {
    Inject,
    Injectable,
    UnauthorizedException,
    HttpService,
    BadRequestException,
    InternalServerErrorException,
    ConflictException,
} from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import appleSignin, { AppleIdTokenType } from 'apple-signin-auth';
import * as bcrypt from 'bcryptjs';
import { sign } from 'jsonwebtoken';

import { UserProvider } from '~common/constants/enums/user-provider.enum';
import { aud, iss } from '~common/constants/social-medias.constants';
import { socialMediasReqUrls } from '~common/constants/urls/social-medias.urls';
import { User } from '~common/db/entities/user.entity';
import { RestrictionsService } from '~modules/restrictions/restrictions.service';
import { UserStatisticsService } from '~modules/user-statistics/user-statistics.service';
import { UserRepository } from '~modules/user/user.repository';

import { AuthCredentialsDto } from './dto/auth-credentials.dto';
import { JwtPayloadDto } from './dto/jwt-payload.dto';
import { SocialUserReqDto } from './dto/socialUser-req.dto';

@Injectable()
export class AuthService {
    constructor(
        @Inject(ConfigService) private configService: ConfigService,
        private userRepository: UserRepository,
        private httpService: HttpService,
        private userStatisticsService: UserStatisticsService,
        private restrictionsService: RestrictionsService,
    ) {}

    async signIn(
        authCredentialsDto: AuthCredentialsDto,
    ): Promise<{ accessToken: string; user: User }> {
        const user = await this.validateUserPassword(authCredentialsDto);

        if (!user) throw new UnauthorizedException('INVALID_CREDENTIALS');

        const accessToken: string = this.generateAccessToken(user);
        return { accessToken, user };
    }

    async validateUserPassword(authCredentialsDto: AuthCredentialsDto): Promise<User> {
        const { username, password } = authCredentialsDto;
        const regexEmail = /^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/;
        let user;

        if (username.match(regexEmail)) {
            user = await this.userRepository.findOne({
                where: { email: username },
                relations: ['userStatistics'],
            });
        } else {
            user = await this.userRepository.findOne({
                where: { username },
                relations: ['userStatistics'],
            });
        }

        if (user && (await bcrypt.compare(password, user.password))) {
            if (user.provider == UserProvider.MEMOTOPIA) return user;
            else throw new ConflictException('INVALID_PROVIDER');
        } else {
            return null;
        }
    }

    async validateUser(payload: JwtPayloadDto): Promise<User> {
        const user = await this.userRepository.findOne({
            where: { id: payload.userId },
        });
        return user;
    }

    async socialSignin(
        socialUserReqDto: SocialUserReqDto,
    ): Promise<{ accessToken: string; user: User }> {
        const { provider, userId, accessToken } = socialUserReqDto;
        let email = '';

        if (provider === UserProvider.FACEBOOK) {
            try {
                const fbRes = await this.httpService
                    .get(socialMediasReqUrls.facebook(userId, accessToken))
                    .toPromise();
                email = fbRes.data.email;
            } catch (err) {
                throw new BadRequestException('Invalid Token');
            }
        } else if (provider === UserProvider.APPLE) {
            try {
                const appleRes: AppleIdTokenType = await appleSignin.verifyIdToken(accessToken);

                if (appleRes.iss !== iss) throw new BadRequestException('Unexpected Issuer');
                if (appleRes.aud !== aud) throw new BadRequestException('Unexpected Audience');

                email = appleRes.email;
            } catch (err) {
                throw new BadRequestException('Invalid Token');
            }
        }

        const user = await this.userRepository.findOne({
            where: { email },
            relations: ['userStatistics'],
        });

        if (user) {
            const accessToken = this.generateAccessToken(user);
            return { accessToken, user };
        } else {
            const user = new User();
            let username = email.split('@')[0];

            const userExists = await this.userRepository.findOne({ username });

            if (userExists) {
                const randomString = Math.random()
                    .toString(36)
                    .substring(10);
                username = username + randomString;
            }

            user.username = username;
            user.email = email;
            user.provider = provider;

            try {
                const userCreated = await this.userRepository.save(user);
                await this.userStatisticsService.create({ level: 0, xp: 0, user: userCreated });
                await this.restrictionsService.create(userCreated);
                const accessToken = this.generateAccessToken(userCreated);
                const userToReturn = await this.userRepository.findOne({
                    where: { id: userCreated.id },
                    relations: ['userStatistics'],
                });

                return { accessToken, user: userToReturn };
            } catch (error) {
                throw new InternalServerErrorException();
            }
        }
    }

    generateAccessToken(user: User): string {
        const payload: JwtPayloadDto = {
            userId: user.id,
            username: user.username,
        };

        const JWT_SECRET = this.configService.get<string>('app.jwt.secret');
        const JWT_EXPIRES_IN = this.configService.get<string>('app.jwt.expiresIn');
        const accessToken: string = sign(payload, JWT_SECRET, {
            expiresIn: JWT_EXPIRES_IN,
        });

        return accessToken;
    }
}
