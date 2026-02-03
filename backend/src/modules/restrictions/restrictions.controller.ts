import { Body, Controller, Get, Param, Post, UseGuards, UseInterceptors } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';

import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { GetUser } from '~common/decorators/user.decorator';
import { User } from '~common/db/entities/user.entity';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';
import { Restrictions } from '~common/db/entities/restrictions.entity';

import { RestrictionsDto } from './dto/restrictions.dto';
import { RestrictionsService } from './restrictions.service';

@ApiTags('Restrictions')
@Controller('restrictions')
@ApiBearerAuth('access-token')
@UseGuards(JwtAuthGuard)
export class RestrictionsController {
    constructor(private restrictionsService: RestrictionsService) {}

    @ApiOperation({ summary: 'Update restriction' })
    @ApiResponse({ status: 200, description: 'Updated', type: RestrictionsDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/update')
    update(@Body() restrictionsDto: RestrictionsDto, @GetUser() user: User): Promise<Restrictions> {
        return this.restrictionsService.update(user, restrictionsDto);
    }

    @ApiOperation({ summary: 'Get restrictions by user' })
    @ApiResponse({ status: 200, description: 'Found', type: Restrictions, isArray: true })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found' })
    @UseInterceptors(ExcludeInterceptor)
    @Get('/:currentDate')
    find(@Param('currentDate') currentDate: string, @GetUser() user: User): Promise<Restrictions> {
        return this.restrictionsService.find(user, currentDate);
    }
}
