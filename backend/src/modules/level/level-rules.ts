/** Pure rules for level results and experience points (XP), kept free of the database so they can be unit tested. */

export const LAST_LEVEL = 20;
export const LAST_LEVEL_BONUS_XP = 200;
export const XP_PER_STAR = 10;
export const MAX_STARS = 3;

export function clampStars(stars: number): number {
    if (!Number.isFinite(stars)) return 0;
    return Math.max(0, Math.min(MAX_STARS, Math.floor(stars)));
}

/** True when a level result is acceptable input from a client. */
export function isValidResult(level: number, stars: number, score: number): boolean {
    return (
        Number.isInteger(level) &&
        level >= 1 &&
        level <= LAST_LEVEL &&
        Number.isInteger(stars) &&
        stars >= 1 &&
        stars <= MAX_STARS &&
        Number.isFinite(score) &&
        score >= 0 &&
        score <= 1000
    );
}

/**
 * XP for improving a level: 10 per extra star. The first time a level is cleared it also pays the level number
 * (3 stars on level 7 = 37), and the first time the last level (20) is cleared there is a one-off bonus, whatever
 * the number of stars. oldStars is 0 when there was no record before.
 */
export function xpForImprovement(level: number, oldStars: number, newStars: number): number {
    const before = clampStars(oldStars);
    const after = clampStars(newStars);
    const gained = Math.max(0, after - before) * XP_PER_STAR;
    const firstClear = before === 0 && after > 0;
    const levelBonus = firstClear ? level : 0;
    const topBonus = level === LAST_LEVEL && firstClear ? LAST_LEVEL_BONUS_XP : 0;
    return gained + levelBonus + topBonus;
}

/** The best of the stored and the new result: a replay never lowers stars or score. */
export function bestResult(
    stored: { stars: number; score: number },
    next: { stars: number; score: number },
): { stars: number; score: number } {
    return { stars: Math.max(stored.stars, next.stars), score: Math.max(stored.score, next.score) };
}

/**
 * Player level from XP, without an upper limit: a new level every 100 XP (100 XP = level 2).
 * Below 100 XP the stored level is kept (new accounts start at 0 or 1).
 */
export function levelFromXp(xp: number, currentLevel: number): number {
    if (!Number.isFinite(xp) || xp < 100) return currentLevel;
    return Math.floor(xp / 100) + 1;
}
