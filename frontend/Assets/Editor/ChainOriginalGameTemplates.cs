using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
public static class ChainOriginalGameTemplates {
 static Transform[] originals,copies;
 static void Mark(Component source,string name){if(source!=null)Mark(source.gameObject,name);}
 static void Mark(GameObject source,string name){if(source==null)return;int n=Array.IndexOf(originals,source.transform);if(n>=0)copies[n].name=name;}
 static GameObject Clone(Canvas canvas){var clone=UnityEngine.Object.Instantiate(canvas.gameObject);originals=canvas.GetComponentsInChildren<Transform>(true);copies=clone.GetComponentsInChildren<Transform>(true);return clone;}
 static void Save(GameObject clone,string name){
  foreach(var b in clone.GetComponentsInChildren<Button>(true))b.onClick=new Button.ButtonClickedEvent();
  foreach(var c in clone.GetComponentsInChildren<MonoBehaviour>(true))if(!(c is UnityEngine.EventSystems.UIBehaviour))UnityEngine.Object.DestroyImmediate(c);
  foreach(var c in clone.GetComponentsInChildren<Animator>(true))UnityEngine.Object.DestroyImmediate(c);
  foreach(var c in clone.GetComponentsInChildren<GraphicRaycaster>(true))UnityEngine.Object.DestroyImmediate(c);
  foreach(var c in clone.GetComponentsInChildren<CanvasScaler>(true))UnityEngine.Object.DestroyImmediate(c);
  foreach(var c in clone.GetComponentsInChildren<Canvas>(true))UnityEngine.Object.DestroyImmediate(c);
  clone.name=name;PrefabUtility.SaveAsPrefabAsset(clone,"Assets/Resources/Prefabs/"+name+".prefab");
  File.WriteAllLines("../ChainReuseFix/"+name+"-hierarchy.txt",clone.GetComponentsInChildren<Transform>(true).Select(t=>t.name+" | "+(t.parent==null?"root":t.parent.name)+" | "+t.gameObject.activeSelf));
  UnityEngine.Object.DestroyImmediate(clone);
 }
 public static void Export(){
  EditorSceneManager.OpenScene("Assets/_Scenes/PairsScene.unity");var p=UnityEngine.Object.FindObjectOfType<PairsManager>();
  var canvas=p.img1.GetComponentInParent<Canvas>().rootCanvas;var clone=Clone(canvas);
  Mark(p.img1,"Exact study image");Mark(p.img2,"Exact second image");Mark(p.linkedPairsScreen,"Exact study screen");Mark(p.linkedPairsScreenIpad,"Unused tablet study");Mark(p.gameScreen,"Unused pairs recall");Mark(p.rsg,"Original countdown");Mark(p.firstCountDown,"Exact study timer");
  Mark(p.img1Ipad,"Exact tablet study image");Mark(p.img2Ipad,"Exact tablet second image");Mark(p.firstCountDownIpad,"Exact study timer");
  foreach(var b in canvas.GetComponentsInChildren<Button>(true))for(int i=0;i<b.onClick.GetPersistentEventCount();i++){var method=b.onClick.GetPersistentMethodName(i);if(method=="NextPair")Mark(b,"Exact Next");if(method.Contains("Quit"))Mark(b,"Exact Quit");}
  Save(clone,"ExactPairsChainStudy");
  EditorSceneManager.OpenScene("Assets/_Scenes/FlashScene.unity");var f=UnityEngine.Object.FindObjectOfType<FlashManager>();canvas=f.timerText.GetComponentInParent<Canvas>().rootCanvas;clone=Clone(canvas);
  Mark(f.recallPanel,"Exact recall grid");Mark(f.topPanel,"Exact recall header");Mark(f.topPanelNext,"Exact recall header target");Mark(f.activeCellsPanel,"Unused active cells");Mark(f.deactiveCellsPanel,"Unused inactive cells");Mark(f.rsg,"Original countdown");Mark(f.timerText,"Exact recall timer");
  for(int i=0;i<f.hearts.Length;i++)Mark(f.hearts[i],"Exact heart "+i);
  foreach(var b in canvas.GetComponentsInChildren<Button>(true))for(int i=0;i<b.onClick.GetPersistentEventCount();i++)if(b.onClick.GetPersistentMethodName(i).Contains("Quit"))Mark(b,"Exact Quit");
  Save(clone,"ExactFlashChainRecall");Debug.Log("ORIGINAL_CHAIN_GAME_TEMPLATES_EXPORTED");
 }
}
