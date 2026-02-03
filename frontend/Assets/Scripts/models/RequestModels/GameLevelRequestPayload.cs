using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class GameLevelRequestPayload
    {
        public int stars;
        public int score;
        public int level;
        public string gameName;
    }
}