import { ApiProperty } from '@nestjs/swagger';
import { IsString, MaxLength, MinLength } from 'class-validator';

export class ResendVerificationReqDto {
    @ApiProperty({ description: 'E-mail address or username of the unconfirmed account' })
    @IsString()
    @MinLength(3)
    @MaxLength(254)
    identifier: string;
}
