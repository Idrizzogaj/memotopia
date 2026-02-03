using System;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class GetChallengeHistoryResponsePayload
    {
        public History[] history;
    }
}
