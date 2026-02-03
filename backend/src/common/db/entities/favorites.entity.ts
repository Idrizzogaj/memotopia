import { ApiProperty } from '@nestjs/swagger';
import {
    CreateDateColumn,
    Entity,
    UpdateDateColumn,
    PrimaryGeneratedColumn,
    ManyToOne,
} from 'typeorm';

import { User } from './user.entity';

@Entity()
export class Favorites {
    @PrimaryGeneratedColumn()
    @ApiProperty()
    id: number;

    @ManyToOne(
        () => User,
        user => user.requester,
    )
    @ApiProperty({ nullable: false })
    requester: User;

    @ManyToOne(
        () => User,
        user => user.favorites,
    )
    @ApiProperty({ nullable: false })
    favorites: User;

    @CreateDateColumn()
    @ApiProperty()
    createdAt: Date;

    @UpdateDateColumn()
    @ApiProperty()
    updatedAt: Date | null;
}
