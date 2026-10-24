using System;
using Assets.Script.Models;

[Serializable]
public class UserStatistics
{
    public int id;
    public int level;
    public int xp;
    public int numberOfChallenges;
    public bool challengeFormatsAvailable;
    public int numberOfMatchChallenges;
    public int numberOfSingleChallenges;
    public int numberOfWinChallenges;
    public int numberOfLossChallenges;
    public int numberOfDrawChallenges;
    public User user;
}
