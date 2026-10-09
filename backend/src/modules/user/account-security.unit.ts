import * as crypto from 'crypto';

import { ConflictException, ForbiddenException, UnauthorizedException } from '@nestjs/common';
import * as bcrypt from 'bcryptjs';

import { UserProvider } from '~common/constants/enums/user-provider.enum';
import { User } from '~common/db/entities/user.entity';
import { rateLimit, resetRateLimits } from '~common/rate-limit';
import { AuthService } from '~modules/auth/auth.service';

import { UserService } from './user.service';

function build(overrides: Record<string, unknown> = {}) {
    const saved: User[] = [];
    const deleted: number[] = [];
    const queries: Array<[string, unknown[]]> = [];
    const repo: any = {
        findByUsernameOrEmailCaseInsensitive: jest.fn().mockResolvedValue([]),
        findByEmailCaseInsensitive: jest.fn().mockResolvedValue(undefined),
        save: jest.fn(async (u: User) => {
            u.id = u.id || 7;
            saved.push(u);
            return u;
        }),
        delete: jest.fn(async (id: number) => deleted.push(id)),
        findOne: jest.fn(),
        manager: {
            transaction: jest.fn(async (fn: any) =>
                fn({
                    query: async (q: string, p: unknown[]) => queries.push([q, p]),
                    delete: async (_e: unknown, id: number) => deleted.push(id),
                }),
            ),
        },
        createQueryBuilder: jest.fn(),
        ...overrides,
    };
    const mail: any = {
        verifyEmail: jest.fn().mockResolvedValue(undefined),
        forgotPassword: jest.fn(),
    };
    const stats: any = { create: jest.fn().mockResolvedValue({}) };
    const restr: any = { create: jest.fn().mockResolvedValue({}) };
    const auth: any = { generateAccessToken: jest.fn(() => 'JWT') };
    const service = new UserService(repo, auth, stats, restr, mail);
    return { service, repo, mail, saved, deleted, queries, auth };
}

const newUser = { username: 'someuser', email: 'a@example.com', password: 'Passw0rdX' };

describe('registration requires e-mail verification', () => {
    it('creates a pending account, sends a mail and issues no token', async () => {
        const { service, mail, saved, auth } = build();
        const res = await service.create({ ...newUser });
        expect(res.verificationRequired).toBe(true);
        expect(res.accessToken).toBeUndefined();
        expect(auth.generateAccessToken).not.toHaveBeenCalled();
        expect(saved[0].emailVerificationPending).toBe(true);
        expect(mail.verifyEmail).toHaveBeenCalledTimes(1);
    });

    it('stores only the hash of the token that was mailed', async () => {
        const { service, mail, saved } = build();
        await service.create({ ...newUser });
        const token: string = mail.verifyEmail.mock.calls[0][1];
        expect(token).toMatch(/^[0-9a-f]{64}$/);
        const hash = crypto.createHash('sha256').update(token).digest('hex');
        expect(saved[saved.length - 1].emailVerificationTokenHash).toBe(hash);
        expect(saved[saved.length - 1].emailVerificationTokenHash).not.toBe(token);
    });

    it('still succeeds (so the user can resend) when the mail cannot be sent', async () => {
        const { service, mail } = build();
        mail.verifyEmail.mockRejectedValue(new Error('smtp down'));
        const res = await service.create({ ...newUser });
        expect(res.verificationRequired).toBe(true);
        expect(res.mailSent).toBe(false);
    });

    it('rejects a live duplicate but releases an expired, unconfirmed one', async () => {
        const live = { id: 1, emailVerificationPending: false } as User;
        const a = build({ findByUsernameOrEmailCaseInsensitive: jest.fn().mockResolvedValue([live]) });
        await expect(a.service.create({ ...newUser })).rejects.toBeInstanceOf(ConflictException);

        const stale = {
            id: 2,
            emailVerificationPending: true,
            emailVerificationSentAt: new Date(Date.now() - 25 * 3600 * 1000),
        } as User;
        const b = build({ findByUsernameOrEmailCaseInsensitive: jest.fn().mockResolvedValue([stale]) });
        const res = await b.service.create({ ...newUser });
        expect(b.deleted).toContain(2);
        expect(res.verificationRequired).toBe(true);

        const fresh = { ...stale, emailVerificationSentAt: new Date() } as User;
        const c = build({ findByUsernameOrEmailCaseInsensitive: jest.fn().mockResolvedValue([fresh]) });
        await expect(c.service.create({ ...newUser })).rejects.toBeInstanceOf(ConflictException);
    });
});

