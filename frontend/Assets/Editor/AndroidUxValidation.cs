using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AndroidUxValidation
{
    private const string RunKey = "Memotopia.AndroidUxValidation";
    private static LoadingManager manager;
    private static GameObject indicator;
    private static double started;
    private static int stage;

    static AndroidUxValidation()
    {
        EditorApplication.playModeStateChanged += OnState;
    }
    public static void Run()
    {
        AchievementValidation.Validate();
        FlowValidation.Validate();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SessionState.SetBool(RunKey, true);
        EditorApplication.isPlaying = true;
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Android UX validation: " + message);
    }
    private static void OnState(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(RunKey, false)) return;
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        var root = new GameObject("LoadingValidation");
        indicator = new GameObject("Indicator");
        indicator.transform.SetParent(root.transform);
        manager = root.AddComponent<LoadingManager>();
        stage = 0;
        started = EditorApplication.timeSinceStartup;
        manager.StartLoading();
        TouchTargetValidation.SetUp();
        EditorApplication.update += Tick;
    }
    private static void Tick()
    {
        try
        {
            double elapsed = EditorApplication.timeSinceStartup - started;
            if (stage == 0)
            {
                Require(!indicator.activeSelf, "short load must not flash");
                if (elapsed < .3) return;
                TouchTargetValidation.Validate();
                PurchaseNavigationValidation.Validate();
                manager.EndLoading();
                stage = 1; started = EditorApplication.timeSinceStartup;
            }
            else if (stage == 1)
            {
                Require(!indicator.activeSelf, "cancelled indicator must stay hidden");
                if (elapsed < 1.2) return;
                Time.timeScale = 0;
                manager.StartLoading();
                stage = 2; started = EditorApplication.timeSinceStartup;
            }
            else if (stage == 2)
            {
                if (elapsed < .9) Require(!indicator.activeSelf, "indicator shown before one second");
                if (elapsed < 1.3) return;
                Require(indicator.activeSelf, "slow load must show even when game is paused");
                manager.EndLoading();
                Require(!indicator.activeSelf, "completion must hide immediately");
                Time.timeScale = 1;
                Debug.Log("ANDROID_UX_VALIDATION_PASSED: fast, cancelled, slow and paused loading; achievement and flow rules.");
                Finish(0);
            }
        }
        catch (Exception error) { Debug.LogException(error); Finish(1); }
    }
    private static void Finish(int code)
    {
        EditorApplication.update -= Tick;
        SessionState.SetBool(RunKey, false);
        EditorApplication.Exit(code);
    }
}
