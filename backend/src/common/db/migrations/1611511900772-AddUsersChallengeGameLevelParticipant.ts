import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddUsersChallengeGameLevelParticipant1611511900772 implements MigrationInterface {
    name = 'AddUsersChallengeGameLevelParticipant1611511900772';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `CREATE TYPE "participant_status_enum" AS ENUM('DONE', 'PENDING', 'CANCELED')`,
        );
        await queryRunner.query(
            `CREATE TABLE "participant" ("id" SERIAL NOT NULL, "score" integer NOT NULL DEFAULT '0', "status" "participant_status_enum" NOT NULL DEFAULT 'PENDING', "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), "userId" integer, "challengeId" integer, CONSTRAINT "PK_64da4237f502041781ca15d4c41" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `CREATE TABLE "user" ("id" SERIAL NOT NULL, "email" character varying NOT NULL, "username" character varying NOT NULL, "provider" character varying NOT NULL, "password" character varying, "salt" character varying, "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), CONSTRAINT "UQ_e12875dfb3b1d92d7d7c5377e22" UNIQUE ("email"), CONSTRAINT "UQ_78a916df40e02a9deb1c4b75edb" UNIQUE ("username"), CONSTRAINT "PK_cace4a159ff9f2512dd42373760" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `CREATE TABLE "game_level" ("id" SERIAL NOT NULL, "stars" integer NOT NULL, "score" integer NOT NULL, "level" integer NOT NULL, "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), "userId" integer, "gameId" integer, CONSTRAINT "PK_15d428871f26360a39103b9d907" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `CREATE TABLE "game" ("id" SERIAL NOT NULL, "gameName" character varying NOT NULL, CONSTRAINT "UQ_da2419796cd8a0323f900fbc1c9" UNIQUE ("gameName"), CONSTRAINT "PK_352a30652cd352f552fef73dec5" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `CREATE TABLE "challenge" ("id" SERIAL NOT NULL, "level" integer NOT NULL, "gameId" integer, CONSTRAINT "PK_5f31455ad09ea6a836a06871b7a" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `ALTER TABLE "participant" ADD CONSTRAINT "FK_b915e97dea27ffd1e40c8003b3b" FOREIGN KEY ("userId") REFERENCES "user"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
        await queryRunner.query(
            `ALTER TABLE "participant" ADD CONSTRAINT "FK_20fe720408ad5ef2601960ec0b5" FOREIGN KEY ("challengeId") REFERENCES "challenge"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
        await queryRunner.query(
            `ALTER TABLE "game_level" ADD CONSTRAINT "FK_68f7f4ef92c445ce60131b6a7d2" FOREIGN KEY ("userId") REFERENCES "user"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
        await queryRunner.query(
            `ALTER TABLE "game_level" ADD CONSTRAINT "FK_a232898a18f0dcb3b34771efd89" FOREIGN KEY ("gameId") REFERENCES "game"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
        await queryRunner.query(
            `ALTER TABLE "challenge" ADD CONSTRAINT "FK_d6030c304ada48bdc154518b24d" FOREIGN KEY ("gameId") REFERENCES "game"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "challenge" DROP CONSTRAINT "FK_d6030c304ada48bdc154518b24d"`,
        );
        await queryRunner.query(
            `ALTER TABLE "game_level" DROP CONSTRAINT "FK_a232898a18f0dcb3b34771efd89"`,
        );
        await queryRunner.query(
            `ALTER TABLE "game_level" DROP CONSTRAINT "FK_68f7f4ef92c445ce60131b6a7d2"`,
        );
        await queryRunner.query(
            `ALTER TABLE "participant" DROP CONSTRAINT "FK_20fe720408ad5ef2601960ec0b5"`,
        );
        await queryRunner.query(
            `ALTER TABLE "participant" DROP CONSTRAINT "FK_b915e97dea27ffd1e40c8003b3b"`,
        );
        await queryRunner.query(`DROP TABLE "challenge"`);
        await queryRunner.query(`DROP TABLE "game"`);
        await queryRunner.query(`DROP TABLE "game_level"`);
        await queryRunner.query(`DROP TABLE "user"`);
        await queryRunner.query(`DROP TABLE "participant"`);
        await queryRunner.query(`DROP TYPE "participant_status_enum"`);
    }
}
