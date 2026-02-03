import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddFavoritesTable1616022644149 implements MigrationInterface {
    name = 'AddFavoritesTable1616022644149';

    public async up(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `CREATE TABLE "favorites" ("id" SERIAL NOT NULL, "createdAt" TIMESTAMP NOT NULL DEFAULT now(), "updatedAt" TIMESTAMP NOT NULL DEFAULT now(), "requesterId" integer, "favoritesId" integer, CONSTRAINT "PK_890818d27523748dd36a4d1bdc8" PRIMARY KEY ("id"))`,
        );
        await queryRunner.query(
            `ALTER TABLE "favorites" ADD CONSTRAINT "FK_40da1e6bcb6aaf3bc42e9d9e1e5" FOREIGN KEY ("requesterId") REFERENCES "user"("id") ON DELETE NO ACTION ON UPDATE NO ACTION`,
        );
        await queryRunner.query(
            `ALTER TABLE "favorites" ADD CONSTRAINT "FK_c0fcb3f0a5955a9fd8182b41e6c" FOREIGN KEY ("favoritesId") REFERENCES "user"("id") ON DELETE NO ACTION ON UPDATE NO ACTION`,
        );
    }

    public async down(queryRunner: QueryRunner): Promise<void> {
        await queryRunner.query(
            `ALTER TABLE "favorites" DROP CONSTRAINT "FK_c0fcb3f0a5955a9fd8182b41e6c"`,
        );
        await queryRunner.query(
            `ALTER TABLE "favorites" DROP CONSTRAINT "FK_40da1e6bcb6aaf3bc42e9d9e1e5"`,
        );
        await queryRunner.query(`DROP TABLE "favorites"`);
    }
}
