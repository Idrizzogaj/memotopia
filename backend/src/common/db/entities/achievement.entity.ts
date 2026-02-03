import { ApiProperty } from '@nestjs/swagger';
import {
    CreateDateColumn,
    Entity,
    Column,
    UpdateDateColumn,
    PrimaryGeneratedColumn,
    ManyToOne,
    Unique,
} from 'typeorm';

import { User } from './user.entity';

@Entity()
@Unique(['name', 'user'])
export class Achievement {
    @PrimaryGeneratedColumn()
    @ApiProperty()
    id: number;

    @Column('varchar', { nullable: false })
    @ApiProperty({ nullable: false })
    name: string;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;

    @ManyToOne(
        () => User,
        user => user.achievement,
        { onDelete: 'CASCADE' },
    )
    user: User;
}
