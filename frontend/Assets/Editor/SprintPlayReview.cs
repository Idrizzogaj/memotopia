using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
[InitializeOnLoad] public static class SprintPlayReview {
 static SprintPlayReview(){EditorApplication.playModeStateChanged+=Changed;EditorApplication.update+=Watch;}
 public static void Run(){
  SessionState.SetBool("SprintPlayOldOptionsEnabled",EditorSettings.enterPlayModeOptionsEnabled);SessionState.SetInt("SprintPlayOldOptions",(int)EditorSettings.enterPlayModeOptions);
  EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
  EditorApplication.LockReloadAssemblies();SessionState.SetBool("SprintPlayReview",true);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);EditorApplication.isPlaying=true;
 }
 [MenuItem("SprintReview/Restore")]
 public static void Restore(){EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool("SprintPlayOldOptionsEnabled",false);EditorSettings.enterPlayModeOptions=(EnterPlayModeOptions)SessionState.GetInt("SprintPlayOldOptions",0);EditorApplication.UnlockReloadAssemblies();}
 static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("SprintPlayReview",false)){SessionState.SetBool("SprintPlayReview",false);SessionState.SetBool("SprintPlayRunning",true);SessionState.SetFloat("SprintPlayStart",(float)EditorApplication.timeSinceStartup);var root=new GameObject("Sprint play checks");Object.DontDestroyOnLoad(root);root.AddComponent<SprintPlayDriver>();}}
 static void Watch(){if(EditorApplication.isPlaying && SessionState.GetBool("SprintPlayRunning",false) && EditorApplication.timeSinceStartup-SessionState.GetFloat("SprintPlayStart",0)>180){SessionState.SetBool("SprintPlayRunning",false);PlayerPrefs.SetString("token",SessionState.GetString("SprintPlayPriorToken",""));PlayerPrefs.Save();Restore();Debug.LogError("SPRINT_PLAY_WATCHDOG_TIMEOUT");EditorApplication.Exit(1);}}
}
