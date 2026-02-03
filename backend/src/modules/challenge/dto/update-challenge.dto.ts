import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsNumber, IsOptional, IsPositive } from 'class-validator';

import { ParticipantStatus } from '~common/constants/enums/participant.enum';

export class UpdateChallengeDto {
    @ApiPropertyOptional({ description: 'Score' })
    @IsNumber()
    @IsOptional()
    score?: number;

    @ApiPropertyOptional({ description: 'ChallengerId' })
    @IsNumber()
    @IsPositive()
    challengeId?: number;

    @ApiPropertyOptional({ enum: ParticipantStatus })
    status?: ParticipantStatus;
}
