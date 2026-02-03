using System;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class ChallengeResponsePayload
    {
        public Challenge challenge;
        public Participant[] participant;
        public User user;
    }
}