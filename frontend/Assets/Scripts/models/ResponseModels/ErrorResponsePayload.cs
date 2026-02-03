using System;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class ErrorResponsePayload
    {
        public string httpStatus;
        public string detail;

        public string Code
        {
            get { return httpStatus; }
            set { httpStatus = value; }
        }
        public string Message
        {
            get { return detail; }
            set { detail = value; }
        }
    }
}

