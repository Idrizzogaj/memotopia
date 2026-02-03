import { Injectable, InternalServerErrorException, NotFoundException } from '@nestjs/common';

import { Restrictions } from '~common/db/entities/restrictions.entity';
import { User } from '~common/db/entities/user.entity';

import { RestrictionsDto } from './dto/restrictions.dto';
import { RestrictionsRepository } from './restrictions.repository';

@Injectable()
export class RestrictionsService {
    constructor(private restrictionsRepository: RestrictionsRepository) {}

    async create(user: User): Promise<void> {
        const restrictions: Restrictions = new Restrictions();
        restrictions.numberOfChallengeGames = 3;
        restrictions.lastPlayedDate = new Date().toISOString();
        restrictions.user = user;

        try {
            await this.restrictionsRepository.save(restrictions);
        } catch (error) {
            throw new InternalServerErrorException(error);
        }
    }

    async update(user: User, restrictionsDto: RestrictionsDto): Promise<Restrictions> {
        const { currentDate } = restrictionsDto;
        const restriction = await this.find(user, currentDate);

        if (restriction.numberOfChallengeGames > 0) {
            restriction.numberOfChallengeGames -= 1;
            restriction.lastPlayedDate = currentDate;
        }

        await this.restrictionsRepository.save(restriction);
        return restriction;
    }

    async find(user: User, currentDate: string): Promise<Restrictions> {
        const restriction = await this.restrictionsRepository.findOne({ user });
        if (!restriction)
            throw new NotFoundException(`Restriction with userid "${user.id}" not found!`);
        await this.refreshNrOfChallengeGames(user, currentDate, restriction);
        return restriction;
    }

    async refreshNrOfChallengeGames(
        user: User,
        currentDate: string,
        restriction: Restrictions,
    ): Promise<Restrictions> {
        const lastPlayedDate = new Date(restriction.lastPlayedDate);
        const comingDate = new Date(currentDate);
        const lastPlayedDateDay = lastPlayedDate.getTime() / (1000 * 3600 * 24);
        const comingDateDay = comingDate.getTime() / (1000 * 3600 * 24);

        if (comingDateDay - lastPlayedDateDay >= 1) {
            restriction.numberOfChallengeGames = 3;
            restriction.lastPlayedDate = currentDate;
        }

        await this.restrictionsRepository.save(restriction);
        return restriction;
    }
}
