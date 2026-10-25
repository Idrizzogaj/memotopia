using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// User's handwritten Focus specification; shared by native UI and editor checks.
public static class FocusRules {
 public static readonly string[] Names={"Pairs","Boxes","Flash","Chain"};
 public static float FlashSeconds(int count){return count<=3?1:count<7?2:3;}
 public static int Maximum(int kind){return new[]{8,9,15,13}[kind];}
 public static int NumberCount(int kind,int level){
  if(kind==2)return level<=8?new[]{3,3,5,5,7,7,9,9}[level-1]:new[]{3,5,7,9,12,15,15}[Mathf.Min(level-9,6)];
  return level<=6?new[]{6,6,12,12,15,15}[level-1]:level<=9?new[]{6,12,15}[level-7]:new[]{6,12,15,15}[Mathf.Min(level-10,3)];
 }
 public static int NumberMode(int kind,int level){return level<=(kind==2?8:6)?(level%2==1?0:1):(kind==3 && level<=9?2:3);}
 public static int[] Sequence(int kind,int level){
  int n=NumberCount(kind,level),mode=NumberMode(kind,level);var result=new List<int>();
  if(mode==0)for(int i=1;i<=n;i++)result.Add(i);
  else if(mode==1)for(int i=n;i>0;i--)result.Add(1000+i);
  else if(mode==2){for(int i=1;i<=n;i++)result.Add(i);for(int i=n;i>0;i--)result.Add(1000+i);}
  else for(int i=1;i<=n;i++){result.Add(i);result.Add(1000+n-i+1);}return result.ToArray();
 }
 public static int[] Boxes(int level){return new[]{new[]{1,1,7},new[]{1,2,7},new[]{1,3,7},new[]{2,1,5},new[]{2,1,7},new[]{2,2,7},new[]{2,3,7},new[]{3,1,5},new[]{3,1,7}}[Mathf.Clamp(level-1,0,8)];}
 public sealed class Sum {public int A,B,Answer,Count,Limit;public string Operator;}
 public static Sum Question(int level,System.Random random){
  int limit=level<=2?20:level<=4?50:level<=7?100:200;
  string op=level==1||level==3?"+":level==2||level==4?"−":level==5?"×":level==6?"÷":new[]{"+","−","×","÷"}[random.Next(4)];
  int a,b,answer;
  if(op=="+"){a=random.Next(1,limit);b=random.Next(1,limit-a+1);answer=a+b;}
  else if(op=="−"){a=random.Next(2,limit+1);b=random.Next(1,a+1);answer=a-b;}
  else if(op=="×"){a=random.Next(2,Math.Min(12,limit/2)+1);b=random.Next(2,Math.Min(12,limit/a)+1);answer=a*b;}
  else{b=random.Next(2,13);answer=random.Next(1,limit/b+1);a=b*answer;}
  return new Sum{A=a,B=b,Answer=answer,Count=level<7?5:10,Limit=limit,Operator=op};
 }
 public sealed class Ball {
  public Vector2 Position,Velocity;public int Hits;public bool Armed;
  public int NextWall(){float x=Velocity.x>0?(.95f-Position.x)/Velocity.x:(.05f-Position.x)/Velocity.x,y=Velocity.y>0?(.95f-Position.y)/Velocity.y:(.05f-Position.y)/Velocity.y;return x<y?(Velocity.x>0?1:3):(Velocity.y>0?0:2);}
  public float Arrival(){return Mathf.Min(Velocity.x>0?(.95f-Position.x)/Velocity.x:(.05f-Position.x)/Velocity.x,Velocity.y>0?(.95f-Position.y)/Velocity.y:(.05f-Position.y)/Velocity.y);}
  // Returns the wall hit this tick, or -1; arrival coverage is consumed exactly once.
  public int Step(float seconds){int wall=NextWall();Position+=Velocity*seconds;bool hit=false;
   if(Position.x<.05f || Position.x>.95f){Position.x=Mathf.Clamp(Position.x,.05f,.95f);Velocity.x=-Velocity.x;hit=true;}
   if(Position.y<.05f || Position.y>.95f){Position.y=Mathf.Clamp(Position.y,.05f,.95f);Velocity.y=-Velocity.y;hit=true;}
   return hit?wall:-1;
  }
 }
}
