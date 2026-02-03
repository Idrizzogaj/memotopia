using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class SocialLogInRequestPayload
    {
        public string userId;
        public string accessToken;
        public string provider;
    }
}
