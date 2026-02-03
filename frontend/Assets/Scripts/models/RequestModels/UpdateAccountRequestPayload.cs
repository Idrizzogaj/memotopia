using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class UpdateAccountRequestPayload
    {
        public string newUsername;
        public string avatar;
    }
}