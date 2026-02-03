import { Module } from '@nestjs/common';

import { configModuleFactory } from '~common/config';
import { dbModuleFactory } from '~common/db/db.module';
import { SentryModule } from '~common/sentry/sentry.module';
import { jwtModuleFactory } from '~modules/auth/jwt.module';

import { AppController } from './app.controller';
import { AppService } from './app.service';
import { UserModule } from './modules/user/user.module';
import { AuthModule } from './modules/auth/auth.module';
import { LevelModule } from './modules/level/level.module';
import { ChallengeModule } from './modules/challenge/challenge.module';
import { FavoritesModule } from './modules/favorites/favorites.module';
import { UserStatisticsModule } from './modules/user-statistics/user-statistics.module';
import { RestrictionsModule } from './modules/restrictions/restrictions.module';
import { MailModule } from './modules/mail/mail.module';
import { AchievementsModule } from './modules/achievements/achievements.module';

@Module({
    imports: [
        configModuleFactory(),
        dbModuleFactory(),
        jwtModuleFactory(),
        SentryModule,
        UserModule,
        AuthModule,
        LevelModule,
        ChallengeModule,
        FavoritesModule,
        UserStatisticsModule,
        RestrictionsModule,
        MailModule,
        AchievementsModule,
    ],
    controllers: [AppController],
    providers: [AppService],
})
export class AppModule {}
