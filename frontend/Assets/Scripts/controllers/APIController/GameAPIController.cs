using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.RequestModels;
using Assets.Script.Models.ResponseModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Script.Controllers
{
    public class GameAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;

        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        #region TermsAndConditions
        public void OpenTermsUrl()
        {
            Application.OpenURL(_apiConstants.TermsUrl);
        }
        #endregion

        #region gameLevel
        public void GetLevels(string game, Action<GameLevelResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetLevelsRoute + "/"+ game, null, headers);

            StartCoroutine(GetLevelsRequest(request, game, onSuccess, onFailure));
        }

        private IEnumerator GetLevelsRequest(WWW request, string game, Action<GameLevelResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                string JSONToParse = "{\"levels\":" + request.text + "}";
                var responseObject = JsonUtility.FromJson<GameLevelResponsePayload>(JSONToParse);

                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void CreateGameLevel(int stars, int score, int level, string gameName, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new GameLevelRequestPayload();
            requestObject.stars = stars;
            requestObject.score = score;
            requestObject.level = level;
            requestObject.gameName = gameName;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.LevelsRoute, formData, headers);

            StartCoroutine(CreateGameLevelRequest(request, onSuccess, onFailure));
        }

        private IEnumerator CreateGameLevelRequest(WWW request, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<User>(request.text);
                UserConstants.s_user = responseObject;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void UpdateGameLevel(int id, int stars, int score, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new UpdateGameLevelRequestPayload();
            requestObject.stars = stars;
            requestObject.score = score;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.LevelsRoute + "/" + id, formData, headers);

            StartCoroutine(UpdateGameLevelRequest(request, onSuccess, onFailure));
        }

        private IEnumerator UpdateGameLevelRequest(WWW request, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<User>(request.text);
                UserConstants.s_user = responseObject;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        #endregion

        #region AlertMessage
        AlertMessageContainer ReturnAlertMessageType(string message, string code)
        {
            Debug.LogError($"Unhandled: [{message}]");
            return new AlertMessageContainer() { ErrorMessage = message, StatusCode = code };
        }
        #endregion
    }
}