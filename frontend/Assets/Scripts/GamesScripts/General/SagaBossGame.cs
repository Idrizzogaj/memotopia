using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// One active-play clock, paused between games. Child wins never write normal progress.
public sealed class SagaBossGame : MonoBehaviour
{
    GameObject overlay,hud,intro,result,intermission;
    Coroutine intermissionMotion;
    readonly System.Random feedbackRandom=new System.Random();
    int lastPraise=-1;
    static readonly string[] Praise={"Great work!","Memorable!","Focus complete!","Brilliant!","Well done!"};
    ChainGame chain;
    SaysGame says;
    FocusGame focus;
    bool focusMode;
    SagaGame[] availableGames;
    string Title {get{return focusMode?"FOCUS BOSS":"BOSS";}}
    Action closed,won;
    int owner,level,index,hearts;
    float deadline,totalSeconds,pausedSeconds;
    bool running,switching,betweenRounds;
    SagaGame[] order;
    readonly Dictionary<SagaGame,Image> fills=new Dictionary<SagaGame,Image>();
    Text clock;
    public bool IsOpen {get{return overlay!=null;}}
    public bool WaitingForNext {get{return running && betweenRounds;}}
    public float Remaining {get{return running?(betweenRounds?pausedSeconds:Mathf.Max(0,deadline-Time.realtimeSinceStartup)):0;}}
    public void Open(int value,Action closed,Action won,bool focusMode=false){
        if(IsOpen || SprintRuntime.Active)return;totalSeconds=new SagaLevel(focusMode?SagaGame.FocusBoss:SagaGame.Boss,value).Seconds;this.focusMode=focusMode;availableGames=focusMode?SagaRoute.FocusGames:SagaRoute.Games;level=value;owner=GameManager.AchievementUserId;this.closed=closed;this.won=won;
        var canvas=GetComponentInParent<Canvas>().rootCanvas;var safe=canvas.GetComponent<MobileSafeArea>();Transform parent=safe!=null && safe.Content!=null?safe.Content:canvas.transform;
        overlay=ChainGame.Panel(parent,"Saga boss",Vector2.zero,Vector2.one,new Color32(242,239,250,255));
        if(chain==null){var host=new GameObject("Boss game adapters",typeof(RectTransform));host.transform.SetParent(transform,false);chain=host.AddComponent<ChainGame>();says=host.AddComponent<SaysGame>();focus=host.AddComponent<FocusGame>();}
        hud=ChainGame.Panel(overlay.transform,"Six boss games",new Vector2(0,.88f),Vector2.one,SagaRoute.ColorFor(SagaGame.Boss));
        var hudCanvas=hud.AddComponent<Canvas>();hudCanvas.overrideSorting=true;hudCanvas.sortingOrder=150;hud.AddComponent<GraphicRaycaster>();
        clock=ChainGame.Label(hud.transform,Title+" "+level+" · "+Mathf.CeilToInt(totalSeconds)+" s",new Vector2(.22f,.65f),new Vector2(.98f,1),28,Color.white);
        ChainGame.Button(hud.transform,"<",new Vector2(.02f,.68f),new Vector2(.16f,.98f),Close);
        for(int i=0;i<availableGames.Length;i++){
            var game=availableGames[i];float width=.96f/availableGames.Length,left=.02f+i*width;
            var tile=ChainGame.Panel(hud.transform,game+" progress",new Vector2(left,.03f),new Vector2(left+width-.01f,.65f),new Color(1,1,1,.12f));
            var fill=ChainGame.Panel(tile.transform,"Completed",Vector2.zero,Vector2.one,SagaRoute.ColorFor(game)).GetComponent<Image>();fill.sprite=ContentSelectorBar.Rounded();fill.type=Image.Type.Filled;fill.fillMethod=Image.FillMethod.Horizontal;fill.fillAmount=0;fill.raycastTarget=false;fills[game]=fill;
            int symbol=focusMode?Array.IndexOf(SagaRoute.FocusGames,game):game==SagaGame.Pairs?0:game==SagaGame.Boxes?1:game==SagaGame.Flash?2:game==SagaGame.Chain?3:-1;
            var art=(RectTransform)ChainGame.Panel(tile.transform,"Native game symbol",new Vector2(.28f,.35f),new Vector2(.72f,.98f),Color.clear).transform;art.GetComponent<Image>().raycastTarget=false;
            if(symbol>=0)TrainingGameSymbol.Draw(art,symbol,Color.white,false);
            else if(game==SagaGame.Says){var image=art.GetComponent<Image>();image.color=Color.white;image.sprite=Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01");image.preserveAspect=true;}
            else ChainGame.Label(art,"123",Vector2.zero,Vector2.one,22,Color.white);
            var label=ChainGame.Label(tile.transform,game.ToString().Replace("Focus","").ToUpperInvariant(),Vector2.zero,new Vector2(1,.35f),17,Color.white);label.resizeTextForBestFit=true;label.resizeTextMaxSize=17;label.resizeTextMinSize=10;
        }
        intro=ChainGame.Panel(overlay.transform,"Boss start",new Vector2(.08f,.28f),new Vector2(.92f,.70f),Color.white);
        ChainGame.Label(intro.transform,Title+" "+level+"\n"+availableGames.Length+" games · "+Mathf.CeilToInt(totalSeconds)+" seconds total",new Vector2(.06f,.48f),new Vector2(.94f,.92f),36,ChainGame.Purple);
        ChainGame.Button(intro.transform,"START",new Vector2(.16f,.10f),new Vector2(.84f,.34f),StartBoss);
    }
    public void StartBoss(){
        StopAllCoroutines();intermissionMotion=null;ClearIntermission();betweenRounds=false;CloseChildren();if(intro!=null)CardFaces.DestroyGenerated(intro);intro=null;if(result!=null)CardFaces.DestroyGenerated(result);result=null;
        index=0;hearts=focusMode?5:SagaLevel.Hearts;foreach(var image in fills.Values)image.fillAmount=0;
        order=(SagaGame[])availableGames.Clone();var random=new System.Random();for(int i=order.Length-1;i>0;i--){int j=random.Next(i+1);var game=order[i];order[i]=order[j];order[j]=game;}
        pausedSeconds=totalSeconds;betweenRounds=true;running=true;OpenRound();
    }
    void OpenRound(){
        if(!running)return;CloseChildren();
        if(Remaining<=0){Finish(false);return;}
        var context=new SagaRoundContext(()=>Remaining,RoundFinished,hearts);
        if(focusMode)focus.OpenSaga(new SagaFocusLevel(Array.IndexOf(SagaRoute.FocusGames,order[index]),level),ChildClosed,null,context);
        else if(order[index]==SagaGame.Says)says.OpenSaga(level,ChildClosed,null,null,context);
        else chain.OpenSaga(order[index],level,ChildClosed,null,null,context);
        // Native board/resource setup is part of the paused transition, not play.
        deadline=Time.realtimeSinceStartup+pausedSeconds;betweenRounds=false;
    }
    void RoundFinished(bool success,int remainingHearts){
        if(!running || betweenRounds || owner!=GameManager.AchievementUserId)return;
        if(!success || remainingHearts<=0 || Remaining<=0){Finish(false);return;}
        hearts=Mathf.Min(hearts,remainingHearts);fills[order[index]].fillAmount=1;index++;
        if(index==order.Length){Finish(true);return;}
        pausedSeconds=Remaining;betweenRounds=true;ShowIntermission();
    }
    static string GameName(SagaGame game){return game.ToString().Replace("Focus","").ToUpperInvariant();}
    void ShowIntermission(){
        intermission=ChainGame.Panel(overlay.transform,"Boss intermission",Vector2.zero,new Vector2(1,.88f),new Color32(242,239,250,255));
        var layer=intermission.AddComponent<Canvas>();layer.overrideSorting=true;layer.sortingOrder=151;intermission.AddComponent<GraphicRaycaster>();
        var card=ChainGame.Panel(intermission.transform,"Challenge complete card",new Vector2(.07f,.16f),new Vector2(.93f,.88f),Color.white);
        card.GetComponent<Image>().sprite=ContentSelectorBar.Rounded();card.GetComponent<Image>().type=Image.Type.Sliced;
        Color theme=SagaRoute.ColorFor(focusMode?SagaGame.FocusBoss:SagaGame.Boss);
        ChainGame.Label(card.transform,index+" / "+order.Length+" complete",new Vector2(.08f,.88f),new Vector2(.92f,.96f),24,theme);
        int praise=feedbackRandom.Next(lastPraise<0?Praise.Length:Praise.Length-1);if(lastPraise>=0 && praise>=lastPraise)praise++;lastPraise=praise;
        ChainGame.Label(card.transform,Praise[praise],new Vector2(.06f,.75f),new Vector2(.94f,.88f),48,ChainGame.Purple).fontStyle=FontStyle.Bold;
        var neuron=ChainGame.Panel(card.transform,"Existing Saga neuron",new Vector2(.28f,.48f),new Vector2(.72f,.75f),Color.white).GetComponent<Image>();
        neuron.sprite=Resources.Load<Sprite>("Saga/Neuron");neuron.preserveAspect=true;neuron.raycastTarget=false;
        ChainGame.Label(card.transform,GameName(order[index-1])+" complete!",new Vector2(.08f,.42f),new Vector2(.92f,.50f),26,theme);
        ChainGame.Label(card.transform,"Next challenge",new Vector2(.08f,.32f),new Vector2(.92f,.41f),26,ChainGame.Purple);
        ChainGame.Label(card.transform,GameName(order[index]),new Vector2(.08f,.22f),new Vector2(.92f,.33f),38,theme).fontStyle=FontStyle.Bold;
        var go=ChainGame.Button(card.transform,"GO!",new Vector2(.14f,.055f),new Vector2(.86f,.20f),ContinueBoss);
        go.image.color=theme;go.GetComponentInChildren<Text>().color=Color.white;
        clock.text=Title+" "+level+" · PAUSED · "+Mathf.CeilToInt(pausedSeconds)+" s";
        var group=card.AddComponent<CanvasGroup>();
        if(Application.isPlaying && PlayerPrefs.GetInt("saga.motion.v1",1)!=0){group.alpha=0;card.transform.localScale=Vector3.one*.96f;}
        if(Application.isPlaying)intermissionMotion=StartCoroutine(RevealIntermission((RectTransform)card.transform));else CloseChildren();
    }
    IEnumerator RevealIntermission(RectTransform card){
        // Let the native answer callback return before closing its board.
        yield return null;
        if(!WaitingForNext || intermission==null)yield break;
        CloseChildren();
        var group=card.GetComponent<CanvasGroup>();
        if(PlayerPrefs.GetInt("saga.motion.v1",1)!=0){
            for(float elapsed=0;elapsed<.28f;elapsed+=Time.unscaledDeltaTime){
                if(PlayerPrefs.GetInt("saga.motion.v1",1)==0)break;
                float t=Mathf.SmoothStep(0,1,elapsed/.28f);group.alpha=t;card.localScale=Vector3.one*Mathf.Lerp(.96f,1,t);yield return null;
            }
        }
        group.alpha=1;card.localScale=Vector3.one;intermissionMotion=null;
    }
    void ContinueBoss(){
        if(!WaitingForNext || owner!=GameManager.AchievementUserId)return;
        ClearIntermission();OpenRound();
    }
    void ClearIntermission(){
        if(intermissionMotion!=null)StopCoroutine(intermissionMotion);intermissionMotion=null;
        if(intermission!=null)CardFaces.DestroyGenerated(intermission);intermission=null;
    }
    void Finish(bool success){
        if(!running)return;running=false;betweenRounds=false;StopAllCoroutines();intermissionMotion=null;ClearIntermission();CloseChildren();
        int stars=focusMode?SagaProgress.StarsFromFocusHearts(hearts):hearts;
        if(success)SagaProgress.Complete(focusMode?SagaGame.FocusBoss:SagaGame.Boss,owner,level,stars);
        result=SagaResultView.Show(overlay.transform,SagaRoute.Stop(focusMode?SagaGame.FocusBoss:SagaGame.Boss,level),success,success?stars:0,order.Length.ToString(),StartBoss,Close);
        if(success){
            var message=result.GetComponentsInChildren<Text>(true).FirstOrDefault(t=>t.text=="AWESOME");
            if(message!=null)message.text="You won!";
            if(won!=null)won();
        }
    }
    void ChildClosed(){if(!switching)Close();}
    void CloseChildren(){switching=true;if(chain!=null)chain.Close();if(says!=null)says.Close();if(focus!=null)focus.Close();switching=false;}
    public void Close(){StopAllCoroutines();intermissionMotion=null;ClearIntermission();running=betweenRounds=false;CloseChildren();if(overlay!=null)CardFaces.DestroyGenerated(overlay);overlay=null;fills.Clear();var callback=closed;closed=won=null;if(callback!=null)callback();}
    void Update(){if(!IsOpen)return;if(owner!=GameManager.AchievementUserId){Close();return;}if(running){clock.text=Title+" "+level+" · "+(betweenRounds?"PAUSED · ":"")+Mathf.CeilToInt(Remaining)+" s";if(Remaining<=0)Finish(false);}}
    void OnDisable(){if(IsOpen)Close();}
}
