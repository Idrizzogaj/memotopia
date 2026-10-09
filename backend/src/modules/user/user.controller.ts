import {
    Body,
    Controller,
    Get,
    HttpCode,
    Param,
    ParseIntPipe,
    Post,
    Req,
    Res,
    UseGuards,
    UseInterceptors,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';
import { Request, Response } from 'express';

import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { GetUser } from '~common/decorators/user.decorator';
import { User } from '~common/db/entities/user.entity';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { UserResDto } from './dto/user-res.dto';
import { UserReqDto } from './dto/user-req.dto';
import { UserService } from './user.service';
import { UpdateAccountReqDto } from './dto/update-account-req.dto';
import { CreateUserResDto } from './dto/create-user-res.dto';
import { PaymentStatusReqDto } from './dto/payment-status-req.dto';
import { ForgotPasswordReqDto } from './dto/forgot-password-req.dto';
import { ResetPasswordReqDto } from './dto/reset-password-req';
import { ResendVerificationReqDto } from './dto/resend-verification-req.dto';
import { DeleteAccountReqDto } from './dto/delete-account-req.dto';
import { rateLimit } from '~common/rate-limit';

function page(title: string, body: string): string {
    return `<!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>${title}</title><style>body{font-family:sans-serif;max-width:32rem;margin:3rem auto;padding:0 1rem;text-align:center}button{font-size:1.1rem;padding:.7rem 1.4rem}</style></head><body><h1>${title}</h1>${body}</body></html>`;
}

@ApiTags('Users')
@Controller('users')
export class UserController {
    constructor(private userService: UserService) {}

    @ApiOperation({ summary: 'Get random user' })
    @ApiResponse({ status: 200, description: 'Found', type: UserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Get('/random')
    random(@GetUser() user: User): Promise<UserResDto> {
        return this.userService.getRandom(user);
    }

    @ApiOperation({ summary: 'Get user by id' })
    @ApiResponse({ status: 200, description: 'Found', type: UserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not found!' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Get('/:id')
    findOne(@Param('id', ParseIntPipe) id: number): Promise<UserResDto> {
        return this.userService.getById(id);
    }

    @ApiOperation({ summary: 'Create a new user' })
    @ApiResponse({ status: 201, description: 'Create', type: CreateUserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post()
    create(@Body() userReqDto: UserReqDto, @Req() req: Request): Promise<CreateUserResDto> {
        // Throttle before any costly hashing or outgoing mail: per client IP and per target address.
        rateLimit(`register:ip:${req.ip}`, 5, 60 * 60 * 1000);
        rateLimit(`register:email:${String(userReqDto.email).toLowerCase()}`, 3, 60 * 60 * 1000);
        return this.userService.create(userReqDto);
    }

    @ApiOperation({ summary: 'Resend the e-mail confirmation link' })
    @ApiResponse({ status: 204, description: 'Accepted (always, to avoid revealing which addresses exist)' })
    @HttpCode(204)
    @Post('/resend-verification')
    async resendVerification(
        @Body() dto: ResendVerificationReqDto,
        @Req() req: Request,
    ): Promise<void> {
        rateLimit(`resend:ip:${req.ip}`, 10, 60 * 60 * 1000);
        rateLimit(`resend:target:${dto.identifier.toLowerCase()}`, 5, 60 * 60 * 1000);
        await this.userService.resendVerification(dto.identifier);
    }

    // The link in the e-mail only shows a button; the token is consumed by the POST below. This keeps
    // mail scanners that pre-fetch links from using up the one-time token.
    @ApiOperation({ summary: 'E-mail confirmation page' })
    @Get('/verify-email/:token')
    verifyEmailPage(@Param('token') token: string, @Res() res: Response): void {
        res.type('html').send(
            page(
                'Confirm your email',
                `<p>Press the button to confirm your Memotopia account.</p>
<form method="post" action="/users/verify-email/${encodeURIComponent(token)}"><button type="submit">Confirm email</button></form>`,
            ),
        );
    }

    @ApiOperation({ summary: 'Confirm e-mail address' })
    @Post('/verify-email/:token')
    async verifyEmail(
        @Param('token') token: string,
        @Req() req: Request,
        @Res() res: Response,
    ): Promise<void> {
        rateLimit(`verify:ip:${req.ip}`, 30, 60 * 60 * 1000);
        const ok = await this.userService.verifyEmail(token);
        res.status(ok ? 200 : 400)
            .type('html')
            .send(
                ok
                    ? page('Email confirmed', '<p>Your email is confirmed. Go back to the Memotopia app and log in.</p>')
                    : page(
                          'Link not valid',
                          '<p>This link is invalid, already used, or has expired. Open the app and request a new confirmation email.</p>',
                      ),
            );
    }

    @ApiOperation({ summary: 'Permanently delete the signed-in account' })
    @ApiResponse({ status: 204, description: 'Account deleted' })
    @ApiResponse({ status: 401, description: 'Wrong password' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @HttpCode(204)
    @Post('/delete-account')
    async deleteAccount(@Body() dto: DeleteAccountReqDto, @GetUser() user: User): Promise<void> {
        rateLimit(`delete:user:${user.id}`, 5, 60 * 60 * 1000);
        await this.userService.deleteAccount(user.id, dto);
    }

    @ApiOperation({ summary: 'Update account' })
    @ApiResponse({ status: 200, description: 'Updated', type: UserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Post('/update-account')
    updateAccount(
        @Body() updateAccountReqDto: UpdateAccountReqDto,
        @GetUser() user: User,
    ): Promise<UserResDto> {
        return this.userService.updateAccount(user.id, updateAccountReqDto);
    }

    @ApiOperation({ summary: 'Search user by username' })
    @ApiResponse({ status: 200, description: 'Found', type: UserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Get('search/:search')
    search(@Param('search') search: string, @GetUser() user: User): Promise<UserResDto[]> {
        return this.userService.search(search, user);
    }

    @ApiOperation({ summary: 'Update payment status' })
    @ApiResponse({ status: 200, description: 'Found', type: UserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not found!' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Post('/update-payment-status')
    updatePaymentStatus(
        @Body() paymentStatus: PaymentStatusReqDto,
        @GetUser() user: User,
    ): Promise<UserResDto> {
        return this.userService.updatePaymentStatus(user.id, paymentStatus);
    }

    @ApiOperation({ summary: 'Forgot password' })
    @ApiResponse({ status: 204, description: 'Email has been sent' })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @Post('/forgot')
    forgotPassword(
        @Body() forgotPasswordReqDto: ForgotPasswordReqDto,
        @Req() req: Request,
    ): Promise<void> {
        rateLimit(`forgot:ip:${req.ip}`, 10, 60 * 60 * 1000);
        rateLimit(`forgot:email:${String(forgotPasswordReqDto.email).toLowerCase()}`, 3, 60 * 60 * 1000);
        return this.userService.forgotPassword(forgotPasswordReqDto);
    }

    @ApiOperation({ summary: 'Reset forgot password' })
    @ApiResponse({ status: 200, description: 'Serve html page ' })
    @Get('/reset/:token')
    getResetHtml(@Param('token') token: string, @Req() req: Request, @Res() res: Response): void {
        this.userService.getResetHtml(res, token);
    }

    @ApiOperation({ summary: 'Forgot password' })
    @ApiResponse({ status: 200, description: 'user' })
    @ApiResponse({ status: 404, description: 'Not found!' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @HttpCode(200)
    @UseInterceptors(ExcludeInterceptor)
    @Post('/reset/:token')
    reset(
        @Param('token') token: string,
        @Body() resetPasswordReqDto: ResetPasswordReqDto,
    ): Promise<User> {
        return this.userService.reset(token, resetPasswordReqDto);
    }
}
