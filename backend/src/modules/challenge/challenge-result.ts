export interface Rated {
    score: number;
    progress?: number;
}

/**
 * Positive when `a` did better than `b`, negative when worse, 0 for a genuine tie.
 *
 * Newer apps send `progress` = correct answers * 1000 + seconds left (a finished round counts every
 * item as correct). The player who found the most wins; equal counts are decided by time left (less
 * time used). This also decides rounds where both players failed. Older apps send no progress (the
 * column holds -1); if either side has none, the previous rule applies: the higher score wins.
 */
export function compareParticipants(a: Rated, b: Rated): number {
    const pa = typeof a.progress === 'number' ? a.progress : -1;
    const pb = typeof b.progress === 'number' ? b.progress : -1;
    if (pa >= 0 && pb >= 0) return pa - pb;
    return a.score - b.score;
}
