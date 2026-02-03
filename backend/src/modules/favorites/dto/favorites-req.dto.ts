import { ApiProperty } from '@nestjs/swagger';
import { IsNumber } from 'class-validator';

export class FavoritesReqDto {
    @ApiProperty({ description: 'Requester', nullable: false })
    @IsNumber()
    requester: number;

    @ApiProperty({ description: 'Favorites', nullable: false })
    @IsNumber()
    favorites: number;
}
