using UnityEngine;

namespace Assets.Script.Controllers
{
    public class DataController : MonoBehaviour
    {
        private static readonly string DATA_TOKEN = "token";
        private static readonly string DATA_MENU_LAST_PAGE = "menuLastPage";


        #region Properties
        public string Token
        {
            get
            {
                return GetString(DATA_TOKEN);
            }
            set
            {
                SaveString(DATA_TOKEN, value);
            }
        }

        public string MenuLastPage
        {
            get
            {
                return GetString(DATA_MENU_LAST_PAGE);
            }
            set
            {
                SaveString(DATA_MENU_LAST_PAGE, value);
            }
        }
        #endregion

        #region Private Functions
        private void SaveString(string _data, string _value)
        {
            PlayerPrefs.SetString(_data, _value);
        }

        private string GetString(string _data)
        {
            return PlayerPrefs.GetString(_data);
        }
        #endregion
    }
}
