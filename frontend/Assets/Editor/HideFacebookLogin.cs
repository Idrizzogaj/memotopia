using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class HideFacebookLogin
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_Scenes/LoginScene.unity");
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "fbLogin")
            {
                go.SetActive(false);
                Debug.Log("Disabled fbLogin: " + go.name);
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }
}
