// Only disposable fixtures; never run against the live database.
require('reflect-metadata');const assert=require('assert'),fs=require('fs');
assert.equal(process.env.POSTGRES_DB,'memotopia_live_review_20261024');
const base=fs.existsSync('/app/dist/src/modules')?'/app/dist/src':'/app/dist';require('tsconfig-paths').register({baseUrl:base,paths:{'~*':['*']}});
const {createConnection}=require('typeorm'),{User}=require(base+'/common/db/entities/user.entity'),{UserStatistics}=require(base+'/common/db/entities/user-statistics.entity');
const {AchievementRepository}=require(base+'/modules/achievements/achievements.repository'),{AchievementsService}=require(base+'/modules/achievements/achievements.service');
const keys=['focus-first','focus-pairs-first','focus-pairs-professor','focus-boxes-first','focus-boxes-max','focus-flash-first','focus-flash-vision','focus-chain-first','focus-chain-max'];let connection;
(async()=>{
 connection=await createConnection({type:'postgres',host:process.env.POSTGRES_HOST,port:Number(process.env.POSTGRES_PORT||5432),username:process.env.POSTGRES_USER,password:process.env.POSTGRES_PASSWORD,database:process.env.POSTGRES_DB,entities:[base+'/common/db/entities/*.entity.js'],synchronize:false});
 const user=await connection.getRepository(User).save({email:'focus-awards@example.invalid',username:'focus_awards_fixture',provider:'EMAIL'});await connection.getRepository(UserStatistics).save({user,level:1,xp:37});
 const service=new AchievementsService(connection.getCustomRepository(AchievementRepository));await service.create({achievements:keys},user);await service.create({achievements:keys},user);
 assert.deepEqual((await service.find(user)).sort(),keys.slice().sort());assert.equal((await service.countForProfile(user.id)).count,9);assert.equal((await connection.getRepository(UserStatistics).findOne({where:{user}})).xp,37,'Badges do not invent an XP reward');
 await assert.rejects(()=>service.create({achievements:['focus-not-a-valid-award']},user));console.log('FOCUS_ACHIEVEMENTS_POSTGRES_PASSED:9valid awards, duplicate retries once, profile count, invalid rejection, no invented XP');
})().catch(e=>{console.error(e.stack);process.exitCode=1}).finally(async()=>{if(connection)await connection.close();});
