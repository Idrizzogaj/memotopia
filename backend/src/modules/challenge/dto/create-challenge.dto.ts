import { ApiProperty } from '@nestjs/swagger';
import { IsNumber, IsPositive, IsString } from 'class-validator';

export class CreateChallengeDto {
    @ApiProperty({ description: 'Level' })
    @IsNumber()
    @IsPositive()
    level: number;

    @ApiProperty({ description: 'Game name' })
    @IsString()
    gameName: string;

    @ApiProperty({ description: 'ChallengerId' })
    @IsNumber()
    @IsPositive()
    challengerId: number;
}
