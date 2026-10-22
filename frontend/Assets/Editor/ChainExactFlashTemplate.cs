using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class ChainExactFlashTemplate {
 public static void Review() {Export();AssetDatabase.Refresh();ChainReleaseValidation.Validate();}
 public static void Export() {
  EditorSceneManager.OpenScene("Assets/_Scenes/LevelsScene.unity");
  var source=Object.FindObjectOfType<LevelScript>();
  var clone=Object.Instantiate(source.BackgroundImage.gameObject);clone.name="Exact Flash levels";
  var originals=source.BackgroundImage.GetComponentsInChildren<Transform>(true);
  var copies=clone.GetComponentsInChildren<Transform>(true);
  for(int i=0;i<source.LevelsCell.Length;i++)for(int k=0;k<originals.Length;k++)if(originals[k]==source.LevelsCell[i].transform)copies[k].name="Chain Level "+(i+1);
  for(int k=0;k<originals.Length;k++)if(originals[k]==source.TopGameName.transform)copies[k].name="Chain title";
  foreach(var b in clone.GetComponentsInChildren<Button>(true))b.onClick=new Button.ButtonClickedEvent();
  foreach(var c in clone.GetComponentsInChildren<MonoBehaviour>(true))if(!(c is UnityEngine.EventSystems.UIBehaviour))Object.DestroyImmediate(c);
  foreach(var raycaster in clone.GetComponentsInChildren<GraphicRaycaster>(true))Object.DestroyImmediate(raycaster);
  foreach(var scaler in clone.GetComponentsInChildren<CanvasScaler>(true))Object.DestroyImmediate(scaler);
  foreach(var canvas in clone.GetComponentsInChildren<Canvas>(true))Object.DestroyImmediate(canvas);
  System.IO.Directory.CreateDirectory("Assets/Resources/Prefabs");
  PrefabUtility.SaveAsPrefabAsset(clone,"Assets/Resources/Prefabs/ExactFlashChainLevels.prefab");Object.DestroyImmediate(clone);
  Debug.Log("EXACT_FLASH_TEMPLATE_EXPORTED");
 }
}
