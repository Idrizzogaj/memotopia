using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Six games, without replacement, one clock. Child wins never write normal progress.
public sealed class SagaBossGame : MonoBehaviour
{
    GameObject overlay,hud,intro,result;
    ChainGame chain;
    SaysGame says;
    FocusGame focus;
    bool focusMode;
    SagaGame[] availableGames;
    string Title {get{return focusMode?"FOCUS BOSS":"BOSS";}}
    Action closed,won;
    int owner,level,index,hearts;
    float deadline,totalSeconds;
    bool running,switching;
    SagaGame[] order;
    readonly Dictionary<SagaGame,Image> fills=new Dictionary<SagaGame,Image>();
    Text clock;
    public bool IsOpen {get{return overlay!=null;}}
    public float Remaining {get{return running?Mathf.Max(0,deadline-Time.realtimeSinceStartup):0;}}
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
        StopAllCoroutines();CloseChildren();if(intro!=null)CardFaces.DestroyGenerated(intro);intro=null;if(result!=null)CardFaces.DestroyGenerated(result);result=null;
        index=0;hearts=focusMode?5:SagaLevel.Hearts;foreach(var image in fills.Values)image.fillAmount=0;
        order=(SagaGame[])availableGames.Clone();var random=new System.Random();for(int i=order.Length-1;i>0;i--){int j=random.Next(i+1);var game=order[i];order[i]=order[j];order[j]=game;}
        deadline=Time.realtimeSinceStartup+totalSeconds;running=true;OpenRound();
    }
    void OpenRound(){
        if(!running)return;CloseChildren();
        if(Remaining<=0){Finish(false);return;}
        var context=new SagaRoundContext(deadline,RoundFinished,null,hearts);
        if(focusMode)focus.OpenSaga(new SagaFocusLevel(Array.IndexOf(SagaRoute.FocusGames,order[index]),level),ChildClosed,null,context);
        else if(order[index]==SagaGame.Says)says.OpenSaga(level,ChildClosed,null,null,context);
        else chain.OpenSaga(order[index],level,ChildClosed,null,null,context);
    }
    void RoundFinished(bool success,int remainingHearts){
        if(!running || owner!=GameManager.AchievementUserId)return;
        if(!success || remainingHearts<=0 || Remaining<=0){Finish(false);return;}
        hearts=Mathf.Min(hearts,remainingHearts);fills[order[index]].fillAmount=1;index++;
        if(index==order.Length){Finish(true);return;}
        StartCoroutine(NextRound());
    }
    IEnumerator NextRound(){yield return new WaitForSecondsRealtime(.25f);OpenRound();}
    void Finish(bool success){
        if(!running)return;running=false;StopAllCoroutines();CloseChildren();
        int stars=focusMode?SagaProgress.StarsFromFocusHearts(hearts):hearts;
        if(success)SagaProgress.Complete(focusMode?SagaGame.FocusBoss:SagaGame.Boss,owner,level,stars);
        result=SagaResultView.Show(overlay.transform,SagaRoute.Stop(focusMode?SagaGame.FocusBoss:SagaGame.Boss,level),success,success?stars:0,order.Length.ToString(),StartBoss,Close);
        if(success && won!=null)won();
    }
    void ChildClosed(){if(!switching)Close();}
    void CloseChildren(){switching=true;if(chain!=null)chain.Close();if(says!=null)says.Close();if(focus!=null)focus.Close();switching=false;}
    public void Close(){StopAllCoroutines();running=false;CloseChildren();if(overlay!=null)CardFaces.DestroyGenerated(overlay);overlay=null;fills.Clear();var callback=closed;closed=won=null;if(callback!=null)callback();}
    void Update(){if(!IsOpen)return;if(owner!=GameManager.AchievementUserId){Close();return;}if(running){clock.text=Title+" "+level+" · "+Mathf.CeilToInt(Remaining)+" s";if(Remaining<=0)Finish(false);}}
    void OnDisable(){if(IsOpen)Close();}
}
