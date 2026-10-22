const fs=require('fs'),vm=require('vm'),ts=require('typescript'),jwt=require('jsonwebtoken'),assert=require('assert');
function method(file,name){
 const source=fs.readFileSync(file,'utf8');
 const start=source.indexOf('    '+name+'(');
 assert(start>=0,name);
 const end=source.indexOf('\n    }',start)+6;
 const code='class Subject { '+source.slice(start,end).trim()+' } globalThis.Subject=Subject;';
 const context={sign:jwt.sign};vm.createContext(context);
 vm.runInContext(ts.transpileModule(code,{compilerOptions:{target:ts.ScriptTarget.ES2020}}).outputText,context);
 return context.Subject;
}
const Auth=method('src/modules/auth/auth.service.ts','generateAccessToken');
const auth=new Auth();auth.configService={get:()=> 'test-secret'};
const decoded=jwt.verify(auth.generateAccessToken({id:123,username:'test'}),'test-secret');
assert.equal(decoded.exp-decoded.iat,365*86400);
const Profile=method('src/modules/achievements/achievements.service.ts','async countForProfile');
const profile=new Profile();let supplied;
const query={select(){return this},where(sql,args){supplied=args;return this},async getRawOne(){return {count:'7'}}};
profile.achievementRepository={createQueryBuilder:()=>query};
profile.countForProfile(123).then(result=>{assert.equal(result.count,7);assert.equal(supplied.userId,123);console.log('SESSION_EXPIRY_AND_PROFILE_COUNT_PASSED');}).catch(error=>{console.error(error);process.exitCode=1;});
