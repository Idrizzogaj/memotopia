// Sprint v1 wire contract. Keep parity with ChallengeSprintRules.cs.
export const sprintGames = ['Pairs', 'Boxes', 'Flash', 'Chain'];
export interface SprintResult {
    completed: number; correct: number; turns: number; errors: number;
    roundMs: number[]; trace: { ms: number; units: number }[];
}
export function stageSize(game: string, stage: number): number {
    if (!sprintGames.includes(game) || !Number.isInteger(stage) || stage < 0 || stage > 1000) throw new Error('Invalid stage');
    return game === 'Pairs' ? stage === 0 ? 3 : stage === 1 ? 5 : stage + 5 : [5, 7, 9, 12, 15, 18, 20][Math.min(stage, 6)];
}
export function units(game: string, completed: number, correct: number): number {
    let total = correct; for (let i = 0; i < completed; i++) total += stageSize(game, i); return total;
}
export function validateResult(game: string, result: SprintResult): void {
    if (!result || !sprintGames.includes(game)) throw new Error('Invalid result');
    for (const value of [result.completed,result.correct,result.turns,result.errors]) {
        if (!Number.isInteger(value) || value < 0 || value > 20000) throw new Error('Invalid counter');
    }
    if (result.completed > 1000 || result.correct >= stageSize(game, result.completed)) throw new Error('Invalid progress');
    if (!Array.isArray(result.roundMs) || result.roundMs.length !== result.completed || result.roundMs.some(v => !Number.isInteger(v) || v < 0 || v > 90000) || result.roundMs.reduce((a,b) => a+b,0) > 90000) throw new Error('Invalid round times');
    const total = units(game, result.completed, result.correct);
    if (!Array.isArray(result.trace) || result.trace.length !== total || total > 20000) throw new Error('Invalid trace');
    let time = 0;
    result.trace.forEach((point, i) => {
        if (!point || !Number.isInteger(point.ms) || point.ms < time || point.ms >= 90000 || point.units !== i + 1) throw new Error('Invalid replay point');
        time = point.ms;
    });
    let completedUnits = 0, completedTime = 0;
    result.roundMs.forEach((duration, stage) => {
        completedUnits += stageSize(game, stage); completedTime += duration;
        if (result.trace[completedUnits-1].ms !== completedTime) throw new Error('Round/replay mismatch');
    });
    if (game === 'Pairs' && result.turns < total * 2) throw new Error('Missing card turns');
}
// Positive means a wins. Duration comparison starts with the latest completed round.
export function compareSprint(game: string, a: SprintResult, b: SprintResult): number {
    let difference = a.completed - b.completed || a.correct - b.correct;
    if (difference) return Math.sign(difference);
    if (game === 'Pairs') return Math.sign(b.turns - a.turns);
    difference = b.errors - a.errors; if (difference) return Math.sign(difference);
    for (let i = a.roundMs.length - 1; i >= 0; i--) {
        difference = b.roundMs[i] - a.roundMs[i]; if (difference) return Math.sign(difference);
    }
    return 0;
}
