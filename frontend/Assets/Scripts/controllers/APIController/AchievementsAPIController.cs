using Assets.Script.Constants;
using Assets.Script.Models;
using Assets.Script.Models.RequestModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Script.Controllers
{
    public class AchievementsAPIController : MonoBehaviour
    {
        private readonly HashSet<string> sending = new HashSet<string>();

        public void GetAchievements(Action<Achievements> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            StartCoroutine(Request(null, text =>
            {
                Achievements response;
                try
                {
                    response = JsonUtility.FromJson<Achievements>("{\"achievements\":" + text + "}");
                    if (response == null || response.achievements == null) throw new ArgumentException();
                }
                catch (Exception)
                {
                    onFailure(new AlertMessageContainer { ErrorMessage = "Could not read achievements. Please try again.", StatusCode = "invalid-response" });
                    return;
                }
                onSuccess(response);
            }, onFailure));
        }

        public void AddAchievements(string[] achievements, Action<string> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            StartCoroutine(Request(JsonUtility.ToJson(new Achievements { achievements = achievements }), onSuccess, onFailure));
        }

        private IEnumerator Request(string body, Action<string> onSuccess, Action<AlertMessageContainer> onFailure)
        {
            using (var request = new UnityWebRequest(new APIConstants().Achievements, body == null ? "GET" : "POST"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                if (body != null) request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", PlayerPrefs.GetString("token"));
                request.timeout = 12;
                yield return request.SendWebRequest();
                if (!request.isNetworkError && !request.isHttpError) onSuccess(request.downloadHandler.text);
                else onFailure(new AlertMessageContainer { ErrorMessage = "Achievements could not sync. They will retry when you open Achievements.", StatusCode = request.responseCode.ToString() });
            }
        }

        public void FlushPending()
        {
            int owner = GameManager.AchievementUserId;
            foreach (string key in GameManager.PendingAchievements)
            {
                string requestKey = owner + ":" + key;
                if (!sending.Add(requestKey)) continue;
                AddAchievements(new[] { key }, result =>
                {
                    sending.Remove(requestKey);
                    if (GameManager.AchievementUserId == owner) GameManager.ConfirmAchievement(key);
                }, error =>
                {
                    sending.Remove(requestKey);
                    // The server also awards challenge achievements. Already owned is success.
                    if (error.StatusCode == "409" && GameManager.AchievementUserId == owner) GameManager.ConfirmAchievement(key);
                    else Debug.LogWarning(error.ErrorMessage);
                });
            }
        }
    }
}
