using System;

namespace Assets.Script.Models
{
    [Serializable]
    public class GameLevel
    {
        public int id;
        public int level;
        public int stars;
        public int score;
        public int game_id;
        public User user;
    }
}
