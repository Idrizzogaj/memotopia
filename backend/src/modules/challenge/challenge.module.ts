import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { PassportModule } from '@nestjs/passport';

import { GameRepository } from '~modules/level/game.repository';
import { UserRepository } from '~modules/user/user.repository';
import { AuthModule } from '~modules/auth/auth.module';
import { UserModule } from '~modules/user/user.module';
import { UserStatisticsModule } from '~modules/user-statistics/user-statistics.module';
import { AchievementsModule } from '~modules/achievements/achievements.module';

import { ChallengeService } from './challenge.service';
import { ChallengeController } from './challenge.controller';
import { ChallengeRepository } from './challenge.repository';
import { ParticipantRepository } from './participant.repository';
import { SocialService } from './social.service';
import { SocialController } from './social.controller';
import { SprintService } from './sprint.service';
import { SprintController } from './sprint.controller';

@Module({
    imports: [
        TypeOrmModule.forFeature([
            ChallengeRepository,
            ParticipantRepository,
            GameRepository,
            UserRepository,
        ]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
        AuthModule,
        UserModule,
        UserStatisticsModule,
        AchievementsModule,
    ],
    providers: [ChallengeService,SprintService,SocialService],
    controllers: [SocialController,ChallengeController,SprintController],
})
export class ChallengeModule {}
