import { BadRequestException, Injectable, InternalServerErrorException } from '@nestjs/common';

import { Achievement } from '~common/db/entities/achievement.entity';
import { User } from '~common/db/entities/user.entity';
import { ConflictException } from '~common/exceptions';

import { AchievementRepository } from './achievements.repository';
import { AchievementsDto } from './dto/achievements.dto';
import { achievementConstants } from './achievements.constants';

@Injectable()
export class AchievementsService {
    constructor(private achievementRepository: AchievementRepository) {}

    async create(achievement: AchievementsDto, user: User): Promise<any> {
        const { achievements } = achievement;

        // Check for any invalid achievement
        for (const ach of achievements) {
            if (!achievementConstants.includes(ach))
                throw new BadRequestException(`${ach} is not a valid achievement.`);
        }

        // Add achievements
        for (const ach of achievements) {
            const achievement: Achievement = new Achievement();
            achievement.name = ach;
            achievement.user = user;

            try {
                await this.achievementRepository.save(achievement);
            } catch (error) {
                if (error.code == 23505)
                    throw new ConflictException('User already has this achievement.');

                throw new InternalServerErrorException(error);
            }
        }
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
