import { compareParticipants } from './challenge-result';

const p = (correct: number, secondsLeft: number) => correct * 1000 + secondsLeft;

describe('challenge result ordering', () => {
    it('most correct wins, even when both failed (no more draws for 0 vs 0)', () => {
        expect(compareParticipants({ score: 0, progress: p(5, 0) }, { score: 0, progress: p(3, 100) })).toBeGreaterThan(0);
        expect(compareParticipants({ score: 0, progress: p(2, 90) }, { score: 0, progress: p(7, 0) })).toBeLessThan(0);
    });

    it('a finished round beats a failed one', () => {
        expect(compareParticipants({ score: 12, progress: p(24, 12) }, { score: 0, progress: p(23, 250) })).toBeGreaterThan(0);
    });

    it('equal correct count: more time left (less time used) wins', () => {
        expect(compareParticipants({ score: 80, progress: p(24, 80) }, { score: 40, progress: p(24, 40) })).toBeGreaterThan(0);
        expect(compareParticipants({ score: 10, progress: p(9, 10) }, { score: 55, progress: p(9, 55) })).toBeLessThan(0);
    });

    it('only an identical result is a tie', () => {
        expect(compareParticipants({ score: 30, progress: p(24, 30) }, { score: 30, progress: p(24, 30) })).toBe(0);
    });

    it('falls back to the old score rule when an older app sent no progress', () => {
        expect(compareParticipants({ score: 50, progress: -1 }, { score: 20, progress: p(24, 20) })).toBeGreaterThan(0);
        expect(compareParticipants({ score: 50 }, { score: 50 })).toBe(0);
        expect(compareParticipants({ score: 10, progress: p(5, 10) }, { score: 70 })).toBeLessThan(0);
    });
});
