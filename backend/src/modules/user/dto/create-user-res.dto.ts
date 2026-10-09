import { ApiProperty, ApiPropertyOptional } from '@nestjs/swagger';

import { UserResDto } from './user-res.dto';

export class CreateUserResDto {
    @ApiPropertyOptional({ description: 'Access token (only for accounts that need no verification)' })
    accessToken?: string;

    @ApiPropertyOptional({ description: 'User' })
    user?: UserResDto;

    @ApiProperty({ description: 'True when the e-mail address must be confirmed before login' })
    verificationRequired: boolean;

    @ApiPropertyOptional({ description: 'Address the confirmation mail was sent to' })
    email?: string;

    @ApiPropertyOptional({ description: 'False when the mail could not be sent; use resend' })
    mailSent?: boolean;
}
