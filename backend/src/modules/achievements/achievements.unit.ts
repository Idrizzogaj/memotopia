import { BadRequestException, InternalServerErrorException } from '@nestjs/common';
import { AchievementsService } from './achievements.service';
import { AchievementRepository } from './achievements.repository';
import { User } from '~common/db/entities/user.entity';

describe('achievement retries', () => {
    const user = { id: 42 } as User;
    let save: jest.Mock;
    let service: AchievementsService;
    beforeEach(() => {
        save = jest.fn().mockResolvedValue({});
        service = new AchievementsService(({ save } as unknown) as AchievementRepository);
    });
    it('continues after an already-owned achievement', async () => {
        save.mockRejectedValueOnce({ code: '23505' });
        await service.create({ achievements: ['first-challenge', 'win-ten-challenge'] }, user);
        expect(save).toHaveBeenCalledTimes(2);
        expect(save.mock.calls[1][0].name).toBe('win-ten-challenge');
    });
    it('validates the whole batch before saving', async () => {
        await expect(service.create({ achievements: ['first-challenge', 'invalid'] }, user)).rejects.toBeInstanceOf(BadRequestException);
        expect(save).not.toHaveBeenCalled();
    });
    it('does not hide other database failures', async () => {
        save.mockRejectedValueOnce({ code: '08006' });
        await expect(service.create({ achievements: ['first-challenge'] }, user)).rejects.toBeInstanceOf(InternalServerErrorException);
    });
});
