// Integration test: an isolated database is mandatory; no production user records.
require('reflect-metadata');const assert=require('assert'),fs=require('fs');
const db='memotopia_live_review_20261024';assert.equal(process.env.POSTGRES_DB,db,'Refuse a live database');
const base=fs.existsSync('/app/dist/src/modules')?'/app/dist/src':'/app/dist';
require('tsconfig-paths').register({baseUrl:base,paths:{'~*':['*']}});
const {createConnection}=require('typeorm');const {User}=require(base+'/common/db/entities/user.entity');
const {UserStatistics}=require(base+'/common/db/entities/user-statistics.entity');
const {SprintService}=require(base+'/modules/challenge/sprint.service');const {SocialService}=require(base+'/modules/challenge/social.service');
const empty={completed:0,correct:0,turns:0,errors:0,roundMs:[],trace:[]};
const realNow=Date.now;let now=realNow(),conn;Date.now=()=>now;
(async()=>{
 conn=await createConnection({type:'postgres',host:process.env.POSTGRES_HOST,port:Number(process.env.POSTGRES_PORT||5432),username:process.env.POSTGRES_USER,password:process.env.POSTGRES_PASSWORD,database:db,entities:[base+'/common/db/entities/*.entity.js'],synchronize:true,logging:false});
 assert.equal((await conn.query('SELECT current_database() AS n'))[0].n,db);
 const migration=new(require(base+'/common/db/migrations/1792831800000-AddLiveSocial').AddLiveSocial1792831800000)();const runner=conn.createQueryRunner();await migration.up(runner);await migration.up(runner);await runner.release();
 const ids=[];for(let i=0;i<3;i++){const u=await conn.getRepository(User).save({email:'live-review-'+i+'@example.invalid',username:'live_fixture_'+i,provider:'EMAIL'});ids.push(u.id);await conn.getRepository(UserStatistics).save({user:u,level:15,xp:1466});}
 const [a,b,c]=ids,social=new SocialService(conn),sprint=new SprintService(conn,social);
 const stats=async id=>(await conn.query('SELECT "numberOfChallenges","numberOfDrawChallenges","numberOfWinChallenges","numberOfLossChallenges" FROM user_statistics WHERE "userId"=$1',[id]))[0];
 const zero=await stats(a);assert.equal((await social.settings(a)).visible,false);assert.equal((await social.settings(a)).friendsVisible,true);
 for(const id of ids)await social.heartbeat(id,{active:true});
 await conn.query('INSERT INTO favorites ("requesterId","favoritesId") VALUES ($1,$2)',[c,a]);
 assert.equal((await social.online(c)).players.length,0,'Favorite does not grant presence');
 await social.friend(a,b,'request');await assert.rejects(()=>social.friend(a,b,'accept'));
 assert.equal((await social.online(b)).players.length,0,'Pending request grants no presence');
 await social.friend(b,a,'request');assert.equal((await social.settings(a)).friends[0].accepted,false,'Cross-request never auto-accepts');
 await social.friend(b,a,'accept');assert((await social.online(b)).players.some(p=>p.id===a&&p.friend));assert((await social.online(a)).players.some(p=>p.id===b));
 await social.heartbeat(a,{active:true,friendsVisible:false,visible:true});assert(!(await social.online(b)).players.some(p=>p.id===a));assert((await social.online(c)).players.some(p=>p.id===a));
 await social.heartbeat(a,{active:true,friendsVisible:true,visible:false});
 async function create(game='Pairs',live=true){await conn.query('UPDATE sprint_battle SET "createdAt"=now()-interval \'2 minutes\'');return sprint.create(a,{recipientId:b,game,tier:6,mode:'single',version:2,live});}
 let battle=await create();assert.equal(battle.state.invitation,'pending');assert(battle.state.live);await assert.rejects(()=>sprint.start(a,battle.id));await assert.rejects(()=>sprint.respond(a,battle.id,true));await assert.rejects(()=>sprint.read(c,battle.id));
 await Promise.all([sprint.respond(b,battle.id,false),sprint.respond(b,battle.id,false)]);assert.equal((await social.settings(b)).declined,1);assert.deepEqual(await stats(a),zero);assert.deepEqual(await stats(b),zero);
 battle=await create();await sprint.respond(b,battle.id,true);assert.deepEqual(await stats(a),zero);
 battle=await sprint.start(a,battle.id);assert.equal(battle.state.legs[0].liveStart,undefined);battle=await sprint.start(b,battle.id);const start=battle.state.legs[0].liveStart;assert.equal(start,now+4000);assert.equal(battle.state.legs[0].starts[a],battle.state.legs[0].starts[b]);
 now=start+500;await sprint.progress(a,battle.id,{legIndex:0,ms:500,units:1});assert.equal((await sprint.read(b,battle.id)).state.legs[0].progress[a].units,1);await assert.rejects(()=>sprint.progress(c,battle.id,{legIndex:0,ms:500,units:1}));await assert.rejects(()=>sprint.progress(a,battle.id,{legIndex:0,ms:499,units:0}));await assert.rejects(()=>sprint.submit(a,battle.id,0,empty));
 now=start+90000;await Promise.all([sprint.submit(a,battle.id,0,empty),sprint.submit(a,battle.id,0,empty)]);await sprint.submit(b,battle.id,0,empty);assert.equal((await stats(a)).numberOfChallenges,1);assert.equal((await stats(b)).numberOfDrawChallenges,1);
 await social.heartbeat(a,{active:true});await social.heartbeat(b,{active:true});
 battle=await create('Chain');await social.block(b,a,true);assert.equal((await sprint.read(a,battle.id)).state.invitation,'blocked');await assert.rejects(()=>sprint.respond(b,battle.id,true));assert.equal((await social.settings(b)).friends.length,0);assert.equal((await social.online(a)).players.filter(p=>p.id===b).length,0);
 await assert.rejects(()=>create());await assert.rejects(()=>social.friend(a,b,'request'));await social.block(b,a,false);assert.equal((await social.settings(b)).friends.length,0,'Unblock does not restore friends');
 await social.heartbeat(a,{active:true,visible:true});await social.heartbeat(b,{active:true,visible:true});
 battle=await create('Flash');await sprint.respond(b,battle.id,true);await sprint.start(a,battle.id);battle=await sprint.start(b,battle.id);now=battle.state.legs[0].liveStart+90000;await sprint.submit(a,battle.id,0,empty);now+=61000;await sprint.read(a,battle.id);await sprint.read(b,battle.id);assert.equal((await stats(a)).numberOfWinChallenges,1);assert.equal((await stats(b)).numberOfLossChallenges,1);
 assert.equal((await social.online(c)).players.length,0,'Lease expires with no background heartbeat');
 // Declining after sending must not reset pair cooldown or permit a refusal counter to leak.
 await social.friend(a,c,'request');await social.friend(c,a,'decline');await assert.rejects(()=>social.friend(a,c,'request'));
 assert(!('declined' in (await social.online(c)).players));
 await conn.query('DELETE FROM favorites WHERE "requesterId"=$1 OR "favoritesId"=$1',[a]);await conn.getRepository(User).delete(a);for(const table of ['player_social','player_block','player_friend']){const cols=table==='player_social'?'"userId"':table==='player_block'?'"ownerId"':'"lowId"';assert.equal((await conn.query('SELECT 1 FROM '+table+' WHERE '+cols+'=$1',[a])).length,0);}
 console.log('LIVE_SOCIAL_POSTGRES_PASSED: friend consent, independent privacy, favorites, blocks, cooldown, concurrent decline/submit, shared start, progress, disconnect settlement, account deletion');
})().catch(e=>{console.error(e.stack);if(e.query)console.error(e.query);process.exitCode=1}).finally(async()=>{Date.now=realNow;if(conn)await conn.close();});
