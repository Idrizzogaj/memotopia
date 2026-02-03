using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashViewController : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(CallLoadingScene());
    }

    IEnumerator CallLoadingScene()
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("LoginScene");
        asyncOperation.allowSceneActivation = false;
        yield return new WaitForSeconds(3);
        asyncOperation.allowSceneActivation = true;

    }
}
