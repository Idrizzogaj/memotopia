import { ApiProperty } from '@nestjs/swagger';
import { IsNumber } from 'class-validator';

export class UpdateLevelReqDto {
    @ApiProperty({ description: 'Stars', type: Number })
    @IsNumber()
    stars: number;

    @ApiProperty({ description: 'Score', type: Number })
    @IsNumber()
    score: number;
}
