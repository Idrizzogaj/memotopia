using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    private static LoadingManager instance;
    private GameObject loadingScreen;
    private bool navigating;
    public bool IsNavigating { get { return navigating; } }

    private void Awake()
    {
        loadingScreen = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;
        if (instance != null && instance != this)
        {
            if (instance.loadingScreen != null || loadingScreen == null) { Destroy(gameObject); return; }
            Destroy(instance.gameObject); // Replace an editor-only fallback with the real loading canvas.
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void Navigate(string scene)
    {
        if (instance == null) instance = FindObjectOfType<LoadingManager>();
        if (instance == null) instance = new GameObject("SceneTransitions").AddComponent<LoadingManager>();
        instance.GoToSceneWithLoading(scene);
    }

    public void StartLoading() { if (loadingScreen != null) loadingScreen.SetActive(true); }
    public void EndLoading() { if (!navigating && loadingScreen != null) loadingScreen.SetActive(false); }
    public void EndLoadingWithDelay() { EndLoading(); }
    public void GoToSceneWithLoadingInstantly(string scene) { GoToSceneWithLoading(scene); }
    public void GoToSceneWithLoading(string scene)
    {
        if (navigating || string.IsNullOrEmpty(scene)) return;
        navigating = true;
        StartCoroutine(Load(scene));
    }

    private IEnumerator Fade(UnityEngine.CanvasGroup group, float from, float to, float duration)
    {
        for (float elapsed = 0; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            if (group != null) group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        if (group != null) group.alpha = to;
    }

    private IEnumerator Load(string scene)
    {
        float started = Time.realtimeSinceStartup;
        StartLoading();
        var fade = loadingScreen == null ? null : loadingScreen.GetComponent<UnityEngine.CanvasGroup>();
        if (loadingScreen != null && fade == null) fade = loadingScreen.AddComponent<UnityEngine.CanvasGroup>();
        yield return Fade(fade, 0, 1, .075f);
        var operation = SceneManager.LoadSceneAsync(scene);
        if (operation != null) yield return operation;
        yield return Fade(fade, 1, 0, .075f);
        navigating = false;
        EndLoading();
        if (fade != null) fade.alpha = 1;
        Debug.Log("Scene transition " + scene + ": " + ((Time.realtimeSinceStartup - started) * 1000f).ToString("F0") + " ms");
    }
}
