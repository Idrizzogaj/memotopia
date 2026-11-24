using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEngine;

public static class SagaTestResetValidation
{
    const int Owner=1999999822, Other=1999999823;
    static void Check(bool okay,string message){if(!okay)throw new Exception("Saga test reset: "+message);}
    static string Key(SagaGame game,int owner,int level){return "saga."+game.ToString().ToLowerInvariant()+".stars.v1."+owner+"."+level;}
    static bool Reset(int owner,string build){return (bool)typeof(SagaProgress).GetMethod("ResetForTestBuild",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{owner,build});}
    static bool All(int owner,int stars){return Enum.GetValues(typeof(SagaGame)).Cast<SagaGame>().All(g=>Enumerable.Range(1,20).All(n=>SagaProgress.Stars(g,owner,n)==stars));}

    public static void Validate()
    {
        Check(Application.productName=="MemotopiaSagaValidation","Use the isolated validation project.");
        var previous=UserConstants.s_user;UserConstants.s_user=new User{id=Owner,username="Install reset validation"};
        string marker="saga.test.reset-build.v1."+Owner;
        string training="chain.completed.v1."+Owner, focus="focus.stats.device.v1."+Owner;
        string achievements="achievements.verified.v2."+Owner;
        string previousMotion=PlayerPrefs.GetString("saga.motion.v1","");int motion=PlayerPrefs.GetInt("saga.motion.v1",1);
        try{
            PlayerPrefs.DeleteKey(marker);
            foreach(SagaGame game in Enum.GetValues(typeof(SagaGame)))for(int n=1;n<=20;n++){
                PlayerPrefs.SetInt(Key(game,Owner,n),3);PlayerPrefs.SetInt(Key(game,Other,n),2);
            }
            PlayerPrefs.SetInt(training,31);PlayerPrefs.SetString(focus,"unchanged focus fixture");PlayerPrefs.SetString(achievements,"unchanged badge fixture");PlayerPrefs.SetInt("saga.motion.v1",0);
            SagaProgress.PrepareTestInstall(Owner);Check(All(Owner,3),"Normal builds must never auto-reset Saga.");
            Check(!Reset(Owner,"") && !Reset(0,"install-a") && !Reset(Other,"install-a") && All(Owner,3),"Reject empty build IDs and unowned accounts.");
            Check(Reset(Owner,"install-a") && All(Owner,0),"A new test installation resets all240ordinary/boss results.");
            Check(All(Other,2),"Another account must not be reset.");
            SagaProgress.Complete(SagaGame.Chain,Owner,1,3);SagaProgress.Complete(SagaGame.FocusBoss,Owner,20,2);
            Check(!Reset(Owner,"install-a") && SagaProgress.Stars(SagaGame.Chain,1)==3 && SagaProgress.Stars(SagaGame.FocusBoss,20)==2,"Reopening the same build preserves new progress.");
            Check(Reset(Owner,"install-b") && All(Owner,0),"The next test build resets again.");
            Check(PlayerPrefs.GetInt(training)==31 && PlayerPrefs.GetString(focus)=="unchanged focus fixture" && PlayerPrefs.GetString(achievements)=="unchanged badge fixture" && PlayerPrefs.GetInt("saga.motion.v1")==0,"Training, Focus stats, badges and motion settings must survive.");
            Debug.Log("SAGA_TEST_RESET_VALIDATION_PASSED:240results,once per build/account,repeat installation,normal-build guard,account isolation,Training/Focus/badges/settings preserved.");
        }finally{
            foreach(SagaGame game in Enum.GetValues(typeof(SagaGame)))for(int n=1;n<=20;n++){PlayerPrefs.DeleteKey(Key(game,Owner,n));PlayerPrefs.DeleteKey(Key(game,Other,n));}
            PlayerPrefs.DeleteKey(marker);PlayerPrefs.DeleteKey(training);PlayerPrefs.DeleteKey(focus);PlayerPrefs.DeleteKey(achievements);PlayerPrefs.SetInt("saga.motion.v1",motion);PlayerPrefs.Save();UserConstants.s_user=previous;
        }
    }
}
