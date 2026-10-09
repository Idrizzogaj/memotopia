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

    // correct answers * 1000 + seconds left; -1 = reported by an app that does not send it (old rule).
    @Column('int', { default: -1 })
    @ApiProperty({ description: 'Progress (correct * 1000 + seconds left), -1 when unknown' })
    progress: number;

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
