using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    private static GameObject instance;
    private GameObject loadingScreen;

    private void Awake() {
        loadingScreen = gameObject.transform.GetChild(0).gameObject;
        DontDestroyOnLoad(gameObject);

        if (instance == null)
            instance = gameObject;
        else
            Destroy(gameObject);
    }

    public void StartLoading() {
        loadingScreen.SetActive(true);
    }

    public void EndLoading() {
        if (loadingScreen.activeSelf)
            loadingScreen.SetActive(false);
    }

    public void GoToSceneWithLoading(string sceneToLoad)
    {
        StartCoroutine(StartLoad(sceneToLoad));
    }

    public void GoToSceneWithLoadingInstantly(string sceneToLoad)
    {
        StartCoroutine(StartInstantLoad(sceneToLoad));
    }

    IEnumerator StartLoad(string sceneToLoad)
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(2);
        // yield return StartCoroutine(FadeLoadingScreen(1, 1));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!operation.isDone)
        {
            yield return null;
        }

        // yield return StartCoroutine(FadeLoadingScreen(0, 1));
        loadingScreen.SetActive(false);
    }

    IEnumerator StartInstantLoad(string sceneToLoad)
    {
        loadingScreen.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        while (!operation.isDone)
        {
            yield return null;
        }
        loadingScreen.SetActive(false);
    }

    public void EndLoadingWithDelay()
    {
        if (loadingScreen.activeSelf)
            StartCoroutine(EndLoadingWithDelayEnum());
    }

    IEnumerator EndLoadingWithDelayEnum()
    {
        yield return new WaitForSeconds(2);
        loadingScreen.SetActive(false);
    }
}
