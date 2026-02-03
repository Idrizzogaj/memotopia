import { ApiProperty } from '@nestjs/swagger';

import { Challenge } from '~common/db/entities/challenge.entity';

export class PaginationChallengeDto {
    @ApiProperty({ description: 'page', type: Number })
    page: number;

    @ApiProperty({ description: 'limit', type: Number })
    limit: number;

    @ApiProperty({ description: 'total', type: Number })
    total: number;

    @ApiProperty({ description: 'results', type: Challenge, isArray: true })
    results: Challenge[];
}
