import { ApiProperty } from '@nestjs/swagger';
import {
    CreateDateColumn,
    Entity,
    Column,
    UpdateDateColumn,
    PrimaryGeneratedColumn,
    OneToOne,
    JoinColumn,
} from 'typeorm';

import { User } from './user.entity';

@Entity()
export class UserStatistics {
    @PrimaryGeneratedColumn()
    @ApiProperty()
    id: number;

    @Column('int')
    @ApiProperty({ description: 'Level', type: Number })
    level: number;

    @Column('int')
    @ApiProperty({ description: 'XP', type: Number })
    xp: number;

    @Column('int', { nullable: false, default: 0 })
    @ApiProperty({ description: 'Number of challenges', type: Number })
    numberOfChallenges: number;

    @Column('int', { nullable: false, default: 0 })
    @ApiProperty({ description: 'Number of win challenges', type: Number })
    numberOfWinChallenges: number;

    @Column('int', { nullable: false, default: 0 })
    @ApiProperty({ description: 'Number of loss challenges', type: Number })
    numberOfLossChallenges: number;

    @Column('int', { nullable: false, default: 0 })
    @ApiProperty({ description: 'Number of draw challenges', type: Number })
    numberOfDrawChallenges: number;

    @Column('float', { default: 0 })
    @ApiProperty({ description: 'Time Played' })
    timePlayed: number;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;

    @OneToOne(
        () => User,
        user => user.userStatistics,
        { onDelete: 'CASCADE' },
    )
    @JoinColumn()
    user: User;
}
