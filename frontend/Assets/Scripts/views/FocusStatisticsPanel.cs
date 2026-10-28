using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class FocusStatisticsPanel {
 static readonly Color Teal=TrainingHubLayout.FocusTone,Paper=StatisticsCards.Paper;
 static Text Label(Transform parent,string text,Vector2 min,Vector2 max,int size,Color color){return StatisticsCards.Label(parent,text,min,max,size,color);}
 static GameObject Block(Transform parent,string name,float height,Color color){return StatisticsCards.Block(parent,name,height,color);}
 public static void Apply(Transform after){
  if(!TrainingHubLayout.ShowFocus || after==null)return;var parent=after.parent;
  var root=StatisticsCards.Section(parent,"Focus statistics","FOCUS",Teal);root.transform.SetSiblingIndex(after.GetSiblingIndex()+1);
  var subtitle=Block(root.transform,"Scope",38,Color.clear);Label(subtitle.transform,"90-second records · On this device",Vector2.zero,Vector2.one,22,ChainGame.Ink);
  var data=FocusStats.Read(GameManager.AchievementUserId);var totals=Block(root.transform,"Totals",86,Color.white);string[] summary={data.games.Sum(g=>g.sessions)+"\nSessions",data.games.Sum(g=>g.finished)+"\nFinished",Mathf.FloorToInt(data.games.Sum(g=>g.seconds)/60)+" min\nPlayed"};for(int i=0;i<3;i++)Label(totals.transform,summary[i],new Vector2(i/3f,0),new Vector2((i+1)/3f,1),25,Teal);
  var gridObject=StatisticsCards.Grid(root.transform,"Game records");
  for(int kind=0;kind<4;kind++){
   var record=data.games[kind];var card=StatisticsCards.Card(gridObject,FocusRules.Names[kind],Teal,kind);
   Label(card.transform,record.hasBest?record.bestScore.ToString():"—",new Vector2(.05f,.39f),new Vector2(.53f,.77f),65,Color.white);Label(card.transform,"BEST SCORE",new Vector2(.05f,.29f),new Vector2(.53f,.41f),19,Color.white);
   Label(card.transform,record.highest>0?"L"+record.highest:"—",new Vector2(.58f,.42f),new Vector2(.95f,.71f),42,Color.white);Label(card.transform,"HIGHEST",new Vector2(.54f,.29f),new Vector2(.98f,.41f),19,Color.white);
   Label(card.transform,record.hasBest?"Start L"+record.bestStart+" · "+FocusStats.Accuracy(record.bestCorrect,record.bestErrors):"Finish 90s to set a record",new Vector2(.04f,.15f),new Vector2(.96f,.28f),19,Color.white);
   Label(card.transform,record.sessions+(record.sessions==1?" session · ":" sessions · ")+FocusStats.Accuracy(record.correct,record.errors)+" accuracy",new Vector2(.04f,.02f),new Vector2(.96f,.15f),18,Color.white);
  }
  var practice=Block(root.transform,"Practice records",0,Paper);float practiceHeight=PracticeStatisticsPanel.Draw(practice.transform,"Focus");practice.GetComponent<LayoutElement>().preferredHeight=practiceHeight;
  var history=Block(root.transform,"Recent",54,Teal);Label(history.transform,"RECENT",Vector2.zero,Vector2.one,27,Color.white);float historyHeight=0;
  foreach(var run in data.recent.Take(4)){var row=Block(root.transform,"Session",64,Color.white);string progress=run.ended?run.score+" pts · L"+run.start+"→"+run.highest:"Interrupted · result unavailable";string result=run.ended?(run.completed?"90s finished":"Ended early · "+Mathf.RoundToInt(run.seconds)+"s"):"";Label(row.transform,FocusRules.Names[run.kind]+" · "+progress+"\n"+result,Vector2.zero,Vector2.one,22,ChainGame.Ink);historyHeight+=78;}
  if(data.recent.Count==0){var empty=Block(root.transform,"No history",70,Color.white);Label(empty.transform,"Play Focus to create your first record",Vector2.zero,Vector2.one,23,ChainGame.Ink);historyHeight=84;}
  var fit=parent.GetComponent<ContentSizeFitter>();if(fit!=null)fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;StatisticsCards.Reflow(parent);
 }
}
