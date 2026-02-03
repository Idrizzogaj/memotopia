using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class SignUpRequestPayload
    {
        public string username;
        public string password;
        public string email;
    }
}