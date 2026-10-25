using System;

public static class FocusAchievements {
 public static readonly string[] First={"focus-pairs-first","focus-boxes-first","focus-flash-first","focus-chain-first"};
 public static readonly string[] Highest={"focus-pairs-professor","focus-boxes-max","focus-flash-vision","focus-chain-max"};
 public static string Reached(int kind,int started,int previous,int current){
  if(kind<0||kind>3)throw new ArgumentOutOfRangeException("kind");
  if(current!=FocusRules.Maximum(kind) || (kind==0 && started!=1))return null;
  // Called only after a completed round: selecting a high starting level earns nothing.
  return Highest[kind];
 }
}
