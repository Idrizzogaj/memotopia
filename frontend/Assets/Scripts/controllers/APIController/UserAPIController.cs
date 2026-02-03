using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.ResponseModels;
using Assets.Script.Models.RequestModels;
using UnityEngine;
using System.Text;

namespace Assets.Script.Controllers
{
    public class UserAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;

        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        #region User
        public void GetUser(Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetUserRoute, null, headers);

            StartCoroutine(GetUserRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetUserRequest(WWW request, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
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

        public void GetRandomUser(Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetRandomUserRoute, null, headers);

            StartCoroutine(GetRandomUserRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetRandomUserRequest(WWW request, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<User>(request.text);

                UserConstants.s_challengeUser = responseObject;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }
        #endregion

        #region Update Account
        public void UpdateAccount(string newUsername, string avatar, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new UpdateAccountRequestPayload();
            requestObject.newUsername = newUsername;
            requestObject.avatar = avatar;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.UpdateAccount, formData, headers);

            StartCoroutine(ReturnUpdateAccountRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnUpdateAccountRequest(WWW request, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<User>(request.text);
                UserConstants.s_user = responseObject;
                print(UserConstants.s_user.Avatar);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }
        #endregion

        #region Update Payment Status
        public void UpdatePaymentStatus(string paymentStatus, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new UpdatePaymentStatusRequestPayload();
            requestObject.paymentStatus = paymentStatus;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.UpdatePaymentStatus, formData, headers);

            StartCoroutine(ReturnUpdatePaymentStatusRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnUpdatePaymentStatusRequest(WWW request, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
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
