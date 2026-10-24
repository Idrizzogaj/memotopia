const fs=require('fs'),vm=require('vm'),ts=require('typescript'),assert=require('assert');
function load(path,imports={},extra={}){const context={exports:{},require:p=>imports[p]||require(p),...extra};vm.createContext(context);vm.runInContext(ts.transpileModule(fs.readFileSync(path,'utf8'),{compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2020,experimentalDecorators:true,emitDecoratorMetadata:false}}).outputText,context);return context.exports;}
const rules=load('src/modules/challenge/sprint-rules.ts');
const decorator=()=>()=>{};const nest={Injectable:decorator,BadRequestException:Error,ForbiddenException:Error,NotFoundException:Error,ConflictException:Error};
let now=100000;const {SprintService}=load('src/modules/challenge/sprint.service.ts',{'@nestjs/common':nest,'@nestjs/typeorm':{InjectConnection:decorator},'typeorm':{MoreThan:value=>value},'~common/db/entities/sprint-battle.entity':{SprintBattle:class{}},'~common/db/entities/user.entity':{User:class{}},'~common/db/entities/game-level.entity':{GameLevel:class{}},'./sprint-rules':rules},{Date:{now:()=>now}});
let battle;const updates=[];const repo={async findOne(){return battle},async save(value){battle=value;return value}};
const connection={query:async()=>[],getRepository:()=>({async findOne(value){return typeof value==="number"?{id:2,username:"fixture"}:undefined},save:repo.save}),transaction:async fn=>fn({getRepository:()=>repo,query:async(sql,values)=>{if(sql.includes('UPDATE "user_statistics"'))updates.push(values);return sql.includes('COUNT(*)')?[{n:0}]:[]}})};
const service=new SprintService(connection,{lock:async()=>{},allowed:async()=>{},isOnline:async()=>false});const empty={completed:0,correct:0,turns:0,errors:0,roundMs:[],trace:[]};
async function create(user,body){const value=await service.create(user,{...body,version:2});await service.respond(body.recipientId,value.id,true);return value;}
async function play(resultA=empty,resultB=empty){const i=battle.state.legs.length-1,first=battle.state.legs[i].pacer,second=first===1?2:1,results={1:resultA,2:resultB};await service.start(first,1);await assert.rejects(()=>service.start(second,1));await assert.rejects(()=>service.submit(first,1,i,results[first]));now+=90000;await service.submit(first,1,i,results[first]);await service.start(second,1);now+=90000;await service.submit(second,1,i,results[second]);return i;}
(async()=>{
 await create(1,{recipientId:2,game:'Pairs',tier:6,mode:'match'});
 const first=await play();assert.equal(battle.state.chooser,2);assert.equal(battle.state.legs.length,1);assert.equal(battle.state.legs[0].winner,0);
 const beforeRetry=updates.length;await service.submit(2,1,first,empty);assert.equal(updates.length,beforeRetry);assert.equal(battle.state.legs.length,1);
 await assert.rejects(()=>service.choose(1,1,'Boxes'));await assert.rejects(()=>service.choose(2,1,'Pairs'));
 await service.choose(2,1,'Boxes');assert.equal(battle.state.legs[1].pacer,2);await play();assert.equal(battle.state.legs[2].pacer,1);assert.equal(battle.state.legs.length,3);assert(['Flash','Chain'].includes(battle.state.legs[2].game));
 await play();assert.equal(battle.state.done,true);assert.equal(battle.state.winner,0);assert.equal(battle.state.legs.length,3);
 for(const id of [1,2]){const u=updates.filter(v=>v[5]===id);assert.equal(u.reduce((sum,v)=>sum+v[0],0),270);assert.equal(u.reduce((sum,v)=>sum+v[1],0),1);assert.equal(u.reduce((sum,v)=>sum+v[4],0),1);} await assert.rejects(()=>service.start(3,1)); await assert.rejects(()=>create(1,{recipientId:2,game:'Pairs',tier:21,mode:'single'}),/Clear level20/);
 await create(1,{recipientId:2,game:'Pairs',tier:6,mode:'single'});await play();assert.equal(battle.state.done,true);assert.equal(battle.state.winner,0);
 const progress={completed:0,correct:1,turns:2,errors:0,roundMs:[],trace:[{ms:100,units:1}]}; await create(1,{recipientId:2,game:'Pairs',tier:6,mode:'match'});await play(progress,empty);await service.choose(2,1,'Boxes');await play(progress,empty);assert.equal(battle.state.done,true);assert.equal(battle.state.winner,1);assert.equal(battle.state.legs.length,2); console.log('SPRINT_SERVICE_VALIDATION_PASSED: half points,3draws,chooser ownership,distinct games,random remaining,start deadline,submit retry,nonmember');
})().catch(e=>{console.error(e);process.exitCode=1});



