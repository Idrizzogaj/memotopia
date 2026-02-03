import { ApiProperty } from '@nestjs/swagger';
import { IsEmail, IsString, Matches, MaxLength, MinLength } from 'class-validator';

export class UserReqDto {
    @ApiProperty({ description: 'Email', nullable: false })
    @IsEmail()
    email: string;

    @ApiProperty({ description: 'Username', nullable: false })
    @IsString()
    @MinLength(5)
    @MaxLength(25)
    username: string;

    @ApiProperty({ description: 'User password', nullable: false })
    @MinLength(8)
    @MaxLength(20)
    @Matches(/((?=.*\d)|(?=.*\W+))(?![.\n])(?=.*[A-Z])(?=.*[a-z]).*$/, {
        message: 'Password too weak',
    })
    password: string;
}
