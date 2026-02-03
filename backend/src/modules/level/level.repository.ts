import { EntityRepository, Repository } from 'typeorm';

import { GameLevel } from '~common/db/entities/game-level.entity';

@EntityRepository(GameLevel)
export class LevelRepository extends Repository<GameLevel> {}
