using System;
using System.Linq;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Assets.Script.Constants;
public static class ChainReuseValidation {
 public static void ReviewAll(){Review();AthleteReleaseValidation.Review();}
 static void Check(bool v,string m){if(!v)throw new Exception(m);}
 static ChainState State(ChainGame g){return (ChainState)typeof(ChainGame).GetField("state",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(g);}
 public static void Review(){
  ChainOriginalGameTemplates.Export();
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);var old=UserConstants.s_user;UserConstants.s_user=new Assets.Script.Models.User{id=1999999966};
  var root=new GameObject("Reuse review",typeof(RectTransform),typeof(Canvas));var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,2048f*800/945);
  var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
  var camera=new GameObject("Review camera",typeof(Camera));var cam=camera.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=rect.rect.height/2;cam.transform.position=new Vector3(0,0,-20);cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.cullingMask=1<<30;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.white;canvas.worldCamera=cam;
  try {
   CardFaces.Selected=CardFaces.Choice.Images;MemorySettings.ShowImageNumbers=true;
   var screen=(RectTransform)ChainGame.Panel(rect,"Training",Vector2.zero,Vector2.one,Color.white).transform;ChainGame.Attach(screen);var game=screen.GetComponentInChildren<ChainGame>();game.Open();ChainProgress.Complete(1);game.StartLevel(2);
   var next=root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next" && b.gameObject.activeInHierarchy);
   Check(next.image.sprite==Resources.Load<GameObject>("Prefabs/ExactPairsChainStudy").GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next").image.sprite,"Next sprite not original");
   Check(!root.GetComponentsInChildren<Text>(true).Any(t=>t.gameObject.activeInHierarchy && (t.text.Contains("Memorise")||t.text.Contains("CHAIN •"))),"Extra study text");
   Capture(root,cam,"chain-original-study");
   CheckStudyCentre(root,rect,"Exact study image");
   for(int i=0;i<6;i++)next.onClick.Invoke();
   var grid=root.GetComponentsInChildren<GridLayoutGroup>(true).First(g=>g.name=="Exact recall grid");
   var original=Resources.Load<GameObject>("Prefabs/ExactFlashChainRecall").GetComponentsInChildren<GridLayoutGroup>(true).First(g=>g.name=="Exact recall grid");
   Check(grid.cellSize==original.cellSize && grid.spacing==original.spacing && grid.constraintCount==original.constraintCount,"Flash geometry changed");
   Check(!root.GetComponentsInChildren<Text>(true).Any(t=>t.gameObject.activeInHierarchy && (t.text.Contains("Recall 0")||t.text.Contains("Tap the cards"))),"Extra recall text");
   Capture(root,cam,"chain-original-recall");
   var state=State(game);var quit=root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Quit" && b.gameObject.activeInHierarchy);quit.onClick.Invoke();
   var dialog=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="MidGameQuitPanel(Clone)");Capture(root,cam,"chain-original-quit");
   game.Choose(state.Sequence[0]);Check(state.Position==0,"Quit dialog must block recall input");
   dialog.GetChild(0).GetChild(2).GetComponent<Button>().onClick.Invoke();Check(State(game)==state && !state.Finished,"No lost the round");
   quit.onClick.Invoke();dialog=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="MidGameQuitPanel(Clone)");dialog.GetChild(0).GetChild(3).GetComponent<Button>().onClick.Invoke();Check(State(game)==null,"Yes did not return to levels");
   game.StartLevel(20);next=root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next" && b.gameObject.activeInHierarchy);for(int i=0;i<52;i++)next.onClick.Invoke();Capture(root,cam,"chain-original-52");
   var scroll=root.GetComponentsInChildren<ScrollRect>(true).First(s=>s.name=="Flash grid viewport");
   Check(scroll.content.rect.height>scroll.viewport.rect.height,"Full52mustscroll without shrinking Flash cells");
   scroll.verticalNormalizedPosition=0;Canvas.ForceUpdateCanvases();Capture(root,cam,"chain-original-52-bottom");
   foreach(int id in State(game).Sequence)game.Choose(id);Check(State(game).Won,"Recall win callback regression");
   var win=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="WinPanel(Clone)");
   Check(win.GetComponentsInChildren<StarFillScript>(true).All(s=>s.enabled && s.EarnedStarsOverride==3),"Chain must reuse star animation with hearts-based stars");
   Capture(root,cam,"chain-original-win");
   win.GetChild(0).GetChild(11).GetComponent<Button>().onClick.Invoke();Check(!State(game).Finished && State(game).Sequence.Length==52,"Original replay button must restart Chain level");
   next=root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next" && b.gameObject.activeInHierarchy);for(int i=0;i<52;i++)next.onClick.Invoke();
   int wrong=State(game).Sequence[1];for(int i=0;i<5;i++)game.Choose(wrong);
   Check(State(game).Finished && !State(game).Won,"Five errors must lose Chain");
   var lose=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="LosePanel(Clone)");Capture(root,cam,"chain-original-loss");
   lose.GetChild(0).GetChild(3).GetComponent<Button>().onClick.Invoke();Check(State(game)==null,"Original loss home must return to Chain levels");
   game.StartLevel(MemoryAthlete.Level);Check(State(game).Hearts==1,"Athlete must have one heart");
   next=root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next" && b.gameObject.activeInHierarchy);for(int i=0;i<52;i++)next.onClick.Invoke();
   foreach(int id in State(game).Sequence)game.Choose(id);
   win=root.GetComponentsInChildren<Transform>(true).First(t=>t.name=="WinPanel(Clone)");
   Check(!win.GetComponentsInChildren<Image>(true).Any(i=>i.gameObject.activeInHierarchy && i.name.IndexOf("Star",StringComparison.OrdinalIgnoreCase)>=0),"Athlete must show no stars");
   Check(win.GetComponentsInChildren<Text>(true).Any(t=>t.text.Contains("500 XP")),"Athlete result must retain reward status");
   foreach(var popup in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="BadgePopup").ToArray())UnityEngine.Object.DestroyImmediate(popup.gameObject);
   Capture(root,cam,"chain-original-athlete-win");
   rect.sizeDelta=new Vector2(800,1600f*800/1200);game.StartLevel(2);
   Check(root.GetComponentsInChildren<Image>(true).Any(i=>i.name=="Exact tablet study image" && i.gameObject.activeInHierarchy),"Tablet must use original Pairs tablet panel");
   Capture(root,cam,"chain-original-tablet-study",1200,1600);
   CheckStudyCentre(root,rect,"Exact tablet study image");
   next=root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next" && b.gameObject.activeInHierarchy);for(int i=0;i<6;i++)next.onClick.Invoke();Capture(root,cam,"chain-original-tablet-recall",1200,1600);
   Debug.Log("CHAIN_ORIGINAL_REUSE_PASSED: originalNext/Flashgrid/sprites, no extra text, quitNo preserves state, quitYes leaves.");
  }finally{CardFaces.EndSession();UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(camera);UserConstants.s_user=old;}
 }
 static void CheckStudyCentre(GameObject root,RectTransform screen,string imageName){
  Canvas.ForceUpdateCanvases();
  var image=root.GetComponentsInChildren<Image>(true).First(i=>i.name==imageName && i.gameObject.activeInHierarchy);
  var frame=(RectTransform)image.transform.parent;
  var centre=screen.InverseTransformPoint(frame.TransformPoint(frame.rect.center));
  Check(Vector2.Distance(centre,screen.rect.center)<.1f,"Original white study frame is not centred: "+centre);
 }
 static void Capture(GameObject root,Camera cam,string name,int width=945,int height=2048){
  foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;Canvas.ForceUpdateCanvases();
  cam.orthographicSize=((RectTransform)root.transform).rect.height/2;
  var rt=new RenderTexture(width,height,24);cam.targetTexture=rt;cam.Render();var prior=RenderTexture.active;RenderTexture.active=rt;
  var tex=new Texture2D(width,height,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();System.IO.File.WriteAllBytes("../ChainReuseFix/"+name+".png",tex.EncodeToPNG());
  cam.targetTexture=null;RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
 }
}

