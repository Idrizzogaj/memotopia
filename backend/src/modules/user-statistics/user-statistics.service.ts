import { Injectable, InternalServerErrorException, NotFoundException } from '@nestjs/common';

import { ChallengeStatus } from '~common/constants/enums/challenge-status.enum';
import { UserStatistics } from '~common/db/entities/user-statistics.entity';
import { User } from '~common/db/entities/user.entity';
import { levelFromXp } from '~modules/level/level-rules';

import { TimePlayedDto } from './dto/time-played.dto';
import { UserStatisticsDto } from './dto/user-statistics.dto';
import { UserStatisticsRepository } from './user-statistics.repository';

@Injectable()
export class UserStatisticsService {
    constructor(private userStatisticsRepository: UserStatisticsRepository) {}

    async create(userStatisticsDto: UserStatisticsDto): Promise<UserStatistics> {
        const { level, xp, user } = userStatisticsDto;
        const userStatistics = new UserStatistics();
        userStatistics.level = level;
        userStatistics.xp = xp;
        userStatistics.user = user;

        try {
            return await this.userStatisticsRepository.save(userStatistics);
        } catch (error) {
            throw new InternalServerErrorException('Statistics cannot be created');
        }
    }

    async update(userStatisticsDto: UserStatisticsDto): Promise<UserStatistics> {
        const { level, xp, user } = userStatisticsDto;
        const userStatistics: UserStatistics = await this.getByUser(user);

        userStatistics.level = level;
        userStatistics.xp = xp;
        userStatistics.user = user;

        try {
            return await this.userStatisticsRepository.save(userStatistics);
        } catch (error) {
            throw new InternalServerErrorException('Statistics cannot be updated');
        }
    }

    async getByUser(user: User): Promise<UserStatistics> {
        const userStatistics: UserStatistics = await this.userStatisticsRepository.findOne({
            user,
        });
        if (!userStatistics) throw new NotFoundException(`Statistics with id ${user.id} not found`);

        return userStatistics;
    }

    async getGlobalScore(): Promise<UserStatisticsDto[]> {
        const userStatistics: UserStatistics[] = await this.userStatisticsRepository.find({
            relations: ['user'],
            order: { xp: 'DESC' },
        });
        if (!userStatistics) throw new NotFoundException('No user statistics available');

        return userStatistics;
    }

    async levelUp(userStatistics: UserStatistics): Promise<UserStatistics> {
        // a new player level every 100 XP, no upper limit (XP is the world ranking)
        userStatistics.level = levelFromXp(userStatistics.xp, userStatistics.level);

        return userStatistics;
    }

    async increseStatisticValues(id: number, status: string): Promise<void> {
        await this.userStatisticsRepository.increaseChallengeNumberStatitics(id);

        if (status == ChallengeStatus.WON)
            await this.userStatisticsRepository.increaseChallengeWinsStatitics(id);
        else if (status == ChallengeStatus.LOSS)
            await this.userStatisticsRepository.increaseChallengeLossesStatitics(id);
        else if (status == ChallengeStatus.DRAW)
            await this.userStatisticsRepository.increaseChallengeDrawsStatitics(id);
    }

    async getTimePlayed(user: User): Promise<TimePlayedDto> {
        const userStats: UserStatistics = await this.getByUser(user);
        return { timePlayed: userStats.timePlayed }
    }

    async postTimePlayed(user: User, timePlayed: number): Promise<{msg: string}> {
        const userStats: UserStatistics = await this.getByUser(user);        
        await this.userStatisticsRepository.increaseTimePlayed(userStats.id, timePlayed);
        return { 'msg': 'time played updated' }
    }
}
