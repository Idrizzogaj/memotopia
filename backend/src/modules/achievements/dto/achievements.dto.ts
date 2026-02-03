import { ApiProperty } from '@nestjs/swagger';

export class AchievementsDto {
    @ApiProperty({ description: 'Array of Achievement Values', nullable: false })
    achievements: string[];
}
