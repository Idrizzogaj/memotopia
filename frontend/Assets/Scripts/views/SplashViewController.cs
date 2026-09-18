using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashViewController : MonoBehaviour
{
    private void Start() { SceneManager.LoadSceneAsync("LoginScene"); }
}
