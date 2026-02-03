import { EntityRepository, Repository } from 'typeorm';

import { Challenge } from '~common/db/entities/challenge.entity';

@EntityRepository(Challenge)
export class ChallengeRepository extends Repository<Challenge> {}
