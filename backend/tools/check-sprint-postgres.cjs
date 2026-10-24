// Runs only against this disposable review database, never the live database.
require('reflect-metadata');require('dotenv').config({path:'.env.local'});
const fs=require('fs'),assert=require('assert');
const reviewDb='memotopia_sprint_review_20261023';assert.equal(process.env.POSTGRES_DB,reviewDb,'Refuse to test a live database');
const compiled=fs.existsSync('/app/dist/src/modules')?'/app/dist/src':'/app/dist';
require('tsconfig-paths').register({baseUrl:compiled,paths:{'~*':['*']}});
const {createConnection}=require('typeorm');
const {User}=require(compiled+'/common/db/entities/user.entity');const {UserStatistics}=require(compiled+'/common/db/entities/user-statistics.entity');
const {SprintService}=require(compiled+'/modules/challenge/sprint.service');const {AchievementsService}=require(compiled+'/modules/achievements/achievements.service');const {AchievementRepository}=require(compiled+'/modules/achievements/achievements.repository');
let connection;let now=100000;const realNow=Date.now;Date.now=()=>now;
const empty={completed:0,correct:0,turns:0,errors:0,roundMs:[],trace:[]};
(async()=>{
 connection=await createConnection({type:'postgres',host:process.env.POSTGRES_HOST,port:Number(process.env.POSTGRES_PORT||5432),username:process.env.POSTGRES_USER,password:process.env.POSTGRES_PASSWORD,database:reviewDb,entities:[compiled+'/common/db/entities/*.entity.js'],synchronize:true,logging:false});
 assert.equal((await connection.query('SELECT current_database() AS name'))[0].name,reviewDb);
 const runner=connection.createQueryRunner();const {AddSprintBattles1792706400000}=require(compiled+'/common/db/migrations/1792706400000-AddSprintBattles');const migration=new AddSprintBattles1792706400000();await migration.down(runner);await migration.up(runner);await migration.up(runner);await runner.release();
 const users=[];for(let i=0;i<2;i++){const user=await connection.getRepository(User).save({email:'sprint-review-'+i+'@example.invalid',username:'sprint_review_'+i,provider:'EMAIL'});users.push(user);await connection.getRepository(UserStatistics).save({user,level:15,xp:1466});}
 const service=new SprintService(connection,new(require(compiled+'/modules/challenge/social.service').SocialService)(connection)),a=users[0].id,b=users[1].id;let battle=await service.create(a,{recipientId:b,game:'Pairs',tier:6,mode:'match',version:2});await service.respond(b,battle.id,true);
 async function play(first,second){const index=battle.state.legs.length-1;await service.start(first,battle.id);await assert.rejects(()=>service.start(second,battle.id));now+=90000;await Promise.all([service.submit(first,battle.id,index,empty),service.submit(first,battle.id,index,empty)]);await service.start(second,battle.id);now+=90000;await Promise.all([service.submit(second,battle.id,index,empty),service.submit(second,battle.id,index,empty)]);battle=await service.read(a,battle.id);}
 await play(a,b);assert.equal(battle.state.chooser,b);battle=await service.choose(b,battle.id,'Boxes');assert.equal(battle.state.legs[1].pacer,b);await play(b,a);assert.equal(battle.state.legs[2].pacer,a);await play(a,b);assert.equal(battle.state.done,true);assert.equal(battle.state.winner,0);assert.equal(battle.state.legs.length,3);
 for(const user of users){const stats=await connection.getRepository(UserStatistics).findOne({where:{user}});assert.equal(stats.timePlayed,270);assert.equal(stats.numberOfChallenges,1);assert.equal(stats.numberOfDrawChallenges,1);assert.equal(stats.xp,1466);}
 const rewards=new AchievementsService(connection.getCustomRepository(AchievementRepository));await Promise.all([rewards.create({achievements:['athlete-one-heart-chain']},users[0]),rewards.create({achievements:['athlete-one-heart-chain']},users[0])]);
 const stats=await connection.getRepository(UserStatistics).findOne({where:{user:users[0]}});assert.equal(stats.xp,1966);assert.equal(stats.level,20);assert.equal((await rewards.countForProfile(a)).count,1);
 console.log('SPRINT_POSTGRES_VALIDATION_PASSED: additive migration, real row locks, concurrent retry receipts, chooser pace,3 draws, stats once,500 XP once,profile count.');
})().catch(e=>{console.error(e.name+': '+e.message);process.exitCode=1}).finally(async()=>{Date.now=realNow;if(connection)await connection.close();});
