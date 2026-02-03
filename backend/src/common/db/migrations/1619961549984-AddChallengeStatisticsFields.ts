import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddChallengeStatisticsFields1619961549984 implements MigrationInterface {
    name = 'AddChallengeStatisticsFields1619961549984';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "user_statistics" ADD "numberOfChallenges" integer NOT NULL DEFAULT '0'`,
        );
        await queryRunner.query(
            `ALTER TABLE "user_statistics" ADD "numberOfWinChallenges" integer NOT NULL DEFAULT '0'`,
        );
        await queryRunner.query(
            `ALTER TABLE "user_statistics" ADD "numberOfLossChallenges" integer NOT NULL DEFAULT '0'`,
        );
        await queryRunner.query(
            `ALTER TABLE "user_statistics" ADD "numberOfDrawChallenges" integer NOT NULL DEFAULT '0'`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "user_statistics" DROP COLUMN "numberOfDrawChallenges"`,
        );
        await queryRunner.query(
            `ALTER TABLE "user_statistics" DROP COLUMN "numberOfLossChallenges"`,
        );
        await queryRunner.query(
            `ALTER TABLE "user_statistics" DROP COLUMN "numberOfWinChallenges"`,
        );
        await queryRunner.query(`ALTER TABLE "user_statistics" DROP COLUMN "numberOfChallenges"`);
    }
}
