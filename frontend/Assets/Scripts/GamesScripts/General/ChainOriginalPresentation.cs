using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Uses exported original scene hierarchies. No replacement buttons, card frames or grids.
public sealed class ChainOriginalPresentation {
 private Text recallClock,timerCaption;
 public GameObject Root;public Image Study;public Button Next;public RectTransform Grid;public Button Cell;
 Transform Find(string name){return Root.GetComponentsInChildren<Transform>(true).First(t=>t.name==name);}
 void Active(string name,bool value){foreach(var t in Root.GetComponentsInChildren<Transform>(true).Where(t=>t.name==name))t.gameObject.SetActive(value);}
 public ChainOriginalPresentation(Transform parent,bool study,UnityEngine.Events.UnityAction quit,UnityEngine.Events.UnityAction next,Material theme){
  Root=Object.Instantiate(Resources.Load<GameObject>("Prefabs/"+(study?"ExactPairsChainStudy":"ExactFlashChainRecall")),parent,false);
  var r=(RectTransform)Root.transform;r.localScale=Vector3.one;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.anchoredPosition3D=Vector3.zero;
  foreach(var b in Root.GetComponentsInChildren<Button>(true)){b.onClick=new Button.ButtonClickedEvent();if(b.name=="Exact Quit")b.onClick.AddListener(quit);}
  Active("Original countdown",false);
  Active("HowToPlay",false);
  if(study){
   bool tablet=Mathf.RoundToInt(r.rect.width/r.rect.height*4)==3;
   Active("Exact study screen",!tablet);Active("Unused tablet study",tablet);Active("Unused pairs recall",false);
   Study=Find(tablet?"Exact tablet study image":"Exact study image").GetComponent<Image>();
   Find(tablet?"Exact tablet second image":"Exact second image").parent.gameObject.SetActive(false);
   foreach(var layout in Study.GetComponentsInParent<LayoutGroup>())layout.enabled=false;
   var card=(RectTransform)Study.transform.parent;var originalCardSize=card.rect.size;
   // The original Pairs card pivots on its right edge. Centre the frame itself,
   // not that edge, while preserving the original frame and image dimensions.
   card.anchorMin=card.anchorMax=card.pivot=new Vector2(.5f,.5f);card.sizeDelta=originalCardSize;card.anchoredPosition=Vector2.zero;
   Active("BackgroundImage",false);
   // The full-screen Chain overlay already supplies the purple background. The copied Pairs
   // background has its own inset/texture and can expose turquoise bands on device aspect ratios.
   // Hide only that Graphic; its original card and NEXT children keep their exact layout.
   var background=Find(tablet?"Unused tablet study":"Exact study screen").GetComponent<Image>();if(background!=null)background.enabled=false;
   var canvasBackground=Root.GetComponent<Image>();if(canvasBackground!=null)canvasBackground.enabled=false;
   Next=Root.GetComponentsInChildren<Button>(true).First(b=>b.name=="Exact Next" && b.gameObject.activeInHierarchy);Next.onClick.AddListener(next);
   foreach(var label in Next.GetComponentsInChildren<Text>(true))label.text="NEXT";
   foreach(var text in Root.GetComponentsInChildren<Text>(true))if(text.name=="Exact study timer")text.text="";
  }else{
   Active("Unused active cells",false);Active("Unused inactive cells",false);Active("Exact recall grid",true);
   var header=(RectTransform)Find("Exact recall header");header.anchoredPosition=((RectTransform)Find("Exact recall header target")).anchoredPosition;
   Grid=(RectTransform)Find("Exact recall grid");Cell=Grid.GetChild(0).GetComponent<Button>();Cell.gameObject.SetActive(false);
   recallClock=Find("Exact recall timer").GetComponent<Text>();recallClock.text="";
   foreach(var text in header.GetComponentsInChildren<Text>(true))if(text.text=="Timer"){timerCaption=text;text.gameObject.SetActive(false);}
  }
  foreach(var button in Root.GetComponentsInChildren<Button>(true).Where(b=>b.name=="Exact Quit"))foreach(var image in button.GetComponentsInChildren<Image>(true))image.material=theme;
  foreach(var image in Root.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Exact heart "))){image.color=Color.white;image.material=theme;}
 }
 public void ShowRecallClock(int seconds,bool untimed=false){
  if(recallClock==null)return;recallClock.color=ChainGame.Purple;recallClock.text=untimed?"∞":Mathf.Max(0,seconds).ToString();
  if(timerCaption!=null)timerCaption.gameObject.SetActive(true);
 }
 public void Hearts(int remaining){for(int i=0;i<5;i++){var t=Root.GetComponentsInChildren<Transform>(true).FirstOrDefault(x=>x.name=="Exact heart "+i);if(t!=null)t.gameObject.SetActive(i<remaining);}}
 public void FitOverflow(int count){
  Canvas.ForceUpdateCanvases();var layout=Grid.GetComponent<GridLayoutGroup>();
  int columns=Mathf.Max(1,Mathf.FloorToInt((Grid.rect.width-layout.padding.horizontal+layout.spacing.x)/(layout.cellSize.x+layout.spacing.x)));
  int rows=Mathf.CeilToInt(count/(float)columns);float height=layout.padding.vertical+rows*(layout.cellSize.y+layout.spacing.y)-layout.spacing.y;
  if(height<=Grid.rect.height)return;
  var viewport=new GameObject("Flash grid viewport",typeof(RectTransform),typeof(RectMask2D),typeof(ScrollRect));var rect=(RectTransform)viewport.transform;
  rect.SetParent(Grid.parent,false);rect.anchorMin=Grid.anchorMin;rect.anchorMax=Grid.anchorMax;rect.pivot=Grid.pivot;rect.sizeDelta=Grid.sizeDelta;rect.anchoredPosition=Grid.anchoredPosition;
  Grid.SetParent(rect,false);Grid.anchorMin=new Vector2(0,1);Grid.anchorMax=Vector2.one;Grid.pivot=new Vector2(.5f,1);Grid.sizeDelta=new Vector2(0,height);Grid.anchoredPosition=Vector2.zero;
  var scroll=viewport.GetComponent<ScrollRect>();scroll.horizontal=false;scroll.viewport=rect;scroll.content=Grid;
  Canvas.ForceUpdateCanvases();scroll.Rebuild(CanvasUpdate.PostLayout);scroll.verticalNormalizedPosition=1;Canvas.ForceUpdateCanvases();
  Find("Exact recall header").SetAsLastSibling();Find("Exact Quit").SetAsLastSibling();
 }
 public void Destroy(){if(Root!=null)CardFaces.DestroyGenerated(Root);}
}
