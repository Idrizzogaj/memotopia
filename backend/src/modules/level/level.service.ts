import {
    BadRequestException,
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
import { bestResult, isValidResult, xpForImprovement } from './level-rules';

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

        if (!isValidResult(level, stars, score)) throw new BadRequestException('Invalid level result');

        const game = await this.gameRepository.findOne({ gameName });

        if (!game) throw new NotFoundException(`Game with name "${gameName}" not found!`);

        // A result that was sent twice (for example after a bad connection) must never be paid twice:
        // when this level already has a record, merge into it instead of adding a second one.
        const existing = await this.levelRepository.findOne({ where: { user, game, level } });
        if (existing) return await this.applyResult(existing, stars, score, user);

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

        await this.addXp(user, xpForImprovement(level, 0, stars));

        return await this.userService.getUserEntityById(user.id);
    }

    /** Adds XP and recalculates the player level (no upper limit). */
    private async addXp(user: User, xp: number): Promise<void> {
        if (xp <= 0) return;

        let userStatistics: UserStatistics = await this.userStatisticsService.getByUser(user);
        userStatistics.xp += xp;
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

    /** Stores the better of the old and the new result and pays XP for the improvement only. */
    private async applyResult(gameLevel: GameLevel, stars: number, score: number, user: User): Promise<User> {
        const oldStars = gameLevel.stars;
        const best = bestResult(gameLevel, { stars, score });

        if (best.stars !== gameLevel.stars || best.score !== gameLevel.score) {
            gameLevel.stars = best.stars;
            gameLevel.score = best.score;

            try {
                await this.levelRepository.save(gameLevel);
            } catch (error) {
                throw new InternalServerErrorException('Game level failed to be updated');
            }
        }

        await this.addXp(user, xpForImprovement(gameLevel.level, oldStars, best.stars));

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

        if (!isValidResult(gameLevel.level, stars, score)) throw new BadRequestException('Invalid level result');

        return await this.applyResult(gameLevel, stars, score, user);
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
