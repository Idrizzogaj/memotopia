import { Module } from '@nestjs/common';
import { PassportModule } from '@nestjs/passport';
import { TypeOrmModule } from '@nestjs/typeorm';

import { UserStatisticsModule } from '~modules/user-statistics/user-statistics.module';
import { UserModule } from '~modules/user/user.module';

import { GameRepository } from './game.repository';
import { LevelController } from './level.controller';
import { LevelRepository } from './level.repository';
import { LevelService } from './level.service';

@Module({
    imports: [
        TypeOrmModule.forFeature([LevelRepository, GameRepository]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
        UserStatisticsModule,
        UserModule,
    ],
    controllers: [LevelController],
    providers: [LevelService],
})
export class LevelModule {}
