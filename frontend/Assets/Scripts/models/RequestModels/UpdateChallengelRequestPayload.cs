using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class UpdateChallengelRequestPayload
    {
        public float score;
        public int challengeId;
        public string status;
        public int progress = -1;
    }
}
