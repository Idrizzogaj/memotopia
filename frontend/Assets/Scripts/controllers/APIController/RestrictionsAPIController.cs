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
    public class RestrictionsAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;

        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        public void updateRestrictions(string lastPlayedDate, Action<Restrictions> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new RestrictionsRequestPayload();
            requestObject.lastPlayedDate = lastPlayedDate;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.UpdateRestrictions, formData, headers);

            StartCoroutine(updateRestrictionsRequest(request, onSuccess, onFailure));
        }

        private IEnumerator updateRestrictionsRequest(WWW request, Action<Restrictions> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<Restrictions>(request.text);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetRestriction(Action<Restrictions> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            string currentDate = System.DateTime.Now.ToString("yyyy-MM-dd");

            WWW request = new WWW(_apiConstants.GetRestrictions + "/" + currentDate + "T00:00:59.076Z", null, headers);

            StartCoroutine(GetRestrictionRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetRestrictionRequest(WWW request, Action<Restrictions> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<Restrictions>(request.text);
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
            Debug.LogError($"Unhandled: [{message}]");
            return new AlertMessageContainer() { ErrorMessage = message, StatusCode = code };
        }
        #endregion
    }
}