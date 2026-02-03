import { Body, Controller, Get, Param, Post, UseGuards, UseInterceptors } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';
import { GetUser } from '~common/decorators/user.decorator';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { LevelReqDto } from './dto/level-req.dto';
import { LevelResDto } from './dto/level-res.dto';
import { StarsAvgDto } from './dto/stars-avg.dto';
import { UpdateLevelReqDto } from './dto/update-level-req.dto';
import { LevelService } from './level.service';

@ApiTags('Levels')
@ApiBearerAuth('access-token')
@UseGuards(JwtAuthGuard)
@Controller('levels')
export class LevelController {
    constructor(private levelService: LevelService) {}

    @ApiOperation({ summary: 'Get a list of levels' })
    @ApiResponse({ status: 200, description: 'Found', type: LevelResDto, isArray: true })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found' })
    @UseInterceptors(ExcludeInterceptor)
    @Get('/list/:gameName')
    findAll(@Param('gameName') gameName: string, @GetUser() user: User): Promise<LevelResDto[]> {
        return this.levelService.findAll(gameName, user);
    }

    @ApiOperation({ summary: 'Create a new level' })
    @ApiResponse({ status: 201, description: 'Create', type: User })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post()
    create(@Body() levelReqDto: LevelReqDto, @GetUser() user: User): Promise<User> {
        return this.levelService.create(levelReqDto, user);
    }

    @ApiOperation({ summary: 'Update level' })
    @ApiResponse({ status: 200, description: 'Updated', type: User })
    @ApiResponse({ status: 400, description: 'Bad Request' })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/:id')
    update(
        @Param('id') id: string,
        @GetUser() user: User,
        @Body() level: UpdateLevelReqDto,
    ): Promise<User> {
        return this.levelService.update(id, user, level);
    }

    @ApiOperation({ summary: 'Get stars average' })
    @ApiResponse({ status: 200, description: 'Found', type: StarsAvgDto, isArray: true })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found' })
    @UseInterceptors(ExcludeInterceptor)
    @Get('/avg')
    starsAvg(@GetUser() user: User): Promise<StarsAvgDto> {
        return this.levelService.getStarsAvg(user);
    }
}
