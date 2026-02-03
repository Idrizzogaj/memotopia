import { ApiProperty } from '@nestjs/swagger';

export class LevelResDto {
    @ApiProperty({ description: 'Level Id' })
    id: number;

    @ApiProperty({ description: 'Stars', type: Number })
    stars: number;

    @ApiProperty({ description: 'Score', type: Number })
    score: number;

    @ApiProperty({ description: 'Level', type: Number })
    level: number;

    @ApiProperty()
    createdAt: Date;

    @ApiProperty()
    updatedAt: Date | null;
}
