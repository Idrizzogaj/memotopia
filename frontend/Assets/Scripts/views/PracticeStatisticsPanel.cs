using System.Linq;using UnityEngine;using UnityEngine.UI;
public static class PracticeStatisticsPanel {
 public static float Draw(Transform host){
  var layout=host.gameObject.AddComponent<VerticalLayoutGroup>();layout.spacing=10;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false;var records=PracticeStats.Read(GameManager.AchievementUserId).records;float height=0;int rows=0;
  System.Func<string,float,Color,GameObject> block=(name,h,color)=>{var go=ChainGame.Panel(host,name,Vector2.zero,Vector2.one,color);go.AddComponent<LayoutElement>().preferredHeight=h;height+=h;rows++;return go;};
  System.Action<Transform,string,Vector2,Vector2,int,Color> text=(parent,value,min,max,size,color)=>{var t=ChainGame.Label(parent,value,min,max,size,color);t.fontStyle=FontStyle.Bold;};
  var title=block("Practice header",58,TrainingHubLayout.FocusTone);text(title.transform,"PRACTICE",Vector2.zero,Vector2.one,32,Color.white);
  var note=block("Practice record rule",58,Color.white);text(note.transform,"Best level + time · On this device\nHigher level first; equal level uses shorter time",Vector2.zero,Vector2.one,21,ChainGame.Ink);
  foreach(string group in new[]{"Focus","Memory"}){
   var header=block(group+" practice heading",46,group=="Focus"?TrainingHubLayout.FocusTone:ChainGame.Purple);text(header.transform,group.ToUpperInvariant(),new Vector2(.03f,0),new Vector2(.48f,1),24,Color.white);text(header.transform,"Level",new Vector2(.50f,0),new Vector2(.70f,1),23,Color.white);text(header.transform,"Time",new Vector2(.73f,0),new Vector2(.98f,1),23,Color.white);
   foreach(string game in group=="Focus"?FocusRules.Names:new[]{"Pairs","Boxes","Flash","Chain","Says"}){var r=records.FirstOrDefault(x=>x.game==group+"."+game);var row=block(group+" "+game+" practice record",60,Color.white);text(row.transform,game,new Vector2(.04f,0),new Vector2(.48f,1),25,ChainGame.Ink);text(row.transform,r!=null&&r.hasBest?r.level.ToString():"—",new Vector2(.50f,0),new Vector2(.70f,1),28,ChainGame.Ink);text(row.transform,r!=null&&r.hasBest?PracticeStats.Time(r.seconds):"—",new Vector2(.73f,0),new Vector2(.98f,1),26,ChainGame.Ink);}
  }
  return height+(rows-1)*10;
 }
}
