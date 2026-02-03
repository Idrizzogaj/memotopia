import { ApiProperty } from '@nestjs/swagger';
import { Entity, Column, PrimaryGeneratedColumn, ManyToOne, OneToMany } from 'typeorm';

import { Game } from './game.entity';
import { Participant } from './participant.entity';

@Entity()
export class Challenge {
    @PrimaryGeneratedColumn()
    @ApiProperty({ description: 'Challenge Id' })
    id: number;

    @Column('int')
    @ApiProperty({ description: 'Level' })
    level: number;

    @ManyToOne(
        () => Game,
        game => game.gameLevels,
        { onDelete: 'CASCADE' },
    )
    game: Game;

    @OneToMany(
        () => Participant,
        participant => participant.challenge,
    )
    participants: Participant[];
}
