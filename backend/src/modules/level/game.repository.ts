import { EntityRepository, Repository } from 'typeorm';

import { Game } from '~common/db/entities/game.entity';

@EntityRepository(Game)
export class GameRepository extends Repository<Game> {}
