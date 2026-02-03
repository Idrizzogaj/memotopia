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
    create(@Body() userReqDto: UserReqDto): Promise<CreateUserResDto> {
        return this.userService.create(userReqDto);
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
        return this.userService.forgotPassword(forgotPasswordReqDto, req.headers.host);
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
