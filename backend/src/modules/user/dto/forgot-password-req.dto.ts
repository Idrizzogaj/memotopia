import { ApiProperty } from '@nestjs/swagger';
import { IsEmail } from 'class-validator';

export class ForgotPasswordReqDto {
    @ApiProperty({ description: 'User email' })
    @IsEmail()
    email: string;
}
