using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class LogInRequestPayload
    {
        public string username;
        public string password;
    }
}
