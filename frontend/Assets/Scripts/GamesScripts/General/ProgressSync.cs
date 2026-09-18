using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models;
using UnityEngine;

public class ProgressSync : MonoBehaviour
{
    private static ProgressSync instance;
    private GameAPIController api;
    private bool uploading;
    private readonly Dictionary<string, List<Action<bool>>> readers = new Dictionary<string, List<Action<bool>>>();
    private readonly Dictionary<string, float> refreshed = new Dictionary<string, float>();
    public static ProgressSync Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("ProgressSync").AddComponent<ProgressSync>();
                DontDestroyOnLoad(instance.gameObject);
                instance.api = instance.gameObject.AddComponent<GameAPIController>();
            }
            LevelProgress.EnsureUser();
            return instance;
        }
    }

    public T GetApi<T>() where T : Component
    {
        var component = GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }

    public void GetLevels(string game, Action<bool> ready)
    {
        int owner = LevelProgress.Owner;
        string key = owner + ":" + game;
        if (LevelProgress.Get(game) != null)
        {
            if (ready != null) ready(true);
            ready = null;
            float time;
            if (refreshed.TryGetValue(key, out time) && Time.realtimeSinceStartup - time < 60) { RetryPending(); return; }
        }
        List<Action<bool>> waiting;
        if (readers.TryGetValue(key, out waiting)) { if (ready != null) waiting.Add(ready); return; }
        waiting = new List<Action<bool>>(); if (ready != null) waiting.Add(ready);
        readers[key] = waiting;
        api.GetLevels(game, result =>
        {
            readers.Remove(key);
            if (LevelProgress.Owner != owner) return;
            LevelProgress.Merge(game, result.levels); refreshed[key] = Time.realtimeSinceStartup;
            foreach (var callback in waiting) callback(true);
            RetryPending();
        }, error =>
        {
            readers.Remove(key);
            if (LevelProgress.Owner != owner) return;
            foreach (var callback in waiting) callback(false);
        });
    }

    public void Complete(string game, int level, int stars, int score)
    {
        LevelProgress.Record(game, level, stars, score);
        RetryPending();
    }

    public void RetryPending()
    {
        if (uploading || LevelProgress.Owner <= 0) return;
        var entry = LevelProgress.Pending.FirstOrDefault();
        if (entry == null) return;
        uploading = true;
        int owner = LevelProgress.Owner;
        Action<AlertMessageContainer> failed = error =>
        {
            uploading = false;
            Debug.LogWarning("Level progress is saved on this device and will retry syncing.");
            if (LevelProgress.Owner != owner) RetryPending();
        };
        Action<User> saved = user =>
        {
            uploading = false;
            if (LevelProgress.Owner != owner) { RetryPending(); return; }
            if (user != null && user.ID == owner) UserConstants.s_user = user;
            LevelProgress.Confirm(entry);
            RetryPending();
        };
        Action<int> upload = id =>
        {
            if (LevelProgress.Owner != owner) { uploading = false; RetryPending(); return; }
            if (id > 0) api.UpdateGameLevel(id, entry.stars, entry.score, saved, failed);
            else api.CreateGameLevel(entry.stars, entry.score, entry.level, entry.game, saved, failed);
        };
        {
            // A previous POST might have succeeded even if its response was lost. Find its id before retrying.
            api.GetLevels(entry.game, result =>
            {
                if (LevelProgress.Owner != owner) { uploading = false; RetryPending(); return; }
                LevelProgress.Merge(entry.game, result.levels);
                var existing = result.levels.FirstOrDefault(l => l.level == entry.level);
                if (existing != null && existing.score >= entry.score && existing.stars >= entry.stars) saved(null);
                else
                {
                    if (existing != null)
                    {
                        entry.score = Math.Max(entry.score, existing.score);
                        entry.stars = Math.Max(entry.stars, existing.stars);
                    }
                    upload(existing == null ? 0 : existing.id);
                }
            }, failed);
        }
    }

    private void OnApplicationPause(bool paused) { if (!paused) RetryPending(); }
}
