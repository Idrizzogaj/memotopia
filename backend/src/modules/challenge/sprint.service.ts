import { Injectable, BadRequestException, ForbiddenException, NotFoundException, ConflictException } from '@nestjs/common';
import { InjectConnection } from '@nestjs/typeorm';
import { Connection, MoreThan } from 'typeorm';
import { randomBytes } from 'crypto';
import { SprintBattle, SprintLeg } from '~common/db/entities/sprint-battle.entity';
import { User } from '~common/db/entities/user.entity';
import { GameLevel } from '~common/db/entities/game-level.entity';
import { SprintResult, compareSprint, validateResult, sprintGames } from './sprint-rules';

@Injectable()
export class SprintService {
    constructor(@InjectConnection() private connection: Connection) {}
    private leg(game: string, pacer: number): SprintLeg {
        return { game, pacer, seed: randomBytes(4).readInt32LE(0), starts: {}, results: {}, winner: null };
    }
    private member(battle: SprintBattle, user: number): void {
        if (!battle) throw new NotFoundException();
        if (battle.ownerId !== user && battle.recipientId !== user) throw new ForbiddenException();
    }
    async create(user: number, body: { recipientId: number; game: string; tier: number; mode: string }): Promise<SprintBattle> {
        if (!body || !Number.isInteger(body.recipientId) || body.recipientId === user || !sprintGames.includes(body.game) || ![6,12,18,21].includes(body.tier) || !['single','match'].includes(body.mode)) throw new BadRequestException();
        const opponent=await this.connection.getRepository(User).findOne(body.recipientId);
        const owner=await this.connection.getRepository(User).findOne(user);
        if (!opponent || !owner) throw new NotFoundException('Player not found');
        if(body.tier===21 && !await this.connection.getRepository(GameLevel).findOne({where:{user:{id:user},level:20,stars:MoreThan(0)}}))throw new ForbiddenException('Clear level20 to unlock Memory Athlete');
        const battle = new SprintBattle();battle.ownerId=user;battle.recipientId=body.recipientId;
        battle.ownerName=owner.username;battle.recipientName=opponent.username;
        battle.state={version:1,mode:body.mode as 'single'|'match',tier:body.tier,legs:[this.leg(body.game,user)],chooser:null,winner:null,done:false};
        return this.connection.getRepository(SprintBattle).save(battle);
    }
    async list(user: number): Promise<SprintBattle[]> {
        return this.connection.getRepository(SprintBattle).find({where:[{ownerId:user},{recipientId:user}],order:{updatedAt:'DESC'},take:100});
    }
    async read(user: number,id: number): Promise<SprintBattle> {
        const battle=await this.connection.getRepository(SprintBattle).findOne(id);this.member(battle,user);return battle;
    }
    private async change(user: number,id: number,update: (battle: SprintBattle)=>void): Promise<SprintBattle> {
        return this.connection.transaction(async manager=>{
            const repository=manager.getRepository(SprintBattle);
            const battle=await repository.findOne(id,{lock:{mode:'pessimistic_write'}});this.member(battle,user);
            const wasDone=battle.state.done;
            const players=[battle.ownerId,battle.recipientId].sort((a,b)=>a-b);
            const before=players.map(id=>battle.state.legs.filter(l=>!!l.results[id]).length);
            update(battle);
            for(let index=0;index<players.length;index++){
                const id=players[index],played=battle.state.legs.filter(l=>!!l.results[id]).length-before[index];
                const settled=!wasDone && battle.state.done;
                if(!played && !settled)continue;
                const won=settled && battle.state.winner===id,lost=settled && battle.state.winner!==0 && !won,draw=settled && battle.state.winner===0;
                // Exactly once, in the same transaction as the round/match receipt. Match points are not XP.
                await manager.query('UPDATE "user_statistics" SET "timePlayed"="timePlayed"+$1,"numberOfChallenges"="numberOfChallenges"+$2,"numberOfWinChallenges"="numberOfWinChallenges"+$3,"numberOfLossChallenges"="numberOfLossChallenges"+$4,"numberOfDrawChallenges"="numberOfDrawChallenges"+$5 WHERE "userId"=$6',[played*90,settled?1:0,won?1:0,lost?1:0,draw?1:0,id]);
            }
            return repository.save(battle);
        });
    }
    async choose(user: number,id: number,game: string): Promise<SprintBattle> {
        return this.change(user,id,battle=>{
            if(battle.state.done || battle.state.chooser!==user || !sprintGames.includes(game) || battle.state.legs.some(l=>l.game===game))throw new ConflictException('Not an available game choice');
            battle.state.legs.push(this.leg(game,user));battle.state.chooser=null;
        });
    }
    async start(user: number,id: number): Promise<SprintBattle> {
        return this.change(user,id,battle=>{
            const leg=battle.state.legs[battle.state.legs.length-1];
            if(battle.state.done || battle.state.chooser!==null || leg.results[user])throw new ConflictException('Round not playable');
            const pacer=leg.pacer || battle.ownerId;
            if(user!==pacer && !leg.results[pacer])throw new ConflictException('Waiting for pace setter');
            if(!leg.starts[user])leg.starts[user]=Date.now(); // Retrying never grants a new90seconds.
        });
    }
    async submit(user: number,id: number,legIndex: number,result: SprintResult): Promise<SprintBattle> {
        return this.change(user,id,battle=>{
            const state=battle.state;
            if(!Number.isInteger(legIndex) || legIndex<0 || legIndex>=state.legs.length)throw new BadRequestException('Invalid round');
            const leg=state.legs[legIndex];
            try{validateResult(leg.game,result);}catch(error){throw new BadRequestException('Invalid sprint result');}
            if(leg.results[user]){
                const fingerprint=(r:SprintResult)=>JSON.stringify([r.completed,r.correct,r.turns,r.errors,r.roundMs,r.trace.map(p=>[p.ms,p.units])]);
                if(fingerprint(leg.results[user])===fingerprint(result))return;
                throw new ConflictException('Result already recorded');
            }
            if(legIndex!==state.legs.length-1 || state.done || state.chooser!==null || !leg.starts[user] || Date.now()-leg.starts[user]<89000)throw new ConflictException('Round not ready for submission');
            leg.results[user]=result;
            const a=leg.results[battle.ownerId],b=leg.results[battle.recipientId];if(!a || !b)return;
            const comparison=compareSprint(leg.game,a,b);leg.winner=comparison>0?battle.ownerId:comparison<0?battle.recipientId:0;
            if(state.mode==='single'){state.done=true;state.winner=leg.winner;return;}
            const points=(player:number)=>state.legs.reduce((sum,l)=>sum+(l.winner===player?1:l.winner===0?.5:0),0);
            const ownerPoints=points(battle.ownerId),recipientPoints=points(battle.recipientId);
            if(ownerPoints>=2 || recipientPoints>=2 || state.legs.length===3){state.done=true;state.winner=ownerPoints>recipientPoints?battle.ownerId:recipientPoints>ownerPoints?battle.recipientId:0;return;}
            const played=Array.from(new Set(state.legs.map(l=>l.game)));
            if(played.length===1){state.chooser=battle.recipientId;return;}
            const remaining=sprintGames.filter(game=>!played.includes(game));
            state.legs.push(this.leg(remaining[randomBytes(4).readUInt32LE(0)%remaining.length],battle.ownerId));
        });
    }
    async cancel(user:number,id:number):Promise<SprintBattle>{
        return this.change(user,id,battle=>{
            if(battle.state.done)return;
            battle.state.done=true;battle.state.chooser=null;
            battle.state.winner=user===battle.ownerId?battle.recipientId:battle.ownerId;
        });
    }
}
