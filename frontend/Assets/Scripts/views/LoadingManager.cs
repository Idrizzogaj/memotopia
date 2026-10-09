using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager instance;
    private GameObject loadingScreen;
    private bool navigating;
    private Coroutine pendingIndicator;
    public const float IndicatorDelaySeconds = 1f;
    public bool IsNavigating { get { return navigating; } }

    private void Awake()
    {
        loadingScreen = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (instance != null && instance != this)
        {
            if (instance.loadingScreen != null || loadingScreen == null) { Destroy(gameObject); return; }
            Destroy(instance.gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        if (loadingScreen != null) loadingScreen.SetActive(false);
    }

    public static void Navigate(string scene)
    {
        if (instance == null) instance = FindObjectOfType<LoadingManager>();
        if (instance == null) instance = new GameObject("SceneTransitions").AddComponent<LoadingManager>();
        instance.GoToSceneWithLoading(scene);
    }

    public void StartLoading()
    {
        if (loadingScreen == null || loadingScreen.activeSelf || pendingIndicator != null) return;
        pendingIndicator = StartCoroutine(ShowWhenSlow());
    }

    private IEnumerator ShowWhenSlow()
    {
        yield return new WaitForSecondsRealtime(IndicatorDelaySeconds);
        pendingIndicator = null;
        if (loadingScreen != null) loadingScreen.SetActive(true);
    }

    public void EndLoading()
    {
        if (navigating) return;
        if (pendingIndicator != null) StopCoroutine(pendingIndicator);
        pendingIndicator = null;
        if (loadingScreen != null) loadingScreen.SetActive(false);
    }
    public void EndLoadingWithDelay() { EndLoading(); }
    public void GoToSceneWithLoadingInstantly(string scene) { GoToSceneWithLoading(scene); }
    public void GoToSceneWithLoading(string scene)
    {
        if (navigating || string.IsNullOrEmpty(scene)) return;
        navigating = true;
        StartCoroutine(Load(scene));
    }

    private IEnumerator Load(string scene)
    {
        float started = Time.realtimeSinceStartup;
        StartLoading();
        try
        {
            var operation = SceneManager.LoadSceneAsync(scene);
            if (operation != null) yield return operation;
        }
        finally
        {
            navigating = false;
            EndLoading();
        }
        Debug.Log("Scene transition " + scene + ": " + ((Time.realtimeSinceStartup - started) * 1000f).ToString("F0") + " ms");
    }
}