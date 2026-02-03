import {
    ConflictException,
    Injectable,
    InternalServerErrorException,
    NotFoundException,
} from '@nestjs/common';

import { Favorites } from '~common/db/entities/favorites.entity';
import { User } from '~common/db/entities/user.entity';
import { UserRepository } from '~modules/user/user.repository';
import { UserService } from '~modules/user/user.service';

import { FavoritesReqDto } from './dto/favorites-req.dto';
import { FavoritesRepository } from './favorites.repository';

@Injectable()
export class FavoritesService {
    constructor(
        private favoritesRepository: FavoritesRepository,
        private userRepository: UserRepository,
        private userService: UserService,
    ) {}

    async create(favoritesReqDto: FavoritesReqDto): Promise<{ msg: string }> {
        const { requester, favorites } = favoritesReqDto;

        if (requester == favorites)
            throw new ConflictException('You cannot make favorite yourself');

        const userRequester = await this.userService.getUserEntityById(requester);
        const userFavorites = await this.userService.getUserEntityById(favorites);
        const favoritesExists: Favorites = await this.favoritesRepository.findOne({
            requester: userRequester,
            favorites: userFavorites,
        });

        if (favoritesExists)
            throw new ConflictException(`You already favorite user with id ${favorites}`);

        const favoritesEntity: Favorites = new Favorites();
        favoritesEntity.requester = userRequester;
        favoritesEntity.favorites = userFavorites;

        try {
            await this.favoritesRepository.save(favoritesEntity);
            return { msg: 'Favorites created.' };
        } catch (error) {
            throw new InternalServerErrorException(error);
        }
    }

    async delete(favoritesReqDto: FavoritesReqDto): Promise<{ msg: string }> {
        const { requester, favorites } = favoritesReqDto;

        if (requester == favorites) throw new ConflictException('You cannot delete favor yourself');

        const userRequester = await this.userService.getUserEntityById(requester);
        const userFavorites = await this.userService.getUserEntityById(favorites);
        const favoritesExists: Favorites = await this.favoritesRepository.findOne({
            requester: userRequester,
            favorites: userFavorites,
        });

        if (!favoritesExists)
            throw new ConflictException(`You don\'t favor user with id ${favorites}`);

        try {
            await this.favoritesRepository.remove(favoritesExists);
            return { msg: 'Favorites deleted.' };
        } catch (error) {
            throw new InternalServerErrorException(error);
        }
    }

    async getFavorites(requester: number): Promise<User[]> {
        try {
            const userRequester = await this.userService.getUserEntityById(requester);

            if (!userRequester) throw new NotFoundException('User not found.');

            return await this.userRepository.getFavoriteUsers(requester);
        } catch (error) {
            throw new InternalServerErrorException(error);
        }
    }
}
