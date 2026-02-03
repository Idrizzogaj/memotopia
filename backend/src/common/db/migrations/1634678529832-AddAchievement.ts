import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddAchievement1634678529832 implements MigrationInterface {
    name = 'AddAchievement1634678529832';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `CREATE TABLE "achievement" ("id" SERIAL NOT NULL, "name" character varying NOT NULL, "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), "userId" integer, CONSTRAINT "UQ_2d6649aca16a005e277e9eb875f" UNIQUE ("name", "userId"), CONSTRAINT "PK_441339f40e8ce717525a381671e" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `ALTER TABLE "achievement" ADD CONSTRAINT "FK_61ea514b7a1ee99bc55c310bac9" FOREIGN KEY ("userId") REFERENCES "user"("id") ON DELETE CASCADE ON UPDATE NO ACTION`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "achievement" DROP CONSTRAINT "FK_61ea514b7a1ee99bc55c310bac9"`,
        );
        await queryRunner.query(`DROP TABLE "achievement"`);
    }
}
