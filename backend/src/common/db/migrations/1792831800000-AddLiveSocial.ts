import { MigrationInterface, QueryRunner } from 'typeorm';
export class AddLiveSocial1792831800000 implements MigrationInterface {
    name='AddLiveSocial1792831800000';
    async up(q:QueryRunner):Promise<void>{
        await q.query(`CREATE TABLE IF NOT EXISTS player_social ("userId" integer PRIMARY KEY REFERENCES "user"(id) ON DELETE CASCADE,visible boolean NOT NULL DEFAULT false,"friendsVisible" boolean NOT NULL DEFAULT true,"onlineUntil" bigint NOT NULL DEFAULT 0,declined integer NOT NULL DEFAULT 0,"friendWindow" bigint NOT NULL DEFAULT 0,"friendRequests" integer NOT NULL DEFAULT 0)`);
        await q.query(`CREATE TABLE IF NOT EXISTS player_block ("ownerId" integer REFERENCES "user"(id) ON DELETE CASCADE,"targetId" integer REFERENCES "user"(id) ON DELETE CASCADE,PRIMARY KEY("ownerId","targetId"))`);
        await q.query(`CREATE TABLE IF NOT EXISTS player_friend ("lowId" integer REFERENCES "user"(id) ON DELETE CASCADE,"highId" integer REFERENCES "user"(id) ON DELETE CASCADE,"requesterId" integer NOT NULL,accepted boolean NOT NULL DEFAULT false,declined boolean NOT NULL DEFAULT false,"requestedAt" bigint NOT NULL,PRIMARY KEY("lowId","highId"))`);
    }
    async down(q:QueryRunner):Promise<void>{await q.query('DROP TABLE IF EXISTS player_friend,player_block,player_social');}
}
