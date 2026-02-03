using System;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class LogInResponsePayload
    {
        public string accessToken;
        public User user;

        public string Token
        {
            get { return accessToken; }
            set { accessToken = value; }
        }
        public User User
        {
            get { return user; }
        }
    }
}
