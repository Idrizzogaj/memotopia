import {
    bestResult,
    isValidResult,
    levelFromXp,
    xpForImprovement,
} from './level-rules';

describe('level XP rules', () => {
    it('gives 10 XP per new star, plus the level number the first time the level is cleared', () => {
        expect(xpForImprovement(5, 0, 1)).toBe(15);
        expect(xpForImprovement(5, 0, 3)).toBe(35);
        expect(xpForImprovement(7, 0, 3)).toBe(37);
        expect(xpForImprovement(15, 0, 2)).toBe(35);
        expect(xpForImprovement(5, 1, 3)).toBe(20); // improving: stars only, no level bonus again
        expect(xpForImprovement(5, 2, 3)).toBe(10);
    });

    it('gives nothing for a replay with the same or fewer stars', () => {
        expect(xpForImprovement(5, 3, 3)).toBe(0);
        expect(xpForImprovement(5, 3, 1)).toBe(0);
    });

    it('adds 200 XP the first time level 20 is cleared, whatever the stars', () => {
        expect(xpForImprovement(20, 0, 1)).toBe(10 + 20 + 200);
        expect(xpForImprovement(20, 0, 3)).toBe(30 + 20 + 200);
        expect(xpForImprovement(20, 1, 3)).toBe(20); // no second bonus
        expect(xpForImprovement(19, 0, 3)).toBe(30 + 19);
    });

    it('first clear of level 20 with 3 / 2 / 1 stars fills every level: 1010 / 810 / 610 XP', () => {
        const all = (stars: number) => {
            let total = 0;
            for (let level = 1; level <= 20; level++) total += xpForImprovement(level, 0, stars);
            return total;
        };
        expect(all(3)).toBe(600 + 210 + 200);
        expect(all(2)).toBe(400 + 210 + 200);
        expect(all(1)).toBe(200 + 210 + 200);
    });
});

describe('level result validation and merging', () => {
    it('rejects impossible input', () => {
        expect(isValidResult(5, 3, 120)).toBe(true);
        expect(isValidResult(0, 3, 120)).toBe(false);
        expect(isValidResult(21, 3, 120)).toBe(false);
        expect(isValidResult(5, 0, 120)).toBe(false);
        expect(isValidResult(5, 4, 120)).toBe(false);
        expect(isValidResult(5, 2.5, 120)).toBe(false);
        expect(isValidResult(5, 3, -1)).toBe(false);
        expect(isValidResult(5, 3, NaN)).toBe(false);
    });

    it('keeps the best stars and score, never lowers them', () => {
        expect(bestResult({ stars: 3, score: 150 }, { stars: 1, score: 40 })).toEqual({ stars: 3, score: 150 });
        expect(bestResult({ stars: 1, score: 40 }, { stars: 2, score: 100 })).toEqual({ stars: 2, score: 100 });
    });
});

describe('player level from XP (no upper limit)', () => {
    it('is a new level every 100 XP', () => {
        expect(levelFromXp(0, 1)).toBe(1);
        expect(levelFromXp(99, 1)).toBe(1);
        expect(levelFromXp(100, 1)).toBe(2);
        expect(levelFromXp(999, 1)).toBe(10);
        expect(levelFromXp(1000, 10)).toBe(11);
        expect(levelFromXp(2650, 10)).toBe(27);
    });

    it('keeps the stored level below 100 XP', () => {
        expect(levelFromXp(50, 0)).toBe(0);
    });
});
