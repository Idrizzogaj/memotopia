using System;

namespace Assets.Script.Models.RequestModels
{
    [Serializable]
    public class DeleteAccountRequestPayload
    {
        public string password;
        public string confirm;
    }
}
