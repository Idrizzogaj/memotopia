using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class UpdateGameLevelRequestPayload
    {
        public int stars;
        public int score;
    }
}