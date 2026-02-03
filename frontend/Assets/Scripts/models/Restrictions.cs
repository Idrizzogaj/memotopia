using System;

namespace Assets.Script.Models
{
    [Serializable]
    public class Restrictions
    {
        public int id;
        public int numberOfChallengeGames;
        public string lastPlayedDate;
        public int user_id;
    }
}
