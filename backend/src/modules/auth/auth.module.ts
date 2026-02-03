import { HttpModule, Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';

import { RestrictionsModule } from '~modules/restrictions/restrictions.module';
import { UserStatisticsModule } from '~modules/user-statistics/user-statistics.module';
import { UserRepository } from '~modules/user/user.repository';

import { AuthController } from './auth.controller';
import { AuthService } from './auth.service';
import { JwtStrategy } from './jwt.strategy';

@Module({
    imports: [
        TypeOrmModule.forFeature([UserRepository]),
        HttpModule,
        UserStatisticsModule,
        RestrictionsModule,
    ],
    controllers: [AuthController],
    providers: [AuthService, JwtStrategy],
    exports: [AuthService],
})
export class AuthModule {}
