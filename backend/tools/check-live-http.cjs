require('reflect-metadata');const assert=require('assert'),http=require('http'),jwt=require('jsonwebtoken'),fs=require('fs');
assert.equal(process.env.POSTGRES_DB,'memotopia_live_review_20261024','Only isolated review database');
const base=fs.existsSync('/app/dist/src/modules')?'/app/dist/src':'/app/dist';require('tsconfig-paths').register({baseUrl:base,paths:{'~*':['*']}});
const {createConnection}=require('typeorm'),{User}=require(base+'/common/db/entities/user.entity'),{UserStatistics}=require(base+'/common/db/entities/user-statistics.entity');
function request(path,token,body){return new Promise((resolve,reject)=>{const data=body===undefined?null:JSON.stringify(body);const req=http.request({hostname:'127.0.0.1',port:Number(process.env.PORT||80),path,method:data?'POST':'GET',headers:{'Content-Type':'application/json',...(token?{Authorization:'Bearer '+token}:{})}},res=>{let text='';res.on('data',c=>text+=c);res.on('end',()=>{let value;try{value=JSON.parse(text)}catch(_){value=text}resolve({status:res.statusCode,body:value});});});req.setTimeout(10000,()=>req.destroy(new Error('Timeout')));req.on('error',reject);if(data)req.write(data);req.end();});}
let connection;
(async()=>{
 connection=await createConnection({type:'postgres',host:process.env.POSTGRES_HOST,port:Number(process.env.POSTGRES_PORT||5432),username:process.env.POSTGRES_USER,password:process.env.POSTGRES_PASSWORD,database:process.env.POSTGRES_DB,entities:[base+'/common/db/entities/*.entity.js'],synchronize:false});
 const users=[];for(let i=0;i<3;i++){const u=await connection.getRepository(User).save({email:'live-http-'+i+'@example.invalid',username:'live_http_'+i,provider:'EMAIL',emailVerificationPending:false});users.push(u);await connection.getRepository(UserStatistics).save({user:u,level:15,xp:1466});}
 const [a,b,c]=users;const tokens=users.map(u=>jwt.sign({userId:u.id,username:u.username},process.env.JWT_SECRET,{expiresIn:'5m'}));const [ta,tb,tc]=tokens;
 for(const path of ['/sprints','/sprints/social','/sprints/social/online'])assert.equal((await request(path)).status,401,path);
 assert.equal((await request('/sprints/social',ta)).status,200,'Social route must precede id route');
 const created=await request('/sprints',ta,{recipientId:b.id,game:'Chain',tier:6,mode:'match',version:2,live:false});assert.equal(created.status,201);assert.equal(created.body.invitation,'pending');const id=created.body.id;
 assert.equal((await request('/sprints/'+id,tc)).status,403);
 assert.equal((await request('/sprints/'+id+'/start',ta,{})).status,409);
 assert.equal((await request('/sprints/'+id+'/respond',ta,{accept:true})).status,403);
 assert.equal((await request('/sprints/'+id+'/respond',tb,{accept:false})).status,201);
 assert.equal((await request('/sprints/'+id+'/respond',tb,{accept:false})).status,201);
 const sa=await request('/sprints/social',ta),sb=await request('/sprints/social',tb);assert.equal(sa.body.declined,0);assert.equal(sb.body.declined,1);
 for(const t of [ta,tb]){const stats=await request('/user-statistics',t);assert.equal(stats.status,200);assert.equal(stats.body.numberOfChallenges,0);assert.equal(stats.body.numberOfMatchChallenges,0);}
 assert.equal((await request('/sprints/social/block',tb,{targetId:a.id,blocked:true})).status,201);
 assert.equal((await request('/sprints',ta,{recipientId:b.id,game:'Pairs',tier:6,mode:'single',version:2})).status,403);
 assert.equal((await request('/sprints/social/friend',ta,{targetId:b.id,action:'request'})).status,403);
 await request('/sprints/social/block',tb,{targetId:a.id,blocked:false});
 assert.equal((await request('/sprints/social/friend',ta,{targetId:b.id,action:'request'})).status,201);
 assert.equal((await request('/sprints/social/friend',tb,{targetId:a.id,action:'accept'})).status,201);
 for(const t of [ta,tb])assert.equal((await request('/sprints/social/heartbeat',t,{active:true,visible:false,friendsVisible:true})).status,201);
 const online=await request('/sprints/social/online',ta);assert(online.body.players.some(p=>p.id===b.id && p.friend));assert(!(await request('/sprints/social/online',tc)).body.players.some(p=>p.id===b.id));
 await connection.query('UPDATE sprint_battle SET "createdAt"=now()-interval \'2 minutes\'');
 const live=await request('/sprints',ta,{recipientId:b.id,game:'Flash',tier:6,mode:'single',version:2,live:true});assert.equal(live.status,201);assert(live.body.live);const lid=live.body.id;
 assert.equal((await request('/sprints/'+lid+'/respond',tb,{accept:true})).status,201);
 assert.equal((await request('/sprints/'+lid+'/start',ta,{})).body.legs[0].liveStart,0);const started=await request('/sprints/'+lid+'/start',tb,{});assert.equal(started.status,201);assert.equal(started.body.legs[0].ownerStart,started.body.legs[0].recipientStart);assert(started.body.legs[0].liveStart>Date.now());
 const [record]=await connection.query('SELECT state FROM sprint_battle WHERE id=$1',[lid]);record.state.legs[0].liveStart=Date.now()-90001;record.state.legs[0].starts[a.id]=record.state.legs[0].liveStart;record.state.legs[0].starts[b.id]=record.state.legs[0].liveStart;await connection.query('UPDATE sprint_battle SET state=$1::jsonb WHERE id=$2',[JSON.stringify(record.state),lid]);
 const empty={completed:0,correct:0,turns:0,errors:0,roundMs:[],trace:[]};assert.equal((await request('/sprints/'+lid+'/result',ta,{legIndex:0,result:empty})).status,201);const final=await request('/sprints/'+lid+'/result',tb,{legIndex:0,result:empty});assert.equal(final.status,201);assert(final.body.done);assert.equal(final.body.winner,0);
 const stats=await request('/user-statistics',tb);assert.equal(stats.body.numberOfChallenges,1);assert.equal(stats.body.numberOfSingleChallenges,1);
 for(const [sender,recipient,st,rt] of [[a,b,ta,tb],[b,a,tb,ta]]){
  await connection.query('UPDATE sprint_battle SET "createdAt"=now()-interval \'2 minutes\'');
  for(const t of [st,rt])await request('/sprints/social/heartbeat',t,{active:true,visible:false,friendsVisible:true});
  const invitation=await request('/sprints',st,{recipientId:recipient.id,game:'Boxes',tier:6,mode:'single',version:2,live:true});assert.equal(invitation.status,201);assert(invitation.body.live);const gameId=invitation.body.id;
  const duplicate=await request('/sprints',rt,{recipientId:sender.id,game:'Chain',tier:6,mode:'single',version:2,live:true});assert.equal(duplicate.status,201);assert.equal(duplicate.body.id,gameId);assert.equal(duplicate.body.invitation,'pending');
  await request('/sprints/'+gameId+'/respond',rt,{accept:true});await request('/sprints/'+gameId+'/start',st,{});await request('/sprints/'+gameId+'/start',rt,{});
  const gaveUp=await request('/sprints/'+gameId+'/cancel',st,{});assert.equal(gaveUp.status,201);assert.equal(gaveUp.body.endReason,'gave_up');assert.equal(gaveUp.body.forfeitedId,sender.id);
  for(const path of ['result','start','progress']){const response=await request('/sprints/'+gameId+'/'+path,rt,path==='result'?{legIndex:0,result:empty}:path==='progress'?{legIndex:0,ms:0,units:0}:{});assert.equal(response.status,201);assert(response.body.done);assert.equal(response.body.winner,recipient.id);}
 }
 console.log('LIVE_HTTP_PASSED: authenticated routing, both invitation directions, idempotent crossed invitations, give-up receipts, late result/start/progress acknowledgement, shared live deadline and original social/statistics checks');
})().catch(e=>{console.error(e.stack);process.exitCode=1}).finally(async()=>{if(connection)await connection.close();});
