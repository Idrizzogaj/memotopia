import { ApiProperty } from '@nestjs/swagger';
import { IsNumber } from 'class-validator';

export class StarsAvgDto {
    @ApiProperty({ description: 'Stars Average', type: Number })
    @IsNumber()
    avg: number;
}
