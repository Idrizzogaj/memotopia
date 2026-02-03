import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddRestrictionsTable1622999409542 implements MigrationInterface {
    name = 'AddRestrictionsTable1622999409542';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `CREATE TABLE "restrictions" ("id" SERIAL NOT NULL, "numberOfChallengeGames" integer NOT NULL, "lastPlayedDate" character varying NOT NULL, "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), "userId" integer, CONSTRAINT "REL_ba813f7b8b1f08fe8f0f3c6aaa" UNIQUE ("userId"), CONSTRAINT "PK_f86a9a487b1348b0349f104154e" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `CREATE TYPE "user_paymentstatus_enum" AS ENUM('NONE', 'MONTHLY', 'YEARLY', 'LIFETIME')`,
        );
        await queryRunner.query(
            `ALTER TABLE "user" ADD "paymentStatus" "user_paymentstatus_enum" NOT NULL DEFAULT 'NONE'`,
        );
        await queryRunner.query(
            `ALTER TABLE "restrictions" ADD CONSTRAINT "FK_ba813f7b8b1f08fe8f0f3c6aaac" FOREIGN KEY ("userId") REFERENCES "user"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "restrictions" DROP CONSTRAINT "FK_ba813f7b8b1f08fe8f0f3c6aaac"`,
        );
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "paymentStatus"`);
        await queryRunner.query(`DROP TYPE "user_paymentstatus_enum"`);
        await queryRunner.query(`DROP TABLE "restrictions"`);
    }
}
