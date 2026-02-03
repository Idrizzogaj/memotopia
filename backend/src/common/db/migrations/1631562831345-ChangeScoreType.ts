import { MigrationInterface, QueryRunner } from 'typeorm';

export class ChangeScoreType1631562831345 implements MigrationInterface {
    name = 'ChangeScoreType1631562831345';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query('ALTER TABLE "participant" DROP COLUMN "score"');
        await queryRunner.query(
            'ALTER TABLE "participant" ADD "score" integer NOT NULL DEFAULT \'0\'',
        );
        await queryRunner.query('ALTER TABLE "participant" DROP COLUMN "score"');
        await queryRunner.query(
            'ALTER TABLE "participant" ADD "score" double precision NOT NULL DEFAULT \'0\'',
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query('ALTER TABLE "participant" DROP COLUMN "score"');
        await queryRunner.query(
            'ALTER TABLE "participant" ADD "score" integer NOT NULL DEFAULT \'0\'',
        );
        await queryRunner.query('ALTER TABLE "participant" DROP COLUMN "score"');
        await queryRunner.query(
            'ALTER TABLE "participant" ADD "score" double precision NOT NULL DEFAULT \'0\'',
        );
    }
}
