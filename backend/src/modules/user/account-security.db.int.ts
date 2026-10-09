/**
 * Integration test against a THROWAWAY local Postgres (never production). Run with:
 *   DB_TEST_URL=postgres://postgres:test@127.0.0.1:55432/postgres jest --testRegex '.db.int.ts$'
 * It builds the schema from the entities (the app runs with synchronize: true) and exercises the
 * real SQL of verification, search and account deletion.
 */
import * as crypto from 'crypto';

import { createConnection, Connection } from 'typeorm';
import * as bcrypt from 'bcryptjs';

import { UserProvider } from '~common/constants/enums/user-provider.enum';
import { User } from '~common/db/entities/user.entity';
import { UserRepository } from './user.repository';
import { UserService } from './user.service';

const url = process.env.DB_TEST_URL;
const maybe = url ? describe : describe.skip;

maybe('account security against a real database', () => {
    let conn: Connection;
    let repo: UserRepository;
    let service: UserService;
    const sentTokens: string[] = [];

    beforeAll(async () => {
        conn = await createConnection({
            type: 'postgres',
            url,
            entities: [__dirname + '/../../common/db/entities/*.entity{.ts,.js}'],
            synchronize: true,
            dropSchema: true,
        });
        repo = conn.getCustomRepository(UserRepository);
        const mail: any = {
            verifyEmail: async (_u: User, t: string): Promise<void> => {
                sentTokens.push(t);
            },
            forgotPassword: jest.fn(),
        };
        const stats: any = { create: jest.fn().mockResolvedValue({}) };
        const restr: any = { create: jest.fn().mockResolvedValue({}) };
        service = new UserService(repo, { generateAccessToken: () => 'JWT' } as any, stats, restr, mail);
    });
    afterAll(async () => {
        await conn.close();
    });

    it('registers pending, verifies exactly once, and rejects reuse', async () => {
        const res = await service.create({ username: 'dbuser01', email: 'DB1@Example.com', password: 'Passw0rdX' });
        expect(res.verificationRequired).toBe(true);
        const before = await repo.findByEmailCaseInsensitive('db1@example.com');
        expect(before!.emailVerificationPending).toBe(true);
        expect(before!.emailVerificationTokenHash).toBe(
            crypto.createHash('sha256').update(sentTokens[0]).digest('hex'),
        );

        const results = await Promise.all([service.verifyEmail(sentTokens[0]), service.verifyEmail(sentTokens[0])]);
        expect(results.filter(Boolean).length).toBe(1); // atomic: only one concurrent request wins

        const after = await repo.findByEmailCaseInsensitive('db1@example.com');
        expect(after!.emailVerificationPending).toBe(false);
        expect(after!.emailVerifiedAt).toBeTruthy();
        expect(after!.emailVerificationTokenHash).toBeNull();
        expect(await service.verifyEmail(sentTokens[0])).toBe(false);
    });

    it('does not accept an expired token', async () => {
        await service.create({ username: 'dbuser02', email: 'db2@example.com', password: 'Passw0rdX' });
        const u = (await repo.findByEmailCaseInsensitive('db2@example.com'))!;
        u.emailVerificationExpiresAt = new Date(Date.now() - 1000);
        await repo.save(u);
        expect(await service.verifyEmail(sentTokens[sentTokens.length - 1])).toBe(false);
        expect((await repo.findByEmailCaseInsensitive('db2@example.com'))!.emailVerificationPending).toBe(true);
    });

    it('existing rows added before the feature default to usable accounts', async () => {
        await conn.query(
            `INSERT INTO "user" (email, username, provider, password, salt) VALUES ('old@example.com','olduser01','MEMOTOPIA','x','y')`,
        );
        const old = (await repo.findByEmailCaseInsensitive('old@example.com'))!;
        expect(old.emailVerificationPending).toBe(false);
    });

    it('search is injection-safe and escapes LIKE wildcards', async () => {
        const caller = (await repo.findByEmailCaseInsensitive('db1@example.com'))!;
        expect(await service.search("x' OR '1'='1", caller)).toEqual([]);
        expect(await service.search('%', caller)).toEqual([]);
        expect((await service.search('dbuser', caller)).map(u => u.username)).toEqual(['dbuser02']);
    });

    it('deletes an account with favorites in both directions and leaves others intact', async () => {
        const hash = await bcrypt.hash('Passw0rdX', 4);
        const a = await repo.save(Object.assign(new User(), { email: 'a@x.com', username: 'delete_me1', provider: UserProvider.MEMOTOPIA, password: hash, salt: 's' }));
        const b = await repo.save(Object.assign(new User(), { email: 'b@x.com', username: 'keep_me_01', provider: UserProvider.MEMOTOPIA, password: hash, salt: 's' }));
        await conn.query(`INSERT INTO "favorites" ("requesterId","favoritesId") VALUES ($1,$2),($2,$1)`, [a.id, b.id]);

        await expect(service.deleteAccount(a.id, { confirm: 'DELETE', password: 'wrong' })).rejects.toBeTruthy();
        expect(await repo.findOne(a.id)).toBeTruthy();

        await service.deleteAccount(a.id, { confirm: 'DELETE', password: 'Passw0rdX' });
        expect(await repo.findOne(a.id)).toBeUndefined();
        expect(await repo.findOne(b.id)).toBeTruthy();
        expect(await conn.query(`SELECT 1 FROM "favorites" WHERE "requesterId"=$1 OR "favoritesId"=$1`, [a.id])).toEqual([]);
    });

    it('releases an abandoned unconfirmed registration after its token expired', async () => {
        await service.create({ username: 'squatter1', email: 'victim@example.com', password: 'Passw0rdX' });
        await expect(
            service.create({ username: 'realowner', email: 'victim@example.com', password: 'Passw0rdX' }),
        ).rejects.toBeTruthy();
        const s = (await repo.findByEmailCaseInsensitive('victim@example.com'))!;
        s.emailVerificationSentAt = new Date(Date.now() - 25 * 3600 * 1000);
        await repo.save(s);
        const ok = await service.create({ username: 'realowner', email: 'victim@example.com', password: 'Passw0rdX' });
        expect(ok.verificationRequired).toBe(true);
    });
});
