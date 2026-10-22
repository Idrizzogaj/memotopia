require('reflect-metadata');require('dotenv').config({path:'.env.local'});
const assert=require('assert'),http=require('http'),jwt=require('jsonwebtoken'),{createConnection}=require('typeorm');
assert.equal(process.env.POSTGRES_DB,'memotopia_sprint_review_20261023','Only the review database is allowed');
const empty={completed:0,correct:0,turns:0,errors:0,roundMs:[],trace:[]};
function request(path,token,body){return new Promise((resolve,reject)=>{const data=body===undefined?null:JSON.stringify(body);const req=http.request({hostname:'127.0.0.1',port:Number(process.env.PORT||80),path,method:data?'POST':'GET',headers:{'Content-Type':'application/json',...(token?{Authorization:'Bearer '+token}:{})}},res=>{let text='';res.on('data',c=>text+=c);res.on('end',()=>{let value;try{value=JSON.parse(text)}catch(_){value=text}resolve({status:res.statusCode,body:value});});});req.setTimeout(10000,()=>req.destroy(new Error('HTTP timeout')));req.on('error',reject);if(data)req.write(data);req.end();});}
let connection;
(async()=>{
 connection=await createConnection({type:'postgres',host:process.env.POSTGRES_HOST,port:Number(process.env.POSTGRES_PORT||5432),username:process.env.POSTGRES_USER,password:process.env.POSTGRES_PASSWORD,database:process.env.POSTGRES_DB});
 const users=await connection.query('SELECT id,username FROM "user" WHERE username IN ($1,$2) ORDER BY id',['sprint_review_0','sprint_review_1']);assert.equal(users.length,2);
 const token=u=>jwt.sign({userId:u.id,username:u.username},process.env.JWT_SECRET,{expiresIn:'5m'});const a=token(users[0]),b=token(users[1]);
 assert.equal((await request('/sprints')).status,401);
 const session=await request('/auth/session',a,{});assert.equal(session.status,201);const decoded=jwt.verify(session.body.accessToken,process.env.JWT_SECRET);assert.equal(decoded.exp-decoded.iat,365*86400);
 const profile=await request('/achievements/users/'+users[0].id+'/count',a);assert.equal(profile.status,200);assert.equal(profile.body.count,1);
 const created=await request('/sprints',a,{recipientId:users[1].id,game:'Chain',tier:6,mode:'single'});assert.equal(created.status,201);const id=created.body.id;assert.equal(created.body.legs[0].ownerPlayed,false);assert.equal(created.body.legs[0].ownerResult,null);
 assert.equal((await request('/sprints/'+id+'/start',b,{})).status,409);assert.equal((await request('/sprints/'+id+'/start',a,{})).status,201);
 async function expire(user){const [record]=await connection.query('SELECT state FROM sprint_battle WHERE id=$1',[id]);record.state.legs[0].starts[user]=Date.now()-90001;await connection.query('UPDATE sprint_battle SET state=$1::jsonb WHERE id=$2',[JSON.stringify(record.state),id]);}
 await expire(users[0].id);const first=await request('/sprints/'+id+'/result',a,{legIndex:0,result:empty});assert.equal(first.status,201);assert.equal(first.body.legs[0].ownerPlayed,true);assert.equal(first.body.legs[0].recipientPlayed,false);
 assert.equal((await request('/sprints/'+id+'/start',b,{})).status,201);await expire(users[1].id);const final=await request('/sprints/'+id+'/result',b,{legIndex:0,result:empty});assert.equal(final.status,201);assert.equal(final.body.done,true);assert.equal(final.body.winner,0);
 console.log('SPRINT_HTTP_VALIDATION_PASSED: JWT protection,365-day renewal,profile count,Chain create,pace gate,wire receipt flags,completed draw.');
})().catch(e=>{console.error(e.name+': '+e.message);process.exitCode=1}).finally(async()=>{if(connection)await connection.close();});
