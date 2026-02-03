import { ApiProperty } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';

export class GetHistoryRes {
    @ApiProperty({ description: 'Who challenged', nullable: false, type: String })
    challenger: User;

    @ApiProperty({ description: 'Did you win or lose', nullable: false, type: String })
    winOrLose: string;

    @ApiProperty({ description: 'Game name', nullable: false, type: String })
    gameName: string;

    @ApiProperty({ description: 'Level of game', nullable: false, type: Number })
    level: number;
}
