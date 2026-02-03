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
    public class AchievementsAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;

        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        public void GetAchievements(Action<Achievements> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.Achievements, null, headers);

            StartCoroutine(GetAchievementsRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetAchievementsRequest(WWW request, Action<Achievements> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                string JSONToParse = "{\"achievements\":" + request.text + "}";
                var responseObject = JsonUtility.FromJson<Achievements>(JSONToParse);

                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void AddAchievements(string[] achievements, Action<String> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new Achievements();
            requestObject.achievements = achievements;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.Achievements, formData, headers);

            StartCoroutine(AddAchievementsRequest(request, onSuccess, onFailure));
        }

        private IEnumerator AddAchievementsRequest(WWW request, Action<String> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                onSuccess("Done");
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
            Debug.LogError($"Unhandled: [{message}]");
            return new AlertMessageContainer() { ErrorMessage = message, StatusCode = code };
        }
        #endregion
    }
}