import { HttpException, HttpStatus } from '@nestjs/common';

/**
 * Minimal in-memory sliding-window limiter. The API runs as a single process behind Caddy, so a
 * process-local store is sufficient. Counters reset on restart, which is acceptable here.
 */
const hits = new Map<string, number[]>();

export function rateLimit(key: string, limit: number, windowMs: number): void {
    const now = Date.now();
    const recent = (hits.get(key) || []).filter(t => now - t < windowMs);
    if (recent.length >= limit) {
        hits.set(key, recent);
        throw new HttpException('TOO_MANY_REQUESTS', HttpStatus.TOO_MANY_REQUESTS);
    }
    recent.push(now);
    hits.set(key, recent);

    if (hits.size > 50000) {
        for (const [k, v] of hits) {
            if (v.every(t => now - t >= windowMs)) hits.delete(k);
        }
    }
}

export function resetRateLimits(): void {
    hits.clear();
}
