import { BadRequestException, Body, Controller, Get, Param, ParseIntPipe, Post, UseGuards } from '@nestjs/common';
import { JwtAuthGuard } from '~common/guards/jwt.guard';
import { GetUser } from '~common/decorators/user.decorator';
import { User } from '~common/db/entities/user.entity';
import { SprintService } from './sprint.service';
import { SprintResult } from './sprint-rules';
import { SprintBattle } from '~common/db/entities/sprint-battle.entity';
function view(battle:SprintBattle){
 const s=battle.state;
 return {id:battle.id,ownerId:battle.ownerId,recipientId:battle.recipientId,ownerName:battle.ownerName,recipientName:battle.recipientName,serverTime:Date.now(),version:s.version,mode:s.mode,tier:s.tier,chooser:s.chooser,winner:s.winner,done:s.done,
 legs:s.legs.map(l=>({game:l.game,seed:l.seed,pacer:l.pacer||battle.ownerId,ownerStart:l.starts[battle.ownerId]||0,recipientStart:l.starts[battle.recipientId]||0,ownerPlayed:!!l.results[battle.ownerId],recipientPlayed:!!l.results[battle.recipientId],ownerResult:l.results[battle.ownerId]||null,recipientResult:l.results[battle.recipientId]||null,winner:l.winner}))};
}
@Controller('sprints')
@UseGuards(JwtAuthGuard)
export class SprintController {
    constructor(private service: SprintService) {}
    @Get() async list(@GetUser() user: User){return {battles:(await this.service.list(user.id)).map(view)};}
    @Post() async create(@GetUser() user: User,@Body() body: {recipientId:number;game:string;tier:number;mode:string}){return view(await this.service.create(user.id,body));}
    @Get(':id') async read(@GetUser() user: User,@Param('id',ParseIntPipe) id:number){return view(await this.service.read(user.id,id));}
    @Post(':id/start') async start(@GetUser() user: User,@Param('id',ParseIntPipe) id:number){return view(await this.service.start(user.id,id));}
    @Post(':id/result') async submit(@GetUser() user: User,@Param('id',ParseIntPipe) id:number,@Body() body:{legIndex:number;result:SprintResult}){if(!body)throw new BadRequestException();return view(await this.service.submit(user.id,id,body.legIndex,body.result));}
    @Post(':id/game') async choose(@GetUser() user: User,@Param('id',ParseIntPipe) id:number,@Body() body:{game:string}){if(!body)throw new BadRequestException();return view(await this.service.choose(user.id,id,body.game));}
    @Post(':id/cancel') async cancel(@GetUser() user:User,@Param('id',ParseIntPipe) id:number){return view(await this.service.cancel(user.id,id));}
}
