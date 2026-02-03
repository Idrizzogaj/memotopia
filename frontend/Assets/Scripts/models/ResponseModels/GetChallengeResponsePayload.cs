using System;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class GetChallengeResponsePayload
    {
        public Challenge [] results;

        public int page;

        public int limit;

        public int total;
    }
}
