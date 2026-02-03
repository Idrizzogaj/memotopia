using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script.Models.ResponseModels
{
    [Serializable]
    public class SignUpResponsePayload
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
