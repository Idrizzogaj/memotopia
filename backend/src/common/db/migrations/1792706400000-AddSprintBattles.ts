import { MigrationInterface, QueryRunner, Table, TableForeignKey, TableIndex } from 'typeorm';
export class AddSprintBattles1792706400000 implements MigrationInterface {
    name='AddSprintBattles1792706400000';
    async up(queryRunner:QueryRunner):Promise<void>{
        if(await queryRunner.hasTable('sprint_battle'))return;
        await queryRunner.createTable(new Table({name:'sprint_battle',columns:[
            {name:'id',type:'integer',isPrimary:true,isGenerated:true,generationStrategy:'increment'},
            {name:'ownerId',type:'integer'},{name:'recipientId',type:'integer'},
            {name:'ownerName',type:'varchar',default:"''"},{name:'recipientName',type:'varchar',default:"''"},
            {name:'state',type:'jsonb'},
            {name:'createdAt',type:'timestamp',default:'now()'},{name:'updatedAt',type:'timestamp',default:'now()'},
        ],foreignKeys:[
            new TableForeignKey({columnNames:['ownerId'],referencedTableName:'user',referencedColumnNames:['id'],onDelete:'CASCADE'}),
            new TableForeignKey({columnNames:['recipientId'],referencedTableName:'user',referencedColumnNames:['id'],onDelete:'CASCADE'}),
        ]}),true);
        await queryRunner.createIndex('sprint_battle',new TableIndex({name:'IDX_sprint_owner',columnNames:['ownerId']}));
        await queryRunner.createIndex('sprint_battle',new TableIndex({name:'IDX_sprint_recipient',columnNames:['recipientId']}));
    }
    async down(queryRunner:QueryRunner):Promise<void>{await queryRunner.dropTable('sprint_battle',true);}
}
