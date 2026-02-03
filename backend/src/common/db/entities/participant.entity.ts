import { ApiProperty } from '@nestjs/swagger';
import {
    Entity,
    Column,
    PrimaryGeneratedColumn,
    ManyToOne,
    CreateDateColumn,
    UpdateDateColumn,
} from 'typeorm';

import { ParticipantStatus } from '~common/constants/enums/participant.enum';

import { Challenge } from './challenge.entity';
import { User } from './user.entity';

@Entity()
export class Participant {
    @PrimaryGeneratedColumn()
    @ApiProperty({ description: 'Participant Id' })
    id: number;

    @Column('float', { default: 0 })
    @ApiProperty({ description: 'Score' })
    score: number;

    @Column({ type: 'enum', enum: ParticipantStatus, default: ParticipantStatus.PENDING })
    @ApiProperty({ description: 'Status', enum: ParticipantStatus })
    status: ParticipantStatus;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;

    @ManyToOne(
        () => User,
        user => user.participants,
        { onDelete: 'CASCADE' },
    )
    user: User;

    @ManyToOne(
        () => Challenge,
        challenge => challenge.participants,
        { onDelete: 'CASCADE' },
    )
    challenge: Challenge;
}
