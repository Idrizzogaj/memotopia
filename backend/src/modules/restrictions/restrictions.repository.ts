import { EntityRepository, Repository } from 'typeorm';

import { Restrictions } from '~common/db/entities/restrictions.entity';

@EntityRepository(Restrictions)
export class RestrictionsRepository extends Repository<Restrictions> {}
