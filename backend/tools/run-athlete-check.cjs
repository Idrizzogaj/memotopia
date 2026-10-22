const fs=require('fs'),vm=require('vm'),ts=require('typescript');
const exp=require('expect'),mock=require('jest-mock');const expect=exp.default||exp;
let setup=()=>{};const tests=[];const globals={expect,jest:{fn:mock.fn.bind(mock)},describe:(name,fn)=>{const prior=setup;setup=()=>{};fn();setup=prior;},beforeEach:fn=>{setup=fn;},it:(name,fn)=>tests.push({name,fn,setup})};
function load(path,imports={}){const context={exports:{},require:p=>{if(p in imports)return imports[p];throw Error('Unexpected dependency '+p)},...globals};vm.createContext(context);vm.runInContext(ts.transpileModule(fs.readFileSync(path,'utf8'),{compilerOptions:{module:ts.ModuleKind.CommonJS,target:ts.ScriptTarget.ES2020,experimentalDecorators:true}}).outputText,context);return context.exports;}
const nest={Injectable:()=>()=>{},BadRequestException:class extends Error{},InternalServerErrorException:class extends Error{}};
const rules=load('src/modules/level/level-rules.ts'),constants=load('src/modules/achievements/achievements.constants.ts');
const service=load('src/modules/achievements/achievements.service.ts',{'@nestjs/common':nest,'~common/db/entities/achievement.entity':{Achievement:class{}},'~common/db/entities/user-statistics.entity':{UserStatistics:class{}},'~common/db/entities/game-level.entity':{GameLevel:class{}},'~modules/level/level-rules':rules,'./achievements.constants':constants});
for(const path of ['src/modules/achievements/achievements.unit.ts','src/modules/achievements/athlete-reward.unit.ts'])load(path,{'@nestjs/common':nest,'./achievements.service':service});
(async()=>{for(const test of tests){await test.setup();await test.fn();console.log('PASS '+test.name);}console.log('ATHLETE_SERVER_CHECKS_PASSED '+tests.length);})().catch(e=>{console.error(e);process.exitCode=1});
