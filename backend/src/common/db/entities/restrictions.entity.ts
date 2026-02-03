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
export class Restrictions {
    @PrimaryGeneratedColumn()
    @ApiProperty()
    id: number;

    @Column('int')
    @ApiProperty({ description: 'Number Of Challenge Games', type: Number })
    numberOfChallengeGames: number;

    @Column('varchar')
    @ApiProperty({ description: 'Last Played Date', type: String })
    lastPlayedDate: string;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;

    @OneToOne(
        () => User,
        user => user.restrictions,
        { onDelete: 'CASCADE' },
    )
    @JoinColumn()
    user: User;
}
