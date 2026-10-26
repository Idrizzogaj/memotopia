import { Injectable, BadRequestException, ForbiddenException, NotFoundException, ConflictException } from '@nestjs/common';
import { InjectConnection } from '@nestjs/typeorm';
import { Connection, MoreThan } from 'typeorm';
import { randomBytes } from 'crypto';
import { SprintBattle, SprintLeg } from '~common/db/entities/sprint-battle.entity';
import { User } from '~common/db/entities/user.entity';
import { GameLevel } from '~common/db/entities/game-level.entity';
import { SocialService } from './social.service';
import { SprintResult, compareSprint, validateResult, sprintGames } from './sprint-rules';

@Injectable()
export class SprintService {
    constructor(@InjectConnection() private connection: Connection, private social:SocialService) {}
    private leg(game: string, pacer: number): SprintLeg {
        return { game, pacer, seed: randomBytes(4).readInt32LE(0), starts: {}, results: {}, winner: null };
    }
    private member(battle: SprintBattle, user: number): void {
        if (!battle) throw new NotFoundException();
        if (battle.ownerId !== user && battle.recipientId !== user) throw new ForbiddenException();
    }
    async create(user: number, body: { recipientId: number; game: string; tier: number; mode: string; version?:number; live?:boolean }): Promise<SprintBattle> {
        if (!body || (body.version!==undefined && body.version!==2) || (body.live!==undefined && typeof body.live!=='boolean') || !Number.isInteger(body.recipientId) || body.recipientId === user || !sprintGames.includes(body.game) || ![6,12,18,21].includes(body.tier) || !['single','match'].includes(body.mode)) throw new BadRequestException();
        if(body.version!==2)throw new ConflictException('Update Memotopia to send invitations');
        const opponent=await this.connection.getRepository(User).findOne(body.recipientId);
        const owner=await this.connection.getRepository(User).findOne(user);
        if (!opponent || !owner) throw new NotFoundException('Player not found');
        if(body.tier===21 && !await this.connection.getRepository(GameLevel).findOne({where:{user:{id:user},level:20,stars:MoreThan(0)}}))throw new ForbiddenException('Clear level20 to unlock Memory Athlete');
        const battle = new SprintBattle();battle.ownerId=user;battle.recipientId=body.recipientId;
        battle.ownerName=owner.username;battle.recipientName=opponent.username;
        battle.state={version:1,mode:body.mode as 'single'|'match',tier:body.tier,legs:[this.leg(body.game,user)],chooser:null,winner:null,done:false};
        return this.connection.transaction(async manager=>{
            await this.social.lock(manager,user,body.recipientId);await this.social.allowed(manager,user,body.recipientId);
            // Per sender and per pair limits are protected by the same database locks as blocking.
            const pending=await manager.query(`SELECT id FROM sprint_battle WHERE (("ownerId"=$1 AND "recipientId"=$2) OR ("ownerId"=$2 AND "recipientId"=$1)) AND state->>'done'='false' AND state->>'invitation'='pending' AND (state->>'invitationExpires')::bigint>$3 ORDER BY id DESC LIMIT 1`,[user,body.recipientId,Date.now()]);
            if(pending.length)return manager.getRepository(SprintBattle).findOne(pending[0].id);
            const active=await manager.query(`SELECT id FROM sprint_battle WHERE (("ownerId"=$1 AND "recipientId"=$2) OR ("ownerId"=$2 AND "recipientId"=$1)) AND state->>'done'='false' AND state->>'invitation'='accepted' ORDER BY id DESC LIMIT 1`,[user,body.recipientId]);
            if(active.length)return manager.getRepository(SprintBattle).findOne(active[0].id);
            // Completed/declined/withdrawn invitations must never lock out a rematch.
            // Keep the sender rate limit, but only charge it for genuinely new invitations.
            const recent=await manager.query(`SELECT COUNT(*)::int AS n FROM sprint_battle WHERE "ownerId"=$1 AND "createdAt">now()-interval '1 minute'`,[user]);
            if(Number(recent[0].n)>=5)throw new ConflictException('Please wait before inviting again');
            // Older clients keep their recorded-pace flow. Version2 always requires consent.
            if(body.version===2){battle.state.version=2;battle.state.invitation='pending';battle.state.invitationExpires=Date.now()+86400000;
                battle.state.live=!!body.live && await this.social.isOnline(manager,user,body.recipientId) && await this.social.isOnline(manager,body.recipientId,user);
            }
            return manager.getRepository(SprintBattle).save(battle);
        });
    }
    async list(user: number): Promise<SprintBattle[]> {
        await this.expire();await this.settleStale(user);
        return this.connection.getRepository(SprintBattle).find({where:[{ownerId:user},{recipientId:user}],order:{updatedAt:'DESC'},take:100});
    }
    async read(user: number,id: number): Promise<SprintBattle> {
        await this.expire();await this.settleStale(user);
        const battle=await this.connection.getRepository(SprintBattle).findOne(id);this.member(battle,user);return battle;
    }
    private async change(user: number,id: number,update: (battle: SprintBattle, manager: import('typeorm').EntityManager)=>void|Promise<void>): Promise<SprintBattle> {
        return this.connection.transaction(async manager=>{
            const repository=manager.getRepository(SprintBattle);
            const initial=await repository.findOne(id);this.member(initial,user);
            await this.social.lock(manager,initial.ownerId,initial.recipientId);
            const battle=await repository.findOne(id,{lock:{mode:'pessimistic_write'}});this.member(battle,user);
            const wasDone=battle.state.done;
            const players=[battle.ownerId,battle.recipientId].sort((a,b)=>a-b);
            const before=players.map(id=>battle.state.legs.filter(l=>!!l.results[id]).length);
            await update(battle,manager);
            for(let index=0;index<players.length;index++){
                const id=players[index],played=battle.state.legs.filter(l=>!!l.results[id]).length-before[index];
                const settled=!wasDone && battle.state.done && (!battle.state.invitation || battle.state.invitation==='accepted');
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
            if(battle.state.done)return;
            this.accepted(battle);
            const last=battle.state.legs[battle.state.legs.length-1];
            if(battle.state.chooser===null && last.game===game && last.pacer===user)return;
            if(battle.state.done || battle.state.chooser!==user || !sprintGames.includes(game) || battle.state.legs.some(l=>l.game===game))throw new ConflictException('Not an available game choice');
            battle.state.legs.push(this.leg(game,user));battle.state.chooser=null;
        });
    }
    async start(user: number,id: number): Promise<SprintBattle> {
        await this.settleStale(user);
        return this.change(user,id,battle=>{
            const leg=battle.state.legs[battle.state.legs.length-1];
            if(battle.state.done || leg.results[user])return;
            this.accepted(battle);
            if(battle.state.done || battle.state.chooser!==null || leg.results[user])throw new ConflictException('Round not playable');
            const pacer=leg.pacer || battle.ownerId;
            if(!battle.state.live && user!==pacer && !leg.results[pacer])throw new ConflictException('Waiting for pace setter');
            if(battle.state.live){
                leg.ready=leg.ready||{};leg.ready[user]=Date.now();
                if(!leg.liveStart && leg.ready[battle.ownerId] && leg.ready[battle.recipientId]){
                    leg.liveStart=Date.now()+4000;leg.starts[battle.ownerId]=leg.liveStart;leg.starts[battle.recipientId]=leg.liveStart;
                }
            }else if(!leg.starts[user])leg.starts[user]=Date.now(); // Retrying never grants a new90seconds.
        });
    }
    async submit(user: number,id: number,legIndex: number,result: SprintResult): Promise<SprintBattle> {
        return this.change(user,id,battle=>{
            const state=battle.state;
            if(!Number.isInteger(legIndex) || legIndex<0 || legIndex>=state.legs.length)throw new BadRequestException('Invalid round');
            const leg=state.legs[legIndex];
            try{validateResult(leg.game,result);}catch(error){throw new BadRequestException('Invalid sprint result');}
            // A late receipt must acknowledge an already forfeited match, never trap
            // the other phone in a permanent409/outbox loop or award statistics twice.
            if(state.done && (state.endReason || state.legs.some(l=>!l.results[battle.ownerId] || !l.results[battle.recipientId])))return;
            this.accepted(battle);
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
    private async settleStale(user:number):Promise<void>{
        const rows=await this.connection.query(`SELECT id FROM sprint_battle WHERE ("ownerId"=$1 OR "recipientId"=$1) AND state->>'live'='true' AND state->>'done'='false' AND (((state->'legs'->-1->>'liveStart')::bigint)<$2 OR (COALESCE((state->'legs'->-1->>'liveStart')::bigint,0)=0 AND (SELECT MIN(value::bigint) FROM jsonb_each_text(COALESCE(state->'legs'->-1->'ready','{}'::jsonb)))<$3)) LIMIT 20`,[user,Date.now()-150000,Date.now()-60000]);
        for(const row of rows)await this.change(user,row.id,battle=>{
            const leg=battle.state.legs[battle.state.legs.length-1];
            if(battle.state.done)return;
            if(!leg.liveStart){
                const ready=leg.ready||{},a=ready[battle.ownerId],b=ready[battle.recipientId];
                if((!a&&!b)||(a&&b)||Date.now()<Number(a||b)+60000)return;
                battle.state.done=true;battle.state.chooser=null;battle.state.winner=a?battle.ownerId:battle.recipientId;
                battle.state.endReason='connection';battle.state.forfeitedId=a?battle.recipientId:battle.ownerId;return;
            }
            if(Date.now()<leg.liveStart+150000)return;
            if(leg.results[battle.ownerId] && leg.results[battle.recipientId])return;
            battle.state.done=true;battle.state.chooser=null;
            battle.state.winner=leg.results[battle.ownerId]?battle.ownerId:leg.results[battle.recipientId]?battle.recipientId:0;
            battle.state.endReason='connection';
            battle.state.forfeitedId=battle.state.winner===0?0:battle.state.winner===battle.ownerId?battle.recipientId:battle.ownerId;
        });
    }
    private accepted(battle:SprintBattle):void {
        if(battle.state.invitation && battle.state.invitation!=='accepted')throw new ConflictException('Invitation has not been accepted');
    }
    private async expire():Promise<void>{
        await this.connection.query(`UPDATE sprint_battle SET state=jsonb_set(state,'{reactions}',COALESCE((SELECT jsonb_agg(r) FROM jsonb_array_elements(state->'reactions') r WHERE (r->>'at')::bigint>$1),'[]'::jsonb)) WHERE EXISTS(SELECT 1 FROM jsonb_array_elements(COALESCE(state->'reactions','[]'::jsonb)) r WHERE (r->>'at')::bigint<=$1)`,[Date.now()-600000]);
        await this.connection.query(`UPDATE sprint_battle SET state=jsonb_set(jsonb_set(state,'{invitation}','"expired"'),'{done}','true'),"updatedAt"=now() WHERE state->>'invitation'='pending' AND (state->>'invitationExpires')::bigint<$1`,[Date.now()]);
        await this.connection.query('UPDATE player_social SET "onlineUntil"=0 WHERE "onlineUntil">0 AND "onlineUntil"<$1',[Date.now()]);
        await this.connection.query('DELETE FROM player_friend WHERE NOT accepted AND "requestedAt"<$1',[Date.now()-604800000]);
        await this.connection.query(`DELETE FROM sprint_battle WHERE state->>'invitation' IN ('declined','expired','blocked','withdrawn') AND "updatedAt"<now()-interval '30 days'`);
    }
    async respond(user:number,id:number,accept:boolean):Promise<SprintBattle>{
        if(typeof accept!=='boolean')throw new BadRequestException();
        return this.change(user,id,async(battle,manager)=>{
            if(user!==battle.recipientId)throw new ForbiddenException();
            if(battle.state.invitation===(accept?'accepted':'declined'))return;
            if(battle.state.invitation!=='pending'||battle.state.done||battle.state.invitationExpires<Date.now())throw new ConflictException('Invitation unavailable');
            await this.social.allowed(manager,battle.ownerId,battle.recipientId);
            battle.state.invitation=accept?'accepted':'declined';battle.state.done=!accept;
            if(!accept){await manager.query(`INSERT INTO player_social ("userId",visible,"onlineUntil",declined) VALUES ($1,false,0,1) ON CONFLICT ("userId") DO UPDATE SET declined=player_social.declined+1`,[user]);return;}
            battle.state.live=!!battle.state.live && await this.social.isOnline(manager,battle.ownerId,battle.recipientId) && await this.social.isOnline(manager,battle.recipientId,battle.ownerId);
        });
    }
    async progress(user:number,id:number,body:{legIndex:number;ms:number;units:number}):Promise<SprintBattle>{
        if(!body||!Number.isInteger(body.legIndex)||!Number.isInteger(body.ms)||!Number.isInteger(body.units)||body.ms<0||body.ms>90000||body.units<0||body.units>10000)throw new BadRequestException();
        await this.settleStale(user);
        return this.change(user,id,battle=>{
            if(battle.state.done)return;
            this.accepted(battle);const leg=battle.state.legs[body.legIndex];
            if(leg && leg.results[user])return;
            if(!battle.state.live||battle.state.done||body.legIndex!==battle.state.legs.length-1||!leg||!leg.liveStart||Date.now()<leg.liveStart||Date.now()>leg.liveStart+105000)throw new ConflictException('Live round unavailable');
            if(body.ms>Date.now()-leg.liveStart+1500)throw new BadRequestException('Progress ahead of clock');
            leg.progress=leg.progress||{};const prior=leg.progress[user];
            if(prior&&(prior.ms>body.ms||prior.units>body.units))throw new BadRequestException('Progress must increase');
            leg.progress[user]={ms:body.ms,units:body.units};
        });
    }
    async cancel(user:number,id:number,reason?:string):Promise<SprintBattle>{
        if(reason!==undefined && !['gave_up','connection'].includes(reason))throw new BadRequestException();
        return this.change(user,id,battle=>{
            if(battle.state.done)return;
            if(battle.state.invitation==='pending'){battle.state.invitation='withdrawn';battle.state.done=true;return;}
            this.accepted(battle);
            battle.state.done=true;battle.state.chooser=null;
            battle.state.winner=user===battle.ownerId?battle.recipientId:battle.ownerId;
            battle.state.endReason=reason||'gave_up';battle.state.forfeitedId=user;
        });
    }
    async reactions(user:number,id:number) {
        const battle=await this.connection.getRepository(SprintBattle).findOne(id);this.member(battle,user);
        return this.connection.transaction(async manager=>{
            await this.social.allowed(manager,battle.ownerId,battle.recipientId);
            const friends=(await manager.query('SELECT 1 FROM player_friend WHERE "lowId"=$1 AND "highId"=$2 AND accepted AND NOT declined',[Math.min(battle.ownerId,battle.recipientId),Math.max(battle.ownerId,battle.recipientId)])).length>0;
            const leg=battle.state.legs[battle.state.legs.length-1],now=Date.now();
            const started=leg.liveStart||leg.starts[user]||0;
            const playing=!battle.state.done && started>0 && now>=started && now<started+90000 && !leg.results[user];
            const visible=friends||!playing;
            const events=(battle.state.reactions||[]).filter(r=>r.at>now-600000);
            const own=events.filter(r=>r.sender===user);
            return {friends,playing,serverTime:now,cooldownUntil:own.length?own[own.length-1].at+5000:0,events:visible?events.filter(r=>r.sender!==user):[]};
        });
    }
    async react(user:number,id:number,key:string) {
        if(!['happy','angry','love','brain','clap','thumb'].includes(key))throw new BadRequestException('Unknown reaction');
        await this.change(user,id,async(battle,manager)=>{
            this.accepted(battle);await this.social.allowed(manager,battle.ownerId,battle.recipientId);
            if(battle.state.done && Date.now()-new Date(battle.updatedAt).getTime()>600000)throw new ConflictException('Reaction window ended');
            const now=Date.now();
            // Player locks in change() serialise sends even across different matches.
            const recent=await manager.query(`SELECT 1 FROM sprint_battle b, LATERAL jsonb_array_elements(COALESCE(b.state->'reactions','[]'::jsonb)) r WHERE (b."ownerId"=$1 OR b."recipientId"=$1) AND (r->>'sender')::int=$1 AND (r->>'at')::bigint>$2 LIMIT 1`,[user,now-5000]);
            if(recent.length)throw new ConflictException('Wait5seconds before sending another reaction');
            battle.state.reactions=(battle.state.reactions||[]).filter(r=>r.at>now-600000).slice(-31);
            battle.state.reactions.push({sender:user,key,at:now});
        });
        return this.reactions(user,id);
    }
}
