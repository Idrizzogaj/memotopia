import { ApiProperty } from '@nestjs/swagger';
import { Entity, Column, PrimaryGeneratedColumn, OneToMany } from 'typeorm';

import { Challenge } from './challenge.entity';
import { GameLevel } from './game-level.entity';

@Entity()
export class Game {
    @PrimaryGeneratedColumn()
    @ApiProperty({ description: 'User Id' })
    id: number;

    @Column('varchar', { unique: true })
    @ApiProperty({ description: 'Game name' })
    gameName: string;

    @OneToMany(
        () => GameLevel,
        gameLevel => gameLevel.game,
    )
    gameLevels: GameLevel[];

    @OneToMany(
        () => Challenge,
        challenge => challenge.game,
    )
    challenges: Challenge[];
}
