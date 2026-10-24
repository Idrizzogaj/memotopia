import { Body, Controller, Get, Post, UseGuards } from '@nestjs/common';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { GetUser } from '~common/decorators/user.decorator';
import { User } from '~common/db/entities/user.entity';
import { SocialService } from './social.service';
@Controller('sprints/social')
@UseGuards(JwtAuthGuard)
export class SocialController {
    constructor(private service:SocialService){}
    @Get() settings(@GetUser() user:User){return this.service.settings(user.id);}
    @Get('online') online(@GetUser() user:User){return this.service.online(user.id);}
    @Post('heartbeat') heartbeat(@GetUser() user:User,@Body() body:{active:boolean;visible?:boolean;friendsVisible?:boolean}){return this.service.heartbeat(user.id,body);}
    @Post('friend') friend(@GetUser() user:User,@Body() body:{targetId:number;action:string}){return this.service.friend(user.id,body&&body.targetId,body&&body.action);}
    @Post('block') block(@GetUser() user:User,@Body() body:{targetId:number;blocked:boolean}){return this.service.block(user.id,body && body.targetId,body && body.blocked);}
}
