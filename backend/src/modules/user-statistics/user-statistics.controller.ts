import { Body, Controller, Get, Post, UseGuards, UseInterceptors } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';
import { GetUser } from '~common/decorators/user.decorator';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { TimePlayedDto } from './dto/time-played.dto';
import { UserStatisticsDto } from './dto/user-statistics.dto';
import { UserStatisticsService } from './user-statistics.service';

@ApiTags('Users Statistics')
@Controller('user-statistics')
export class UserStatisticsController {
    constructor(private userStatisticsService: UserStatisticsService) {}

    @ApiOperation({ summary: 'Get user statistics by user' })
    @ApiResponse({ status: 200, description: 'Found', type: UserStatisticsDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not found!' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Get()
    getUserStatisticsChallengeGames(@GetUser() user: User): Promise<UserStatisticsDto> {
        return this.userStatisticsService.getByUser(user);
    }

    @ApiOperation({ summary: 'Get global score' })
    @ApiResponse({ status: 200, description: 'Found', type: UserStatisticsDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not found!' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Get('/global-score')
    getGlobalScore(): Promise<UserStatisticsDto[]> {
        return this.userStatisticsService.getGlobalScore();
    }

    @ApiOperation({ summary: 'Get time played' })
    @ApiResponse({ status: 200, description: 'Found', type: UserStatisticsDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not found!' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Get('/time-played')
    getTimePlayed(@GetUser() user: User): Promise<TimePlayedDto> {
        return this.userStatisticsService.getTimePlayed(user);
    }

    @ApiOperation({ summary: 'Increment time played' })
    @ApiResponse({ status: 201, description: 'Create' })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @ApiBearerAuth('access-token')
    @UseGuards(JwtAuthGuard)
    @UseInterceptors(ExcludeInterceptor)
    @Post('/time-played')
    postTimePlayed(@Body() timePlayed: TimePlayedDto, @GetUser() user: User): Promise<{msg: string}> {
        return this.userStatisticsService.postTimePlayed(user, timePlayed.timePlayed);
    }
}
