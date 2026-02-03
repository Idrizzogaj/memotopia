import { ApiProperty } from '@nestjs/swagger';
import {
    CreateDateColumn,
    Entity,
    Column,
    UpdateDateColumn,
    PrimaryGeneratedColumn,
    ManyToOne,
} from 'typeorm';

import { Game } from './game.entity';
import { User } from './user.entity';

@Entity()
export class GameLevel {
    @PrimaryGeneratedColumn()
    @ApiProperty()
    id: number;

    @Column('int')
    @ApiProperty({ description: 'Stars', type: Number })
    stars: number;

    @Column('int')
    @ApiProperty({ description: 'Score', type: Number })
    score: number;

    @Column('int')
    @ApiProperty({ description: 'Level', type: Number })
    level: number;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;

    @ManyToOne(
        () => User,
        user => user.gameLevels,
        { onDelete: 'CASCADE' },
    )
    user: User;

    @ManyToOne(
        () => Game,
        game => game.gameLevels,
        { onDelete: 'CASCADE' },
    )
    game: Game;
}
