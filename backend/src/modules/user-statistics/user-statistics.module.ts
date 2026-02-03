import { Module } from '@nestjs/common';
import { PassportModule } from '@nestjs/passport';
import { TypeOrmModule } from '@nestjs/typeorm';

import { UserStatisticsRepository } from './user-statistics.repository';
import { UserStatisticsService } from './user-statistics.service';
import { UserStatisticsController } from './user-statistics.controller';

@Module({
    imports: [
        TypeOrmModule.forFeature([UserStatisticsRepository]),
        PassportModule.register({
            defaultStrategy: 'jwt',
        }),
    ],
    providers: [UserStatisticsService],
    exports: [UserStatisticsService],
    controllers: [UserStatisticsController],
})
export class UserStatisticsModule {}
