import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddEmailVerification1791200000000 implements MigrationInterface {
    name = 'AddEmailVerification1791200000000';

    public async up(queryRunner: QueryRunner): Promise<void> {
        // Existing accounts get emailVerificationPending = false (usable). Only new registrations are gated.
        await queryRunner.query(
            `ALTER TABLE "user" ADD "emailVerificationPending" boolean NOT NULL DEFAULT false`,
        );
        await queryRunner.query(`ALTER TABLE "user" ADD "emailVerifiedAt" TIMESTAMP`);
        await queryRunner.query(
            `ALTER TABLE "user" ADD "emailVerificationTokenHash" character varying`,
        );
        await queryRunner.query(`ALTER TABLE "user" ADD "emailVerificationExpiresAt" TIMESTAMP`);
        await queryRunner.query(`ALTER TABLE "user" ADD "emailVerificationSentAt" TIMESTAMP`);
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "emailVerificationSentAt"`);
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "emailVerificationExpiresAt"`);
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "emailVerificationTokenHash"`);
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "emailVerifiedAt"`);
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "emailVerificationPending"`);
    }
}
