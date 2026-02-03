import { EntityRepository, getRepository, Repository } from 'typeorm';

import { Participant } from '~common/db/entities/participant.entity';
import { User } from '~common/db/entities/user.entity';
import { Challenge } from '~common/db/entities/challenge.entity';
import { ParticipantStatus } from '~common/constants/enums/participant.enum';

import { GetChallengeDto } from './dto/get-challenge.dto';
import { PaginationChallengeDto } from './dto/pagination-challenge.dto';

@EntityRepository(Participant)
export class ParticipantRepository extends Repository<Participant> {
    async findChallenges(
        getChallengeDto: GetChallengeDto,
        user: User,
        myStatus: string,
        enemyStatus: string,
    ): Promise<PaginationChallengeDto> {
        const { page, limit } = getChallengeDto;
        const skippedItems = (page - 1) * limit;

        const query = this.createQueryBuilder('participant').leftJoinAndSelect(
            'participant.challenge',
            'challenge',
        );

        query.where('participant.status = :status', { status: myStatus });
        query.andWhere('participant.user = :userId', { userId: user.id });
        query
            .skip(skippedItems)
            .take(limit)
            .orderBy('participant.createdAt', 'DESC');

        const [results1, total1] = await query.getManyAndCount();

        const challenges = results1.map(i => i.challenge.id);

        const query1 = getRepository(Challenge)
            .createQueryBuilder('challenge')
            .innerJoinAndSelect('challenge.participants', 'participant');

        query1.leftJoinAndSelect('participant.user', 'user');
        query1.leftJoinAndSelect('user.userStatistics', 'userStatistics');
        query1.leftJoinAndSelect('challenge.game', 'game');
        query1.andWhere('challenge.id IN (:...ids)', { ids: challenges });

        if (myStatus == ParticipantStatus.DONE)
            query1.andWhere('participant.status = :status OR participant.status = :pendingStatus', {
                status: enemyStatus,
                pendingStatus: ParticipantStatus.PENDING,
            });
        else query1.andWhere('participant.status = :status', { status: enemyStatus });

        if (myStatus != enemyStatus)
            query1.andWhere('participant.user.id != :userId', { userId: user.id });

        query1.orderBy('participant.createdAt', 'DESC');

        const [results, total] = await query1.getManyAndCount();

        return {
            results,
            total,
            page,
            limit,
        };
    }
}
