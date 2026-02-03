import { ApiProperty } from '@nestjs/swagger';

import { UserResDto } from './user-res.dto';

export class CreateUserResDto {
    @ApiProperty({ description: 'Access token', nullable: false })
    accessToken: string;

    @ApiProperty({ description: 'User', nullable: false })
    user: UserResDto;
}
