import { Body, Controller, Post, UseInterceptors, ValidationPipe } from '@nestjs/common';
import { ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { AuthService } from './auth.service';
import { AuthCredentialsDto } from './dto/auth-credentials.dto';
import { SocialUserReqDto } from './dto/socialUser-req.dto';

@ApiTags('Auth')
@Controller('auth')
export class AuthController {
    constructor(private authService: AuthService) {}

    @ApiOperation({ summary: 'Signin' })
    @ApiResponse({ status: 200, description: 'Signin' })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 409, description: 'Conflict - Invalid Provider' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/signin')
    signIn(
        @Body(ValidationPipe) authCredentialsDto: AuthCredentialsDto,
    ): Promise<{ accessToken: string }> {
        return this.authService.signIn(authCredentialsDto);
    }

    @ApiOperation({ summary: 'Create a social user' })
    @ApiResponse({ status: 201, description: 'Sign in with social media' })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 400, description: 'Bad Request' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/social-signin')
    socialSignIn(
        @Body(ValidationPipe) socialUserReqDto: SocialUserReqDto,
    ): Promise<{ accessToken: string; user: User }> {
        return this.authService.socialSignin(socialUserReqDto);
    }
}
