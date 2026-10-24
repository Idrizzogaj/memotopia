const fs=require('fs'),vm=require('vm'),ts=require('typescript'),assert=require('assert');
const text=fs.readFileSync('src/modules/user-statistics/user-statistics.service.ts','utf8');
const start=text.indexOf('    async getChallengeSummary('),end=text.indexOf('\n    }',start)+6;
const context={};vm.createContext(context);vm.runInContext(ts.transpileModule('class Subject {'+text.slice(start,end)+'} globalThis.Subject=Subject;',{compilerOptions:{target:ts.ScriptTarget.ES2017}}).outputText,context);
(async()=>{
 for(const [total,matches] of [[0,0],[12,4],[500,204],[205,205]]){
  const subject=new context.Subject();let query,args;
  subject.getByUser=async()=>({numberOfChallenges:total,numberOfLossChallenges:2,numberOfDrawChallenges:1});
  subject.userStatisticsRepository={query:async(sql,params)=>{query=sql;args=params;return [{matches,games:total,losses:2,draws:1}];}};
  const result=await subject.getChallengeSummary({id:123});
  assert.equal(result.numberOfChallenges,total);assert.equal(result.numberOfMatchChallenges,matches);assert.equal(result.numberOfSingleChallenges,total-matches);assert.equal(result.challengeFormatsAvailable,true);
  assert.equal(result.numberOfLossChallenges,2);assert.equal(result.numberOfDrawChallenges,1);assert.equal(args.length,1);assert.equal(args[0],123);
  assert(query.includes('"ownerId"=$1 OR "recipientId"=$1'));assert(query.includes("state->>'done'='true'"));assert(query.includes("state->>'mode'='match'"));assert(!/\blimit\b/i.test(query));
 }
 console.log('CHALLENGE_FORMAT_STATISTICS_PASSED: all-time totals, empty account, both participant roles, format split, unchanged losses/draws');
})().catch(error=>{console.error(error);process.exitCode=1;});
