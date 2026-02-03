import { EntityRepository, Repository } from 'typeorm';

import { Favorites } from '~common/db/entities/favorites.entity';

@EntityRepository(Favorites)
export class FavoritesRepository extends Repository<Favorites> {}
