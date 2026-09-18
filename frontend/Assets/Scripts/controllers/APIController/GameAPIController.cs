using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.RequestModels;
using Assets.Script.Models.ResponseModels;
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Script.Controllers
{
    public class GameAPIController : MonoBehaviour
    {
        private APIConstants routes = new APIConstants();
        public void OpenTermsUrl() { Application.OpenURL(routes.TermsUrl); }

        public void GetLevels(string game, Action<GameLevelResponsePayload> success, Action<AlertMessageContainer> failure)
        {
            StartCoroutine(Request(routes.GetLevelsRoute + "/" + game, null, true, success, failure));
        }
        public void CreateGameLevel(int stars, int score, int level, string gameName, Action<User> success, Action<AlertMessageContainer> failure)
        {
            string body = JsonUtility.ToJson(new GameLevelRequestPayload { stars = stars, score = score, level = level, gameName = gameName });
            StartCoroutine(Request(routes.LevelsRoute, body, false, success, failure));
        }
        public void UpdateGameLevel(int id, int stars, int score, Action<User> success, Action<AlertMessageContainer> failure)
        {
            string body = JsonUtility.ToJson(new UpdateGameLevelRequestPayload { stars = stars, score = score });
            StartCoroutine(Request(routes.LevelsRoute + "/" + id, body, false, success, failure));
        }
        private IEnumerator Request<T>(string url, string body, bool array, Action<T> success, Action<AlertMessageContainer> failure) where T : class
        {
            using (var request = new UnityWebRequest(url, body == null ? "GET" : "POST"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                if (body != null) request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", PlayerPrefs.GetString("token"));
                request.timeout = 12;
                yield return request.SendWebRequest();
                if (request.isNetworkError || request.isHttpError)
                {
                    failure(new AlertMessageContainer { ErrorMessage = "Could not connect. Please try again.", StatusCode = request.responseCode.ToString() });
                    yield break;
                }
                T result = null;
                try
                {
                    string json = request.downloadHandler.text;
                    result = JsonUtility.FromJson<T>(array ? "{\"levels\":" + json + "}" : json);
                    var levels = result as GameLevelResponsePayload;
                    if (array && (levels == null || levels.levels == null)) result = null;
                }
                catch (ArgumentException) { }
                if (result == null) failure(new AlertMessageContainer { ErrorMessage = "Could not read game progress. Please try again.", StatusCode = "invalid-response" });
                else success(result);
            }
        }
    }
}
