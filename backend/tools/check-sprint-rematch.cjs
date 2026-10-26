// Portable service regression, no database or real players. PostgreSQL checks live in check-live-social.
require('reflect-metadata');require('ts-node').register({transpileOnly:true});
require('tsconfig-paths').register({baseUrl:require('path').resolve('src'),paths:{'~*':['*']}});
const assert=require('assert');const {SprintService}=require('../src/modules/challenge/sprint.service');
const rows=[];let restricted=false;
const repo={findOne:async id=>rows.find(r=>r.id===id),save:async row=>{if(!row.id){row.id=rows.length+1;rows.push(row);}return row;}};
const query=async(sql,args)=>{
 if(sql.includes('COUNT(*)'))return [{n:restricted?5:0}];
 if(sql.startsWith('SELECT id FROM sprint_battle')){
  assert(sql.includes("state->>'done'='false'"),'Invitation reuse must exclude completed rows');
  const state=sql.includes("'pending'")?'pending':'accepted';return rows.filter(r=>!r.state.done&&r.state.invitation===state&&((r.ownerId===args[0]&&r.recipientId===args[1])||(r.ownerId===args[1]&&r.recipientId===args[0]))).slice(-1).map(r=>({id:r.id}));
 }
 assert(!sql.includes('SELECT 1 FROM sprint_battle'),'No blanket pair cooldown after a finished invitation');return [];
};
const manager={getRepository:()=>repo,query};
const connection={getRepository:()=>({findOne:async id=>({id,username:'fixture'+id})}),transaction:async fn=>fn(manager),query};
const service=new SprintService(connection,{lock:async()=>{},allowed:async()=>{},isOnline:async()=>true});
const create=(sender=1,recipient=2)=>service.create(sender,{recipientId:recipient,game:'Pairs',tier:6,mode:'single',version:2,live:true});
(async()=>{
 let first=await create();assert.equal((await create(2,1)).id,first.id);
 await service.respond(2,first.id,false);let next=await create();assert.notEqual(next.id,first.id,'Declined invitation must allow immediate rematch');
 await service.respond(2,next.id,true);assert.equal((await create(2,1)).id,next.id,'Active game reused in both directions');
 next.state.done=true;let third=await create();assert.notEqual(third.id,next.id,'Finished game must allow rematch');
 restricted=true;assert.equal((await create()).id,third.id,'Retries do not consume another rate-limit slot');
 await service.respond(2,third.id,false);await assert.rejects(()=>create(),/Please wait/,'Spam protection still applies to new invitations');
 restricted=false;assert.notEqual((await create(2,1)).id,third.id,'Reverse invitation after decline');
 console.log('SPRINT_REMATCH_VALIDATION_PASSED: completed/declined rematches, both directions, idempotent pending and active reuse, sender rate limit retained');
})().catch(error=>{console.error(error);process.exitCode=1;});
