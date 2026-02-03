import { IsNumber, IsOptional, Min } from 'class-validator';
import { Type } from 'class-transformer';
import { ApiPropertyOptional } from '@nestjs/swagger';

export class PaginationQueryDto {
    @ApiPropertyOptional({ description: 'Page number', type: Number })
    @IsNumber()
    @IsOptional()
    @Min(1)
    @Type(() => Number)
    page: number = 1;

    @ApiPropertyOptional({ description: 'Limit number', type: Number })
    @IsNumber()
    @IsOptional()
    @Min(1)
    @Type(() => Number)
    limit: number = 20;
}
