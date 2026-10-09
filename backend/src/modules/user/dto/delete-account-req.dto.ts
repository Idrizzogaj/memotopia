import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';
import { Equals, IsOptional, IsString, MaxLength } from 'class-validator';

export class DeleteAccountReqDto {
    @ApiPropertyOptional({ description: 'Current password (required for e-mail accounts)' })
    @IsOptional()
    @IsString()
    @MaxLength(100)
    password?: string;

    @ApiProperty({ description: 'Must be the literal text DELETE' })
    @Equals('DELETE')
    confirm: string;
}
