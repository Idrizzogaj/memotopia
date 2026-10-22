import { BadRequestException, Injectable, InternalServerErrorException } from '@nestjs/common';

import { Achievement } from '~common/db/entities/achievement.entity';
import { User } from '~common/db/entities/user.entity';
import { UserStatistics } from '~common/db/entities/user-statistics.entity';
import { levelFromXp } from '~modules/level/level-rules';
import { GameLevel } from '~common/db/entities/game-level.entity';

import { AchievementRepository } from './achievements.repository';
import { AchievementsDto } from './dto/achievements.dto';
import { achievementConstants } from './achievements.constants';

@Injectable()
export class AchievementsService {
    constructor(private achievementRepository: AchievementRepository) {}

    async countForProfile(userId: number): Promise<{ count: number }> {
        const result = await this.achievementRepository.createQueryBuilder('achievement')
            .select('COUNT(DISTINCT achievement.name)', 'count')
            .where('achievement.userId = :userId', { userId }).getRawOne();
        return { count: Number(result.count) };
    }

    async create(achievement: AchievementsDto, user: User): Promise<any> {
        const { achievements } = achievement;

        // Check for any invalid achievement
        for (const ach of achievements) {
            if (!achievementConstants.includes(ach))
                throw new BadRequestException(`${ach} is not a valid achievement.`);
        }

        // Add achievements
        for (const ach of achievements) {
            if (ach.startsWith('athlete-one-heart-')) {
                await this.awardAthlete(ach, user);
                continue;
            }
            const achievement: Achievement = new Achievement();
            achievement.name = ach;
            achievement.user = user;

            try {
                await this.achievementRepository.save(achievement);
            } catch (error) {
                // The client and challenge completion can award the same badge.
                // A retry must not fail the match or skip subsequent achievements.
                if (error.code == '23505') continue;

                throw new InternalServerErrorException(error);
            }
        }
    }

    // Existing unique(name,user) is the durable reward receipt. Badge + XP commit together.
    // Chain has no server game records yet; its completion is client-attested like legacy play.
    private async awardAthlete(name: string, user: User): Promise<void> {
        await this.achievementRepository.manager.transaction(async manager => {
            const stats = await manager.findOne(UserStatistics, {
                where: { user }, lock: { mode: 'pessimistic_write' },
            });
            if (!stats) throw new BadRequestException('User statistics missing.');
            if (await manager.findOne(Achievement, { where: { name, user } })) return;
            const game = name.endsWith('-boxes') ? 'Boxes' : name.endsWith('-flash') ? 'Flash' : null;
            if (game) {
                const prerequisite = await manager.createQueryBuilder(GameLevel, 'level')
                    .innerJoin('level.game', 'game').where('level.userId = :userId', { userId: user.id })
                    .andWhere('level.level = 20 AND level.stars > 0')
                    .andWhere('game.gameName = :game', { game }).getOne();
                if (!prerequisite) throw new BadRequestException('Complete level 20 first; pending progress must sync.');
            }
            await manager.save(Achievement, manager.create(Achievement, { name, user }));
            // Increment only the reward fields; preserve concurrent counters and play time.
            await manager.increment(UserStatistics, { id: stats.id }, 'xp', 500);
            const level = levelFromXp(stats.xp + 500, stats.level);
            await manager.update(UserStatistics, stats.id, { level });
        });
    }

    async find(user: User): Promise<string[]> {
        const achievements = await this.achievementRepository.find({ user });
        const achievementsRes = [];

        for (const achievement of achievements) {
            achievementsRes.push(achievement.name);
        }

        return achievementsRes;
    }
}
