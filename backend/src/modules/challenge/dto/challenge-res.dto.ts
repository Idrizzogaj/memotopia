import { ApiProperty } from '@nestjs/swagger';

import { Challenge } from '~common/db/entities/challenge.entity';
import { Participant } from '~common/db/entities/participant.entity';
import { User } from '~common/db/entities/user.entity';

export class ChallengeResDto {
    @ApiProperty({ description: 'Challenge', nullable: false })
    challenge: Challenge;

    @ApiProperty({ description: 'Participants', nullable: false, type: Participant, isArray: true })
    participants: Participant[];

    @ApiProperty({ description: 'Loggedin User', nullable: true, type: User })
    user?: User;
}
