import { ApiPropertyOptional } from '@nestjs/swagger';
import { IsInt, IsNumber, IsOptional, IsPositive, Max, Min } from 'class-validator';

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

    @ApiPropertyOptional({ description: 'Correct answers * 1000 + seconds left' })
    @IsInt()
    @Min(-1)
    @Max(1000000)
    @IsOptional()
    progress?: number;

    @ApiPropertyOptional({ enum: ParticipantStatus })
    status?: ParticipantStatus;
}
