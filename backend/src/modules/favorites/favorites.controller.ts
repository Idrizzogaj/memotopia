import { Controller, Get, Param, Post, UseGuards, UseInterceptors } from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiResponse, ApiTags } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';
import { GetUser } from '~common/decorators/user.decorator';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { FavoritesService } from './favorites.service';

@ApiTags('Favorites')
@ApiBearerAuth('access-token')
@UseGuards(JwtAuthGuard)
@Controller('favorites')
export class FavoritesController {
    constructor(private favoritesService: FavoritesService) {}

    @ApiOperation({ summary: 'Add a new favorite user' })
    @ApiResponse({ status: 201, description: 'Create', type: String })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found - User' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/:id/add')
    create(@Param('id') favorites: number, @GetUser() user: User): Promise<{ msg: string }> {
        return this.favoritesService.create({ requester: user.id, favorites });
    }

    @ApiOperation({ summary: 'Remove a favorite user' })
    @ApiResponse({ status: 200, description: 'Deleted', type: String })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found - User' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/:id/remove')
    delete(@Param('id') favorites: number, @GetUser() user: User): Promise<{ msg: string }> {
        return this.favoritesService.delete({ requester: user.id, favorites });
    }

    @ApiOperation({ summary: 'Get favorites' })
    @ApiResponse({ status: 200, description: 'Get favorites', type: User })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found - User' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Get()
    get(@GetUser() user: User): Promise<User[]> {
        return this.favoritesService.getFavorites(user.id);
    }
}
