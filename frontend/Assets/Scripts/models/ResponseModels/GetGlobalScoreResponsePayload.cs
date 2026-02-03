using System;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class GetGlobalScoreResponsePayload
    {
        public UserStatistics[] statistics;
    }
}
