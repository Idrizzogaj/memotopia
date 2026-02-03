using System;
using Assets.Script.Models;

[Serializable]
public class UserStatistics
{
    public int id;
    public int level;
    public int xp;
    public int numberOfChallenges;
    public int numberOfWinChallenges;
    public int numberOfLossChallenges;
    public int numberOfDrawChallenges;
    public User user;
}
