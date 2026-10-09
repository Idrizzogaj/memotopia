import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Exclude } from 'class-transformer';
import {
    CreateDateColumn,
    Entity,
    Column,
    UpdateDateColumn,
    PrimaryGeneratedColumn,
    OneToMany,
    OneToOne,
} from 'typeorm';

import { PaymentStatus } from '~common/constants/enums/payment-status';
import { UserProvider } from '~common/constants/enums/user-provider.enum';

import { Achievement } from './achievement.entity';
import { Favorites } from './favorites.entity';
import { GameLevel } from './game-level.entity';
import { Participant } from './participant.entity';
import { Restrictions } from './restrictions.entity';
import { UserStatistics } from './user-statistics.entity';

@Entity()
export class User {
    @PrimaryGeneratedColumn()
    @ApiProperty()
    id: number;

    @Column('varchar', { nullable: false, unique: true })
    @ApiProperty({ nullable: false })
    email: string;

    @Column('varchar', { nullable: false, unique: true })
    @ApiProperty({ nullable: false })
    username: string;

    @Column('varchar', { nullable: false })
    @ApiProperty({ nullable: false })
    provider: UserProvider;

    @Exclude()
    @Column('varchar', { nullable: true })
    @ApiProperty({ nullable: true })
    password: string;

    @Exclude()
    @Column('varchar', { nullable: true })
    @ApiProperty({ nullable: true })
    salt: string;

    @Column('varchar', { nullable: false, default: 'MaskGroup54' })
    @ApiProperty({ nullable: false })
    avatar: string;

    @Exclude()
    @Column('varchar', { nullable: true })
    @ApiPropertyOptional({ description: 'Reset password token', type: String })
    resetPasswordToken: string;

    @Column({ type: 'timestamp', nullable: true })
    @ApiPropertyOptional({ description: 'Reset password expires at', type: Date })
    @Exclude()
    resetPasswordExpiresAt: Date;

    // true only for accounts registered with e-mail/password that have not yet proven they own the
    // address. Defaults to false so every account that existed before this feature stays usable.
    @Column({ type: 'boolean', default: false })
    @ApiProperty({ default: false })
    emailVerificationPending: boolean;

    @Column({ type: 'timestamp', nullable: true })
    @ApiPropertyOptional({ description: 'When the email address was verified', type: Date })
    emailVerifiedAt: Date | null;

    // Only the SHA-256 hash of the one-time verification token is stored.
    @Exclude()
    @Column('varchar', { nullable: true })
    emailVerificationTokenHash: string | null;

    @Exclude()
    @Column({ type: 'timestamp', nullable: true })
    emailVerificationExpiresAt: Date | null;

    @Exclude()
    @Column({ type: 'timestamp', nullable: true })
    emailVerificationSentAt: Date | null;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;

    @Column({ type: 'enum', enum: PaymentStatus, default: PaymentStatus.NONE })
    @ApiProperty({
        description: 'Payment Status',
        enum: PaymentStatus,
        default: PaymentStatus.NONE,
    })
    paymentStatus: PaymentStatus;

    @OneToMany(
        () => GameLevel,
        gameLevel => gameLevel.user,
    )
    gameLevels: GameLevel[];

    @OneToMany(
        () => Participant,
        participant => participant.user,
    )
    participants: Participant[];

    @OneToMany(
        () => Favorites,
        favorites => favorites.requester,
    )
    requester: Favorites[];

    @OneToMany(
        () => Favorites,
        favorites => favorites.favorites,
    )
    favorites: Favorites[];

    @OneToOne(
        () => UserStatistics,
        userStatistics => userStatistics.user,
        { onDelete: 'CASCADE' },
    )
    userStatistics: UserStatistics;

    @OneToOne(
        () => Restrictions,
        restrictions => restrictions.user,
        { onDelete: 'CASCADE' },
    )
    restrictions: Restrictions;

    @OneToMany(
        () => Achievement,
        achievement => achievement.user,
    )
    achievement: Achievement[];
}
