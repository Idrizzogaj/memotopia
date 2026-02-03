import {
    ForbiddenException,
    Injectable,
    InternalServerErrorException,
    NotFoundException,
} from '@nestjs/common';

import { GameLevel } from '~common/db/entities/game-level.entity';
import { UserStatistics } from '~common/db/entities/user-statistics.entity';
import { User } from '~common/db/entities/user.entity';
import { UserStatisticsService } from '~modules/user-statistics/user-statistics.service';
import { UserService } from '~modules/user/user.service';

import { LevelReqDto } from './dto/level-req.dto';
import { LevelResDto } from './dto/level-res.dto';
import { StarsAvgDto } from './dto/stars-avg.dto';
import { UpdateLevelReqDto } from './dto/update-level-req.dto';
import { GameRepository } from './game.repository';
import { LevelRepository } from './level.repository';

@Injectable()
export class LevelService {
    constructor(
        private levelRepository: LevelRepository,
        private gameRepository: GameRepository,
        private userStatisticsService: UserStatisticsService,
        private userService: UserService,
    ) {}

    async create(levelReqDto: LevelReqDto, user: User): Promise<User> {
        const { score, level, stars, gameName } = levelReqDto;
        const game = await this.gameRepository.findOne({ gameName });

        if (!game) throw new NotFoundException(`Game with name "${gameName}" not found!`);

        const gameLevel = new GameLevel();
        gameLevel.score = score;
        gameLevel.level = level;
        gameLevel.stars = stars;
        gameLevel.user = user;
        gameLevel.game = game;

        try {
            await this.levelRepository.save(gameLevel);
        } catch (error) {
            throw new InternalServerErrorException('Game level failed to be created');
        }

        let userStatistics: UserStatistics = await this.userStatisticsService.getByUser(user);

        switch (stars) {
            case 1:
                userStatistics.xp += 10;
                break;
            case 2:
                userStatistics.xp += 20;
                break;
            case 3:
                userStatistics.xp += 30;
                break;
        }

        userStatistics = await this.userStatisticsService.levelUp(userStatistics);

        try {
            await this.userStatisticsService.update({
                level: userStatistics.level,
                xp: userStatistics.xp,
                user,
            });
        } catch (error) {
            throw new InternalServerErrorException('User statistics failed to be updated');
        }

        return await this.userService.getUserEntityById(user.id);
    }

    async findAll(gameName: string, user: User): Promise<LevelResDto[]> {
        const game = await this.gameRepository.findOne({ gameName });

        if (!game) throw new NotFoundException(`Game with name "${gameName}" not found!`);

        return await this.levelRepository.find({ where: { user, game }, order: { level: 'ASC' } });
    }

    async update(id: string, user: User, updateLevel: UpdateLevelReqDto): Promise<User> {
        const gameLevel = await this.levelRepository.findOne(id, {
            relations: ['user'],
        });

        if (!gameLevel) throw new NotFoundException(`Level with id "${id}" not found!`);

        if (gameLevel.user.id != user.id)
            throw new ForbiddenException('You are not allowed to update this level.');

        const { score, stars } = updateLevel;

        if (gameLevel.stars < stars) {
            const starsDifference = stars - gameLevel.stars;

            let userStatistics: UserStatistics = await this.userStatisticsService.getByUser(user);

            switch (starsDifference) {
                case 1:
                    userStatistics.xp += 10;
                    break;
                case 2:
                    userStatistics.xp += 20;
                    break;
            }

            userStatistics = await this.userStatisticsService.levelUp(userStatistics);

            try {
                await this.userStatisticsService.update({
                    level: userStatistics.level,
                    xp: userStatistics.xp,
                    user,
                });
            } catch (error) {
                throw new InternalServerErrorException('User statistics failed to be updated');
            }
        }

        gameLevel.score = score;
        gameLevel.stars = stars;

        try {
            await this.levelRepository.save(gameLevel);
        } catch (error) {
            throw new InternalServerErrorException('Game level failed to be updated');
        }

        return await this.userService.getUserEntityById(user.id);
    }

    async getStarsAvg(user: User): Promise<StarsAvgDto> {
        const levels: GameLevel[] =  await this.levelRepository.find({user});
        let returnValue = 0;
        let sum = 0;

        if (levels.length > 0) {
            for (const level of levels) {
                sum += level.stars;
            }

            returnValue = sum/levels.length;
        }

        return {'avg': Math.round(returnValue * 100) / 100};
    }
}
