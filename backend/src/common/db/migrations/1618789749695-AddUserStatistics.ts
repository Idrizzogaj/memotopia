import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddUserStatistics1618789749695 implements MigrationInterface {
    name = 'AddUserStatistics1618789749695';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `CREATE TABLE "user_statistics" ("id" SERIAL NOT NULL, "level" integer NOT NULL, "xp" integer NOT NULL, "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), "userId" integer, CONSTRAINT "REL_163d3173e678c93fd100a33797" UNIQUE ("userId"), CONSTRAINT "PK_fa0c135e6eb8b7d6507c0461b61" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `ALTER TABLE "user_statistics" ADD CONSTRAINT "FK_163d3173e678c93fd100a337976" FOREIGN KEY ("userId") REFERENCES "user"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "user_statistics" DROP CONSTRAINT "FK_163d3173e678c93fd100a337976"`,
        );
        await queryRunner.query(`DROP TABLE "user_statistics"`);
    }
}
