import { BadRequestException, ConflictException, ForbiddenException, Injectable } from '@nestjs/common';
import { InjectConnection } from '@nestjs/typeorm';
import { Connection, EntityManager } from 'typeorm';

// Presence is a short lease, never a permanent last-seen history. All reads are authenticated.
@Injectable()
export class SocialService {
    constructor(@InjectConnection() private connection: Connection) {}
    async lock(manager: EntityManager, a: number, b: number): Promise<void> {
        for (const id of [a,b].sort((x,y)=>x-y).filter((v,i,s)=>i===0||v!==s[i-1]))
            await manager.query('SELECT pg_advisory_xact_lock(179280,$1)',[id]);
    }
    async allowed(manager: EntityManager, a: number, b: number): Promise<void> {
        if ((await manager.query('SELECT 1 FROM player_block WHERE ("ownerId"=$1 AND "targetId"=$2) OR ("ownerId"=$2 AND "targetId"=$1)',[a,b])).length)
            throw new ForbiddenException('Player unavailable');
    }
    async heartbeat(user:number, body:{visible?:boolean;friendsVisible?:boolean;active?:boolean}) {
        if(!body || typeof body.active!=='boolean' || (body.visible!==undefined && typeof body.visible!=='boolean') || (body.friendsVisible!==undefined && typeof body.friendsVisible!=='boolean'))throw new BadRequestException();
        await this.connection.query(`INSERT INTO player_social ("userId",visible,"onlineUntil",declined) VALUES ($1,COALESCE($2,false),0,0) ON CONFLICT ("userId") DO NOTHING`,[user,body.visible===undefined?null:body.visible]);
        await this.connection.query(`UPDATE player_social SET visible=COALESCE($2,visible),"friendsVisible"=COALESCE($5,"friendsVisible"),"onlineUntil"=CASE WHEN $3 THEN $4::bigint ELSE 0::bigint END WHERE "userId"=$1`,[user,body.visible===undefined?null:body.visible,body.active,Date.now()+45000,body.friendsVisible===undefined?null:body.friendsVisible]);
        return this.settings(user);
    }
    async settings(user:number) {
        const rows=await this.connection.query('SELECT visible,"friendsVisible",declined FROM player_social WHERE "userId"=$1',[user]);
        const blocked=await this.connection.query('SELECT b."targetId" AS id,u.username FROM player_block b JOIN "user" u ON u.id=b."targetId" WHERE b."ownerId"=$1 ORDER BY u.username LIMIT 500',[user]);
        const friends=await this.connection.query(`SELECT u.id,u.username,f.accepted,f."requesterId" FROM player_friend f JOIN "user" u ON u.id=CASE WHEN f."lowId"=$1 THEN f."highId" ELSE f."lowId" END WHERE (f."lowId"=$1 OR f."highId"=$1) AND NOT f.declined AND (f.accepted OR f."requestedAt">$2) ORDER BY u.username LIMIT 500`,[user,Date.now()-604800000]);
        return {friends,friendsVisible:rows.length?rows[0].friendsVisible:true,visible:rows.length?rows[0].visible:false,declined:rows.length?rows[0].declined:0,blocked};
    }
    async online(user:number) {
        return {players:await this.connection.query(`SELECT u.id,u.username,EXISTS(SELECT 1 FROM favorites f WHERE f."requesterId"=$1 AND f."favoritesId"=u.id) AS favorite,EXISTS(SELECT 1 FROM player_friend f WHERE f.accepted AND f."lowId"=LEAST($1,u.id) AND f."highId"=GREATEST($1,u.id)) AS friend FROM player_social p JOIN "user" u ON u.id=p."userId" WHERE (CASE WHEN EXISTS(SELECT 1 FROM player_friend f WHERE f.accepted AND f."lowId"=LEAST($1,u.id) AND f."highId"=GREATEST($1,u.id)) THEN p."friendsVisible" ELSE p.visible END) AND p."onlineUntil">$2 AND u.id<>$1 AND NOT EXISTS(SELECT 1 FROM player_block b WHERE (b."ownerId"=$1 AND b."targetId"=u.id) OR (b."ownerId"=u.id AND b."targetId"=$1)) ORDER BY friend DESC,favorite DESC,u.id LIMIT 100`,[user,Date.now()])};
    }
    async isOnline(manager:EntityManager,user:number,viewer:number):Promise<boolean>{
        return (await manager.query(`SELECT 1 FROM player_social p WHERE p."userId"=$1 AND p."onlineUntil">$3 AND (CASE WHEN EXISTS(SELECT 1 FROM player_friend f WHERE f.accepted AND f."lowId"=LEAST($1,$2) AND f."highId"=GREATEST($1,$2)) THEN p."friendsVisible" ELSE p.visible END)`,[user,viewer,Date.now()])).length>0;
    }
    async friend(user:number,target:number,action:string){
        if(!Number.isInteger(target)||target<=0||target===user||!['request','accept','decline','remove'].includes(action))throw new BadRequestException();
        await this.connection.transaction(async manager=>{
            await this.lock(manager,user,target);await this.allowed(manager,user,target);
            const pair=[Math.min(user,target),Math.max(user,target)];
            const rows=await manager.query('SELECT * FROM player_friend WHERE "lowId"=$1 AND "highId"=$2',pair);const row=rows[0];
            if(action==='request'){
                if(row && (row.accepted||(!row.declined && Number(row.requestedAt)>Date.now()-604800000)))return;
                if(row && Number(row.requestedAt)>Date.now()-60000)throw new ConflictException('Please wait before inviting again');
                if(!(await manager.query('SELECT id FROM "user" WHERE id=$1',[target])).length)throw new BadRequestException();
                await manager.query('INSERT INTO player_social ("userId") VALUES ($1) ON CONFLICT DO NOTHING',[user]);
                const counts=await manager.query('SELECT "friendRequests","friendWindow" FROM player_social WHERE "userId"=$1',[user]);
                const fresh=Number(counts[0].friendWindow)>Date.now()-3600000;
                if(fresh && counts[0].friendRequests>=10)throw new ConflictException('Please wait before sending more friend requests');
                await manager.query('UPDATE player_social SET "friendWindow"=$2,"friendRequests"=$3 WHERE "userId"=$1',[user,fresh?Number(counts[0].friendWindow):Date.now(),fresh?counts[0].friendRequests+1:1]);
                if((await manager.query('SELECT 1 FROM player_friend WHERE "lowId"=$1 OR "highId"=$1',[user])).length>=500)throw new ConflictException('Friend list limit reached');
                await manager.query(`INSERT INTO player_friend ("lowId","highId","requesterId",accepted,"requestedAt") VALUES ($1,$2,$3,false,$4) ON CONFLICT ("lowId","highId") DO UPDATE SET "requesterId"=$3,accepted=false,declined=false,"requestedAt"=$4`,[...pair,user,Date.now()]);
            }else if(action==='accept'){
                if(!row||row.declined||row.requesterId===user||(!row.accepted && Number(row.requestedAt)<Date.now()-604800000))throw new ConflictException('Friend request unavailable');
                await manager.query('UPDATE player_friend SET accepted=true WHERE "lowId"=$1 AND "highId"=$2',pair);
            }else{
                if(action==='decline' && row && (row.accepted||row.requesterId===user))throw new ConflictException();
                await manager.query('UPDATE player_friend SET declined=true,accepted=false WHERE "lowId"=$1 AND "highId"=$2',pair);
            }
        });return this.settings(user);
    }
    async block(user:number,target:number,blocked:boolean) {
        if(!Number.isInteger(target)||target<=0||target===user||typeof blocked!=='boolean')throw new BadRequestException();
        await this.connection.transaction(async manager=>{
            await this.lock(manager,user,target);
            if(blocked){
                if(!(await manager.query('SELECT id FROM "user" WHERE id=$1',[target])).length)throw new BadRequestException();
                if((await manager.query('SELECT 1 FROM player_block WHERE "ownerId"=$1',[user])).length>=500)throw new ConflictException('Block list limit reached');
                await manager.query('INSERT INTO player_block ("ownerId","targetId") VALUES ($1,$2) ON CONFLICT DO NOTHING',[user,target]);
                await manager.query('DELETE FROM player_friend WHERE "lowId"=$1 AND "highId"=$2',[Math.min(user,target),Math.max(user,target)]);
                // Pending invitations disappear without awarding a win or a loss.
                await manager.query(`UPDATE sprint_battle SET state=jsonb_set(jsonb_set(state,'{invitation}','"blocked"'),'{done}','true'),"updatedAt"=now() WHERE (("ownerId"=$1 AND "recipientId"=$2) OR ("ownerId"=$2 AND "recipientId"=$1)) AND state->>'invitation'='pending'`,[user,target]);
            }else await manager.query('DELETE FROM player_block WHERE "ownerId"=$1 AND "targetId"=$2',[user,target]);
        });return this.settings(user);
    }
}
