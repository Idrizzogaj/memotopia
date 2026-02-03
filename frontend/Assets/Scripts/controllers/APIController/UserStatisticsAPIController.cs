using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.ResponseModels;
using Assets.Script.Models.RequestModels;
using System.Text;

namespace Assets.Script.Controllers
{
    public class UserStatisticsAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;
        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        public void GetUserStatisticsChallengeGames(Action<UserStatistics> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetUserStatistics, null, headers);

            StartCoroutine(GetUserStatisticsChallengeGamesRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetUserStatisticsChallengeGamesRequest(WWW request, Action<UserStatistics> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<UserStatistics>(request.text);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetGlobalScore(Action<GetGlobalScoreResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GlobalScore, null, headers);

            StartCoroutine(GetGlobalScoreRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetGlobalScoreRequest(WWW request, Action<GetGlobalScoreResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<GetGlobalScoreResponsePayload>("{\"statistics\":" + request.text + "}");
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetTimePlayed(Action<TimePlayed> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.TimePlayed, null, headers);

            StartCoroutine(GetTimePlayedRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetTimePlayedRequest(WWW request, Action<TimePlayed> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<TimePlayed>(request.text);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void IncreaseTimePlayed(float timePlayed, Action<TimePlayed> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new TimePlayed();
            requestObject.timePlayed = timePlayed;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.TimePlayed, formData, headers);

            StartCoroutine(IncreaseTimePlayedRequest(request, onSuccess, onFailure));
        }

        private IEnumerator IncreaseTimePlayedRequest(WWW request, Action<TimePlayed> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<TimePlayed>(request.text);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetLevelsAvg(Action<LevelsAvg> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetLevelsAvg, null, headers);

            StartCoroutine(GetLevelsAvgRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetLevelsAvgRequest(WWW request, Action<LevelsAvg> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<LevelsAvg>(request.text);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        #region AlertMessage
        AlertMessageContainer ReturnAlertMessageType(string message, string code)
        {
            Debug.LogError($"Unhandled: [{message}], code: [{code}]");
            return new AlertMessageContainer() { ErrorMessage = message, StatusCode = code };
        }
        #endregion
    }
}