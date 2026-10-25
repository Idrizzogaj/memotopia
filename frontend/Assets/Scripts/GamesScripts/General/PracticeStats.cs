using System;using System.Collections.Generic;using System.Linq;using UnityEngine;

public static class PracticeStats {
 [Serializable] public sealed class Record {public string game;public int level,start,sessions;public float seconds;public bool hasBest;}
 [Serializable] public sealed class Data {public List<Record> records=new List<Record>();}
 static string Key(int owner){return "practice.records.v1."+owner;}
 public static Data Read(int owner){try{var d=JsonUtility.FromJson<Data>(PlayerPrefs.GetString(Key(owner),"{}"));if(d!=null&&d.records!=null)return d;}catch(Exception){}return new Data();}
 public static void Save(int owner,string game,int start,int level,float seconds){if(owner<=0||level<=0||float.IsNaN(seconds)||float.IsInfinity(seconds))return;var data=Read(owner);var r=data.records.FirstOrDefault(x=>x.game==game);if(r==null){r=new Record{game=game};data.records.Add(r);}r.sessions++;seconds=Mathf.Max(0,seconds);if(!r.hasBest||level>r.level||level==r.level&&seconds<r.seconds){r.hasBest=true;r.level=level;r.seconds=seconds;r.start=start;}PlayerPrefs.SetString(Key(owner),JsonUtility.ToJson(data));PlayerPrefs.Save();}
 public static string Time(float seconds){int s=Mathf.Max(0,Mathf.FloorToInt(seconds));return s>=3600?(s/3600)+":"+((s/60)%60).ToString("00")+":"+(s%60).ToString("00"):(s/60)+":"+(s%60).ToString("00");}
}