describe('verifyEmail', () => {
    function qb(affected: number) {
        const calls: any[] = [];
        const builder: any = {
            update: () => builder,
            set: (v: any) => (calls.push(v), builder),
            where: (w: string, p: any) => (calls.push({ w, p }), builder),
            execute: async () => ({ affected }),
        };
        return { builder, calls };
    }

    it('rejects malformed tokens without touching the database', async () => {
        const { service, repo } = build();
        expect(await service.verifyEmail('nope')).toBe(false);
        expect(await service.verifyEmail('')).toBe(false);
        expect(repo.createQueryBuilder).not.toHaveBeenCalled();
    });

    it('consumes a valid token once, matching on hash, expiry and pending flag', async () => {
        const token = 'ab'.repeat(32);
        const { builder, calls } = qb(1);
        const { service } = build({ createQueryBuilder: () => builder });
        expect(await service.verifyEmail(token)).toBe(true);
        const where = calls.find(c => c.w);
        expect(where.p.hash).toBe(crypto.createHash('sha256').update(token).digest('hex'));
        expect(where.w).toContain('emailVerificationExpiresAt');
        expect(where.w).toContain('emailVerificationPending');
        expect(calls[0].emailVerificationTokenHash).toBeNull();
    });

    it('returns false when nothing matched (used, expired or unknown)', async () => {
        const { builder } = qb(0);
        const { service } = build({ createQueryBuilder: () => builder });
        expect(await service.verifyEmail('cd'.repeat(32))).toBe(false);
    });
});

describe('resendVerification', () => {
    it('is silent for unknown addresses', async () => {
        const { service, mail } = build();
        await expect(service.resendVerification('x@example.com')).resolves.toBeUndefined();
        expect(mail.verifyEmail).not.toHaveBeenCalled();
    });

    it('does not mail already verified accounts and honours the cooldown', async () => {
        const verified = { id: 1, emailVerificationPending: false } as User;
        const a = build({ findByEmailCaseInsensitive: jest.fn().mockResolvedValue(verified) });
        await a.service.resendVerification('x@example.com');
        expect(a.mail.verifyEmail).not.toHaveBeenCalled();

        const justSent = { id: 2, emailVerificationPending: true, emailVerificationSentAt: new Date() } as User;
        const b = build({ findByEmailCaseInsensitive: jest.fn().mockResolvedValue(justSent) });
        await b.service.resendVerification('x@example.com');
        expect(b.mail.verifyEmail).not.toHaveBeenCalled();

        const old = {
            id: 3,
            emailVerificationPending: true,
            emailVerificationSentAt: new Date(Date.now() - 5 * 60 * 1000),
        } as User;
        const c = build({ findByEmailCaseInsensitive: jest.fn().mockResolvedValue(old) });
        await c.service.resendVerification('x@example.com');
        expect(c.mail.verifyEmail).toHaveBeenCalledTimes(1);
    });
});

