import {
    Body,
    Controller,
    Get,
    Post,
    Query,
    UseGuards,
    UseInterceptors,
    ValidationPipe,
} from '@nestjs/common';
import { ApiBearerAuth, ApiOperation, ApiQuery, ApiResponse, ApiTags } from '@nestjs/swagger';

import { User } from '~common/db/entities/user.entity';
import { GetUser } from '~common/decorators/user.decorator';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { ExcludeInterceptor } from '~common/interceptors/exclude.interceptor';

import { ChallengeService } from './challenge.service';
import { ChallengeResDto } from './dto/challenge-res.dto';
import { CreateChallengeDto } from './dto/create-challenge.dto';
import { GetChallengeDto } from './dto/get-challenge.dto';
import { GetHistoryRes } from './dto/get-history-res.dto';
import { PaginationChallengeDto } from './dto/pagination-challenge.dto';
import { UpdateChallengeDto } from './dto/update-challenge.dto';

@ApiTags('Challenges')
@ApiBearerAuth('access-token')
@UseGuards(JwtAuthGuard)
@Controller('challenges')
export class ChallengeController {
    constructor(private challengeService: ChallengeService) {}

    // Post challenge (create)'
    @ApiOperation({ summary: 'Create a new challenge' })
    @ApiResponse({ status: 201, description: 'Create', type: ChallengeResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found - Game' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Post()
    create(
        @Body() createChallengeDto: CreateChallengeDto,
        @GetUser() user: User,
    ): Promise<ChallengeResDto> {
        return this.challengeService.create(createChallengeDto, user);
    }

    // Patch update
    @ApiOperation({ summary: 'Update a challenge' })
    @ApiResponse({ status: 200, description: 'Update', type: ChallengeResDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 400, description: 'Bad Request' })
    @ApiResponse({ status: 409, description: 'Conflict' })
    @UseInterceptors(ExcludeInterceptor)
    @Post('/update') // For unity we used post
    update(
        @Body() updateChallengeDto: UpdateChallengeDto,
        @GetUser() user: User,
    ): Promise<ChallengeResDto> {
        return this.challengeService.update(updateChallengeDto, user);
    }

    @ApiOperation({ summary: 'Get a list of challenges' })
    @ApiQuery({ type: GetChallengeDto })
    @ApiResponse({ status: 200, description: 'Found', type: PaginationChallengeDto })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @ApiResponse({ status: 404, description: 'Not Found - Challenge' })
    @ApiResponse({ status: 500, description: 'Internal Server Error' })
    @UseInterceptors(ExcludeInterceptor)
    @Get()
    getChallenges(
        @Query(new ValidationPipe({ transform: true })) getChallengeDto: GetChallengeDto,
        @GetUser() user: User,
    ): Promise<PaginationChallengeDto> {
        return this.challengeService.getChallenges(getChallengeDto, user);
    }

    @ApiOperation({ summary: 'Get wins and losses - history' })
    @ApiResponse({ status: 200, description: 'Found', type: GetHistoryRes, isArray: true })
    @ApiResponse({ status: 401, description: 'Unauthorized' })
    @UseInterceptors(ExcludeInterceptor)
    @Get('/history')
    getHistory(
        @Query(new ValidationPipe({ transform: true })) getChallengeDto: GetChallengeDto,
        @GetUser() user: User,
    ): Promise<GetHistoryRes[]> {
        return this.challengeService.getHistory(getChallengeDto, user);
    }


    // @ApiOperation({ summary: 'Add Achievements of challenges' })
    // @ApiResponse({ status: 200 })
    // @ApiResponse({ status: 401, description: 'Unauthorized' })
    // @UseInterceptors(ExcludeInterceptor)
    // @Get('/addAchievements')
    // addAchievements(): Promise<void> {
    //     return this.challengeService.addAchievements();
    // }
}
