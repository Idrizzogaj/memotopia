import { ApiProperty } from '@nestjs/swagger';
import { IsOptional, IsString } from 'class-validator';

export class UpdateAccountReqDto {
    @ApiProperty({ description: 'New username', nullable: true })
    @IsString()
    @IsOptional()
    newUsername?: string;

    @ApiProperty({ description: 'Avatar image', nullable: true })
    @IsString()
    @IsOptional()
    avatar?: string;
}
