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
    public class AuthAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;

        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        #region Singup
        public void PerformSignup(string username, string password, string email, Action<SignUpResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new SignUpRequestPayload();
            requestObject.username = username;
            requestObject.password = password;
            requestObject.email = email;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.SignUpRoute, formData, headers);

            StartCoroutine(ReturnSignupRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnSignupRequest(WWW request, Action<SignUpResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<SignUpResponsePayload>(request.text);
                UserConstants.s_user = responseObject.user;
                _dataController.Token = "Bearer " + responseObject.Token;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }
        #endregion

        #region Login
        public void PerformLogin(string username, string password, Action<LogInResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new LogInRequestPayload();
            requestObject.username = username;
            requestObject.password = password;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.LogInRoute, formData, headers);

            StartCoroutine(ReturnLoginRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnLoginRequest(WWW request, Action<LogInResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<LogInResponsePayload>(request.text);
                UserConstants.s_user = responseObject.user;
                _dataController.Token = "Bearer " + responseObject.Token;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }
        #endregion

        #region Social Login
        public void PerformSocialLogin(string userId, string accessToken, string provider, Action<LogInResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new SocialLogInRequestPayload();
            requestObject.userId = userId;
            requestObject.accessToken = accessToken;
            requestObject.provider = provider;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.SocialLogInRoute, formData, headers);

            StartCoroutine(ReturnSocialLoginRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnSocialLoginRequest(WWW request, Action<LogInResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<LogInResponsePayload>(request.text);
                UserConstants.s_user = responseObject.user;
                _dataController.Token = "Bearer " + responseObject.Token;
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
            Debug.LogError($"Unhandled: [{message}], code: [{code}]");
            return new AlertMessageContainer() { ErrorMessage = message, StatusCode = code };
        }
        #endregion

        #region ForgotPassword
        public void ForgotPassword(string email, Action<Boolean> onSuccess, Action<Boolean> onFailure)
        {
            var requestObject = new ForgotPasswordRequestPayload();
            requestObject.email = email;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.ForgotPassword, formData, headers);

            StartCoroutine(ForgotPasswordRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ForgotPasswordRequest(WWW request, Action<Boolean> onSuccess, Action<Boolean> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                print(request.text);
                onSuccess(true);
            }
            else
            {
                // var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                // onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
                onFailure(false);
            }
        }
        #endregion

    }
}
