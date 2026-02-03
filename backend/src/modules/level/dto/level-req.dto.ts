import { ApiProperty } from '@nestjs/swagger';
import { IsNumber, IsString } from 'class-validator';

export class LevelReqDto {
    @ApiProperty({ description: 'Stars', type: Number })
    @IsNumber()
    stars: number;

    @ApiProperty({ description: 'Score', type: Number })
    @IsNumber()
    score: number;

    @ApiProperty({ description: 'Level', type: Number })
    @IsNumber()
    level: number;

    @ApiProperty({ description: 'Game name', type: String })
    @IsString()
    gameName: string;
}
