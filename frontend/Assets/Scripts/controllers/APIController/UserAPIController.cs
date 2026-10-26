using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.ResponseModels;
using Assets.Script.Models.RequestModels;
using UnityEngine;
using UnityEngine.Networking;
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
            StartCoroutine(GetUserRequest(_dataController.Token, onSuccess, onFailure));
        }

        private IEnumerator GetUserRequest(string original, Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            using (var request = UnityWebRequest.Get(_apiConstants.GetUserRoute))
            {
                request.SetRequestHeader("Authorization", original);
                request.timeout = 12;
                yield return request.SendWebRequest();
                ApplyUserResponse(original, request.responseCode, request.isNetworkError || request.isHttpError,
                    request.downloadHandler.text, onSuccess, onFailure);
            }
        }

        private void ApplyUserResponse(string original, long status, bool failed, string body,
            Action<User> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            // A response from before logout/account switching must never restore that old account.
            if (string.IsNullOrEmpty(original) || PlayerPrefs.GetString("token") != original) return;
            if (!failed)
            {
                User user = null;
                try { user = JsonUtility.FromJson<User>(body); } catch (ArgumentException) { }
                if (user != null && user.ID > 0 && !string.IsNullOrEmpty(user.Username))
                {
                    UserConstants.s_user = user;
                    if (onSuccess != null) onSuccess(user);
                    return;
                }
            }
            // Only an actual HTTP 401 requires credentials again. Network/5xx/bad responses retain the token.
            if (onFailure != null) onFailure(new AlertMessageContainer {
                ErrorMessage = status == 401 ? "Please sign in again." : "Connection unavailable.",
                StatusCode = status == 401 ? "401" : "RETRY"
            });
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

        #region Delete Account
        // Permanently deletes the signed-in account on the server. Password is only checked for e-mail accounts.
        public void DeleteAccount(string password, Action onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new DeleteAccountRequestPayload();
            requestObject.password = password;
            requestObject.confirm = "DELETE";
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.DeleteAccount, formData, headers);

            StartCoroutine(ReturnDeleteAccountRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnDeleteAccountRequest(WWW request, Action onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                if (onSuccess != null) onSuccess();
                yield break;
            }

            ErrorResponsePayload errorPayload = null;
            if (!String.IsNullOrEmpty(request.text))
                errorPayload = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
            var message = (errorPayload != null && !String.IsNullOrEmpty(errorPayload.Message)) ? errorPayload.Message : request.error;
            var code = (errorPayload != null && !String.IsNullOrEmpty(errorPayload.Code)) ? errorPayload.Code : "HTTP_ERROR";
            if (onFailure != null) onFailure(new AlertMessageContainer() { ErrorMessage = message, StatusCode = code });
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
