import { EntityRepository, Repository } from 'typeorm';

import { User } from '~common/db/entities/user.entity';

@EntityRepository(User)
export class UserRepository extends Repository<User> {
    async getMaxId(): Promise<number> {
        const query = this.createQueryBuilder('user').select('MAX(user.id)', 'maxId');
        const maxRank = await query.getRawOne();
        return maxRank.maxId;
    }

    async getFavoriteUsers(id: number): Promise<User[]> {
        return await this.createQueryBuilder('user')
            .select([
                'user.id',
                'user.username',
                'user.avatar',
                'userStatistics.xp',
                'userStatistics.level',
            ])
            .leftJoin('user.favorites', 'favorites')
            .leftJoin('user.userStatistics', 'userStatistics')
            .where('favorites.requester = :requesterId', {
                requesterId: id,
            })
            .getMany();
    }

    async getByUsernameCaseInsensitive(username: string): Promise<User[]> {
        return await this.createQueryBuilder('user')
            .where('lower(username) = :username', { username: `${username.toLowerCase()}` })
            .execute();
    }

    async getByEmailCaseInsensitive(email: string): Promise<User[]> {
        return await this.createQueryBuilder('user')
            .where('lower(email) = :email', { email: `${email.toLowerCase()}` })
            .execute();
    }

    async getUsersThatPlayedThisMonth(month: number, year: number): Promise<{userId: number}[]> {
        return await this.manager.query(`SELECT DISTINCT g."userId"
        FROM public.game_level as g
        inner join public.user_statistics as u
        on g."userId" = u."userId"
        where EXTRACT(MONTH FROM g."createdAt") = ${month} and EXTRACT(YEAR FROM g."createdAt") = ${year} and u.xp >= 400`);
    }
}
