using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class ChallengelRequestPayload
    {
        public int level;
        public string gameName;
        public int challengerId;
    }
}