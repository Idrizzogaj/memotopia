import { ApiProperty } from '@nestjs/swagger';
import { Exclude } from 'class-transformer';

import { UserStatistics } from '~common/db/entities/user-statistics.entity';

export class UserResDto {
    @ApiProperty({ description: 'User Id' })
    id: number;

    @ApiProperty({ description: 'Email', nullable: false })
    email: string;

    @ApiProperty({ description: 'Username', nullable: false })
    username: string;

    @ApiProperty({ description: 'Avatar Image', nullable: false })
    avatar: string;

    @ApiProperty({ description: 'Payment Status', nullable: false })
    paymentStatus: string;

    @ApiProperty({ description: 'Statistics', nullable: false })
    userStatistics: UserStatistics;

    @Exclude()
    password: string;
}
