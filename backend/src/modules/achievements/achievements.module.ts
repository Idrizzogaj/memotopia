import { Module } from '@nestjs/common';
import { PassportModule } from '@nestjs/passport';
import { TypeOrmModule } from '@nestjs/typeorm';

import { UserModule } from '~modules/user/user.module';

import { AchievementsService } from './achievements.service';
import { AchievementsController } from './achievements.controller';
import { AchievementRepository } from './achievements.repository';

@Module({
    imports: [
        TypeOrmModule.forFeature([AchievementRepository]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
        UserModule,
    ],
    providers: [AchievementsService],
    exports: [AchievementsService],
    controllers: [AchievementsController],
})
export class AchievementsModule {}
