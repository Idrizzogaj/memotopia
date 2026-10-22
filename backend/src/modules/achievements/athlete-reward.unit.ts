import { BadRequestException } from '@nestjs/common';
import { AchievementsService } from './achievements.service';
import { AchievementRepository } from './achievements.repository';
import { User } from '~common/db/entities/user.entity';

describe('one-heart reward receipt', () => {
    const user = { id: 42 } as User;
    let manager: any, service: AchievementsService, prerequisite: jest.Mock;
    beforeEach(() => {
        prerequisite = jest.fn().mockResolvedValue({ level: 20, stars: 1 });
        const query: any = { getOne: prerequisite };
        for (const key of ['innerJoin', 'where', 'andWhere']) query[key] = jest.fn().mockReturnValue(query);
        manager = {
            findOne: jest.fn().mockResolvedValueOnce({ id: 8, xp: 250, level: 3 }).mockResolvedValue(null),
            create: jest.fn((_entity, data) => data), save: jest.fn().mockResolvedValue({}),
            increment: jest.fn().mockResolvedValue({}), update: jest.fn().mockResolvedValue({}),
            createQueryBuilder: jest.fn().mockReturnValue(query),
        };
        service = new AchievementsService(({ manager: { transaction: fn => fn(manager) } } as unknown) as AchievementRepository);
    });
    it('uses a single transaction and credits exactly500 without stars', async () => {
        await service.create({ achievements: ['athlete-one-heart-flash'] }, user);
        expect(prerequisite).toHaveBeenCalledTimes(1);
        expect(manager.increment).toHaveBeenCalledWith(expect.anything(), { id: 8 }, 'xp', 500);
        expect(manager.save.mock.calls[0][1]).toEqual({ name: 'athlete-one-heart-flash', user });
        expect(manager.update).toHaveBeenCalledWith(expect.anything(), 8, { level: 8 });
        expect(manager.findOne.mock.calls[0][1].lock).toEqual({ mode: 'pessimistic_write' });
    });
    it('retries and an already-owned receipt cannot credit XP twice', async () => {
        manager.findOne.mockReset().mockResolvedValueOnce({ id: 8, xp: 750, level: 8 }).mockResolvedValueOnce({ id: 19 });
        await service.create({ achievements: ['athlete-one-heart-flash'] }, user);
        expect(manager.increment).not.toHaveBeenCalled();expect(manager.save).not.toHaveBeenCalled();
    });
    it('refuses Boxes reward before20is synced as cleared', async () => {
        prerequisite.mockResolvedValue(null);
        await expect(service.create({ achievements: ['athlete-one-heart-boxes'] }, user)).rejects.toBeInstanceOf(BadRequestException);
        expect(manager.save).not.toHaveBeenCalled();expect(manager.increment).not.toHaveBeenCalled();
    });
    it('does not credit XP when receipt insertion fails', async () => {
        manager.save.mockRejectedValue(new Error('database unavailable'));
        await expect(service.create({ achievements: ['athlete-one-heart-chain'] }, user)).rejects.toThrow('database unavailable');
        expect(manager.increment).not.toHaveBeenCalled();
    });
});
