import { ApiProperty } from '@nestjs/swagger';
import { IsEnum, IsString, IsOptional } from 'class-validator';

import { UserProvider } from '~common/constants/enums/user-provider.enum';

export class SocialUserReqDto {
    @ApiProperty({
        nullable: false,
        enum: UserProvider,
    })
    @IsEnum(UserProvider)
    provider: UserProvider;

    @ApiProperty({ description: 'User Id', nullable: true })
    @IsString()
    @IsOptional()
    userId?: string;

    @ApiProperty({ description: 'Access Token', nullable: false })
    @IsString()
    accessToken: string;
}
