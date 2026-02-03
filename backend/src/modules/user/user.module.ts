import { Module } from '@nestjs/common';
import { PassportModule } from '@nestjs/passport';
import { TypeOrmModule } from '@nestjs/typeorm';

import { AuthModule } from '~modules/auth/auth.module';
import { MailModule } from '~modules/mail/mail.module';
import { RestrictionsModule } from '~modules/restrictions/restrictions.module';
import { UserStatisticsModule } from '~modules/user-statistics/user-statistics.module';

import { UserController } from './user.controller';
import { UserRepository } from './user.repository';
import { UserService } from './user.service';

@Module({
    imports: [
        TypeOrmModule.forFeature([UserRepository]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
        AuthModule,
        UserStatisticsModule,
        RestrictionsModule,
        MailModule,
    ],
    controllers: [UserController],
    providers: [UserService],
    exports: [UserService],
})
export class UserModule {}
