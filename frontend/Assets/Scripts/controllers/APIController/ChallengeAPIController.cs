using System;
using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.ResponseModels;
using Assets.Script.Models.RequestModels;

namespace Assets.Script.Controllers
{
    public class ChallengeAPIController : MonoBehaviour
    {
        APIConstants _apiConstants;
        DataController _dataController;
        private void Awake()
        {
            _apiConstants = new APIConstants();
            _dataController = new DataController();
        }

        public void CreateChallenge(int level, string gameName, int challengerId, Action<ChallengeResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new ChallengelRequestPayload();
            requestObject.level = level;
            requestObject.gameName = gameName;
            requestObject.challengerId = challengerId;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.CreateChallengeRoute, formData, headers);

            StartCoroutine(ReturnCreateChallengeRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnCreateChallengeRequest(WWW request, Action<ChallengeResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;
            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<ChallengeResponsePayload>(request.text);
                // UserConstants.s_user = responseObject.user;
                // _dataController.Token = "Bearer " + responseObject.Token;
                ChallengeConstants.s_challengeId = responseObject.challenge.id;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void UpdateChallenge(float score, string status, int challengeId, Action<ChallengeResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            var requestObject = new UpdateChallengelRequestPayload();
            requestObject.score = score;
            requestObject.status = status;
            requestObject.challengeId = challengeId;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);

            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            WWW request = new WWW(_apiConstants.UpdateChallengeRoute, formData, headers);

            StartCoroutine(ReturnUpdateChallengeRequest(request, onSuccess, onFailure));
        }

        private IEnumerator ReturnUpdateChallengeRequest(WWW request, Action<ChallengeResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;
            
            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<ChallengeResponsePayload>(request.text);
                UserConstants.s_user = responseObject.user;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetChallenges(Action<GetChallengeResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetChallengeRoute+"?status=PENDING", null, headers);

            StartCoroutine(GetChallengesRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetChallengesRequest(WWW request, Action<GetChallengeResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<GetChallengeResponsePayload>(request.text);
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetHistory(Action<GetChallengeHistoryResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetHistoryRoute, null, headers);

            StartCoroutine(GetHistoryRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetHistoryRequest(WWW request, Action<GetChallengeHistoryResponsePayload> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                print(request.text);
                var responseObject = JsonUtility.FromJson<GetChallengeHistoryResponsePayload>("{\"history\":" + request.text + "}");
                
                // UserConstants.s_randomUser = responseObject;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void Search(string search, Action<UserSerializable> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.Search+'/'+search, null, headers);

            StartCoroutine(SearchRequest(request, onSuccess, onFailure));
        }

        private IEnumerator SearchRequest(WWW request, Action<UserSerializable> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<UserSerializable>("{\"users\":" + request.text + "}");
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void GetFavorites(Action<UserSerializable> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;
            WWW request = new WWW(_apiConstants.GetFavorites, null, headers);

            StartCoroutine(GetFavoritesRequest(request, onSuccess, onFailure));
        }

        private IEnumerator GetFavoritesRequest(WWW request, Action<UserSerializable> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                var responseObject = JsonUtility.FromJson<UserSerializable>("{\"users\":" + request.text + "}");
                UserConstants.s_favoriteUser = responseObject;
                onSuccess(responseObject);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void AddFavorite(string id, Action<string> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;

            // Just to make it as post request
            var requestObject = new ChallengelRequestPayload();
            requestObject.level = 1;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            //
            WWW request = new WWW(_apiConstants.GetFavorites + "/" + id + "/add", formData, headers);

            StartCoroutine(AddFavoriteRequest(request, onSuccess, onFailure));
        }

        private IEnumerator AddFavoriteRequest(WWW request, Action<string> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                onSuccess(request.text);
            }
            else
            {
                var responseObject = JsonUtility.FromJson<ErrorResponsePayload>(request.text);
                onFailure(ReturnAlertMessageType(responseObject.Message, responseObject.Code));
            }
        }

        public void DeleteFavorite(string id, Action<string> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            WWWForm form = new WWWForm();
            Dictionary<string, string> headers = form.headers;
            headers["Content-Type"] = "application/json";
            headers["Authorization"] = _dataController.Token;

            // Just to make it as post request
            var requestObject = new ChallengelRequestPayload();
            requestObject.level = 1;
            string jsonRequestObject = JsonUtility.ToJson(requestObject);
            byte[] formData = Encoding.UTF8.GetBytes(jsonRequestObject);
            //
            WWW request = new WWW(_apiConstants.GetFavorites + "/" + id + "/remove", formData, headers);

            StartCoroutine(DeleteFavoriteRequest(request, onSuccess, onFailure));
        }

        private IEnumerator DeleteFavoriteRequest(WWW request, Action<string> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            yield return request;

            if (String.IsNullOrEmpty(request.error))
            {
                onSuccess(request.text);
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