describe('login and sessions for unverified accounts', () => {
    async function authFor(user: Partial<User>) {
        const repo: any = { findOne: jest.fn().mockResolvedValue(user) };
        const cfg: any = { get: (k: string) => (k.endsWith('expiresIn') ? '1h' : 'secret') };
        return new AuthService(cfg, repo, {} as any, {} as any, {} as any);
    }
    it('refuses sign-in with EMAIL_NOT_VERIFIED only after a correct password', async () => {
        const password = await bcrypt.hash('Passw0rdX', 4);
        const svc = await authFor({
            id: 1,
            username: 'u',
            password,
            provider: UserProvider.MEMOTOPIA,
            emailVerificationPending: true,
        });
        await expect(svc.signIn({ username: 'u', password: 'Passw0rdX' })).rejects.toBeInstanceOf(
            ForbiddenException,
        );
        await expect(svc.signIn({ username: 'u', password: 'wrong' })).rejects.toBeInstanceOf(
            UnauthorizedException,
        );
    });
    it('lets verified accounts in and gives pending accounts no session', async () => {
        const password = await bcrypt.hash('Passw0rdX', 4);
        const ok = await authFor({ id: 1, username: 'u', password, provider: UserProvider.MEMOTOPIA, emailVerificationPending: false });
        expect((await ok.signIn({ username: 'u', password: 'Passw0rdX' })).accessToken).toBeTruthy();
        const pending = await authFor({ id: 1, emailVerificationPending: true });
        expect(await pending.validateUser({ userId: 1, username: 'u' } as any)).toBeNull();
        const gone = await authFor(undefined);
        expect(await gone.validateUser({ userId: 1, username: 'u' } as any)).toBeUndefined();
    });
});

describe('deleteAccount', () => {
    it('requires the right password for e-mail accounts and deletes nothing otherwise', async () => {
        const password = await bcrypt.hash('Passw0rdX', 4);
        const user = { id: 5, provider: UserProvider.MEMOTOPIA, password } as User;
        const { service, deleted, queries, repo } = build();
        repo.findOne.mockResolvedValue(user);
        await expect(service.deleteAccount(5, { confirm: 'DELETE' })).rejects.toBeInstanceOf(UnauthorizedException);
        await expect(service.deleteAccount(5, { confirm: 'DELETE', password: 'bad' })).rejects.toBeInstanceOf(
            UnauthorizedException,
        );
        expect(deleted).toEqual([]);
        expect(queries).toEqual([]);
    });

    it('removes favorites in both directions and the user in one transaction', async () => {
        const password = await bcrypt.hash('Passw0rdX', 4);
        const { service, deleted, queries, repo } = build();
        repo.findOne.mockResolvedValue({ id: 5, provider: UserProvider.MEMOTOPIA, password });
        await service.deleteAccount(5, { confirm: 'DELETE', password: 'Passw0rdX' });
        expect(repo.manager.transaction).toHaveBeenCalledTimes(1);
        expect(queries[0][0]).toContain('"requesterId" = $1 OR "favoritesId" = $1');
        expect(queries[0][1]).toEqual([5]);
        expect(deleted).toEqual([5]);
    });

    it('lets a social-provider account delete with the confirmation word alone', async () => {
        const { service, deleted, repo } = build();
        repo.findOne.mockResolvedValue({ id: 9, provider: UserProvider.APPLE });
        await service.deleteAccount(9, { confirm: 'DELETE' });
        expect(deleted).toEqual([9]);
    });
});

describe('search is parameterised', () => {
    it('passes the term through the repository, not into SQL text', async () => {
        const { service, repo } = build();
        repo.searchByUsernamePrefix = jest.fn().mockResolvedValue([]);
        await service.search("x' OR 1=1 --", { id: 3 } as User);
        expect(repo.searchByUsernamePrefix).toHaveBeenCalledWith("x' OR 1=1 --", 3);
    });
});

describe('rate limiting', () => {
    beforeEach(() => resetRateLimits());
    it('blocks the call after the limit and counts keys separately', () => {
        for (let i = 0; i < 3; i++) rateLimit('k', 3, 1000);
        expect(() => rateLimit('k', 3, 1000)).toThrow('TOO_MANY_REQUESTS');
        expect(() => rateLimit('other', 3, 1000)).not.toThrow();
    });
    it('allows calls again after the window has passed', () => {
        const now = Date.now();
        const spy = jest.spyOn(Date, 'now');
        spy.mockReturnValue(now);
        for (let i = 0; i < 2; i++) rateLimit('w', 2, 1000);
        expect(() => rateLimit('w', 2, 1000)).toThrow();
        spy.mockReturnValue(now + 1500);
        expect(() => rateLimit('w', 2, 1000)).not.toThrow();
        spy.mockRestore();
    });
});
