import { EntityRepository, Repository } from 'typeorm';

import { UserStatistics } from '~common/db/entities/user-statistics.entity';

@EntityRepository(UserStatistics)
export class UserStatisticsRepository extends Repository<UserStatistics> {
    async increaseChallengeNumberStatitics(id: number): Promise<void> {
        await this.createQueryBuilder('userStatistics')
            .update()
            .set({ numberOfChallenges: () => '"numberOfChallenges" + 1' })
            .where('id = :id', { id })
            .execute();
    }

    async increaseChallengeWinsStatitics(id: number): Promise<void> {
        await this.createQueryBuilder('userStatistics')
            .update()
            .set({ numberOfWinChallenges: () => '"numberOfWinChallenges" + 1' })
            .where('id = :id', { id })
            .execute();
    }

    async increaseChallengeLossesStatitics(id: number): Promise<void> {
        await this.createQueryBuilder('userStatistics')
            .update()
            .set({ numberOfLossChallenges: () => '"numberOfLossChallenges" + 1' })
            .where('id = :id', { id })
            .execute();
    }

    async increaseChallengeDrawsStatitics(id: number): Promise<void> {
        await this.createQueryBuilder('userStatistics')
            .update()
            .set({ numberOfDrawChallenges: () => '"numberOfDrawChallenges" + 1' })
            .where('id = :id', { id })
            .execute();
    }

    async increaseTimePlayed(id: number, timePlayed: number): Promise<void> {
        await this.createQueryBuilder('userStatistics')
            .update()
            .set({ timePlayed: () => `"timePlayed" + ${timePlayed}` })
            .where('id = :id', { id })
            .execute();
    }
}
