import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddAvatarField1611956261535 implements MigrationInterface {
    name = 'AddAvatarField1611956261535';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "user" ADD "avatar" character varying NOT NULL DEFAULT 'MaskGroup54'`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(`ALTER TABLE "user" DROP COLUMN "avatar"`);
    }
}
