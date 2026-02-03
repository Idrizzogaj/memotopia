import { Body, Controller, Get, Post, UseGuards, UseInterceptors } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';
import { GetUser } from '~common/decorators/user.decorator';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { AchievementsService } from './achievements.service';
import { AchievementsDto } from './dto/achievements.dto';

@ApiTags('Achievements')
@Controller('achievements')
@ApiBearerAuth('access-token')
@UseGuards(JwtAuthGuard)
export class AchievementsController {
    constructor(private achievementsService: AchievementsService) {}

    @ApiOperation({ summary: 'Add Achievement' })
    @ApiResponse({ status: 201, description: 'Achievement created' })
    @ApiResponse({ status: 400, description: 'Invalid Achievement' })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 409, description: 'This achievement for this user exists' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post()
    create(@Body() achievementsDto: AchievementsDto, @GetUser() user: User): Promise<any> {
        return this.achievementsService.create(achievementsDto, user);
    }

    @ApiOperation({ summary: 'Get Achievements' })
    @ApiResponse({
        status: 200,
        description: 'Get Achievements of a user',
        type: String,
        isArray: true,
    })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @UseInterceptors(ExcludeInterceptor)
    @Get()
    find(@GetUser() user: User): Promise<string[]> {
        return this.achievementsService.find(user);
    }
}
