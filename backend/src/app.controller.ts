import {
    ClassSerializerInterceptor,
    Controller,
    Get,
    Header,
    UseGuards,
    UseInterceptors,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse } from '@nestjs/swagger';

import { HealthCheckDto } from '~health-check.dto';
import { AppService } from '~app.service';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { GetUser } from '~common/decorators/user.decorator';
import { User } from '~common/db/entities/user.entity';
import { UserResDto } from '~modules/user/dto/user-res.dto';
import { UserService } from '~modules/user/user.service';

@Controller()
export class AppController {
    constructor(
        private readonly appService: AppService,
        private readonly userService: UserService,
    ) {}

    @Get('/hello')
    getHello(): string {
        return this.appService.getHello();
    }

    @Get()
    @Header('Access-Control-Allow-Origin', '*')
    @Header('Cache-Control', 'no-cache, no-store')
    public async getHealthCheck(): Promise<HealthCheckDto> {
        return this.appService.getHealthCheck();
    }

    @ApiOperation({ summary: 'Get me' })
    @ApiResponse({ status: 200, description: 'Found', type: UserResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ClassSerializerInterceptor)
    @Get('/me')
    async me(@GetUser() user: User): Promise<User> {
        return await this.userService.getUserEntityById(user.id);
    }
}
