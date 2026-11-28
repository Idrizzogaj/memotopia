using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Validate the reusable board independently of the pending Flashing Pairs identity rule.
public static class FlashingPairsBoardValidation
{
    static void Check(bool value,string message){if(!value)throw new Exception("Flashing Pairs board: "+message);}
    public static void Validate(){
        Check(Application.productName=="MemotopiaSagaValidation","Use the isolated validation project.");
        var prior=UserConstants.s_user;UserConstants.s_user=new User{id=1999999825,username="Pairs board validation"};
        var achievements=GameManager.PendingAchievements.ToArray();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var root=new GameObject("Pairs board test canvas",typeof(RectTransform),typeof(Canvas));var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
        var camera=new GameObject("Pairs board camera",typeof(Camera)).GetComponent<Camera>();camera.orthographic=true;camera.transform.position=new Vector3(0,0,-20);camera.nearClipPlane=.1f;camera.farClipPlane=1000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;canvas.worldCamera=camera;
        var theme=new Material(Resources.Load<Shader>("MemotopiaTheme"));theme.SetColor("_ThemeColor",new Color32(36,189,224,255));
        SagaPairsBoard board=null;
        try{
            foreach(bool tablet in new[]{false,true})foreach(int count in new[]{4,42}){
                int width=tablet?1200:800,height=tablet?1600:1734;
                ((RectTransform)root.transform).sizeDelta=new Vector2(width,height);camera.orthographicSize=height*.5f;CardFaces.BeginImageSession();
                var sequence=Enumerable.Range(1,count).ToArray();var state=new ChainState(sequence,5);bool ready=true;
                board=new SagaPairsBoard(root.transform,sequence,()=>{},theme,(first,second)=>{bool right=state.ChoosePair(first,second);board.Hearts(state.Hearts);return right;},true,id=>CardFaces.SmallFace(id),()=>ready,5);
                board.FitAll(count<=10?2:6);board.ShowClock(3);board.Hearts(5);Canvas.ForceUpdateCanvases();
                var cards=board.Root.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("Saga pair ")).ToArray();
                Check(cards.Length==count && cards.All(b=>!b.interactable && b.image.overrideSprite!=Resources.Load<Sprite>("GamesResources/Pairs/M")),"Study shows every image and ignores input.");
                board.Choose(1);Check(state.Position==0,"Study cannot score.");
                var scroll=board.Root.GetComponentsInChildren<ScrollRect>().First();var viewport=scroll.viewport!=null?scroll.viewport:(RectTransform)scroll.transform;
                var corners=new Vector3[4];foreach(var card in cards){card.GetComponent<RectTransform>().GetWorldCorners(corners);foreach(var corner in corners){var point=viewport.InverseTransformPoint(corner);Check(point.x>=viewport.rect.xMin-.5f && point.x<=viewport.rect.xMax+.5f && point.y>=viewport.rect.yMin-.5f && point.y<=viewport.rect.yMax+.5f,"All study cards fit without scrolling.");}}
                typeof(SagaValidation).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{canvas,camera,width,height,"flashing-pairs-board-study-"+(tablet?"tablet":"phone")});
                board.BeginRecall(false);Check(cards.All(b=>b.interactable && b.image.overrideSprite==Resources.Load<Sprite>("GamesResources/Pairs/M")),"Recall hides every face before accepting input.");
                ready=false;board.Choose(1);Check(cards.First(b=>b.name=="Saga pair 1").interactable,"Modal input guard blocks card reveal.");ready=true;
                board.Choose(1);board.Choose(3);Check(state.Hearts==4 && state.Position==0,"Training experiment can charge a heart for a mismatch.");board.Tick(.6f);
                if(count==4){for(int i=1;i<=count;i+=2){board.Choose(i);board.Choose(i+1);}Check(state.Won && state.Hearts==4,"Correct pairs complete the board.");}
                else {for(int error=0;error<4;error++){board.Choose(1);board.Choose(3);board.Tick(.6f);}Check(state.Finished && !state.Won && state.Hearts==0,"Five mismatches exhaust the Training hearts.");}
                board.Destroy();board=null;CardFaces.EndSession();
            }
            Check(GameManager.PendingAchievements.SequenceEqual(achievements) && ChainProgress.Mask==0,"Experimental board cannot award badges or ordinary Chain progress.");
            Debug.Log("FLASHING_PAIRS_BOARD_VALIDATION_PASSED:full-grid study,4/42cards fit phone/tablet,study/paused input guards,hidden recall,mismatch hearts,match completion,no achievements.");
        }finally{if(board!=null)board.Destroy();CardFaces.EndSession();UnityEngine.Object.DestroyImmediate(theme);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(camera.gameObject);UserConstants.s_user=prior;}
    }
}
