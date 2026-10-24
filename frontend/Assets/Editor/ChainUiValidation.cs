using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ChainUiValidation
{
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    public static void Validate() {
        ChainValidation.Validate();
        var oldUser=UserConstants.s_user;bool oldChallenge=ChallengeConstants.s_isChallenge;
        UserConstants.s_user=new Assets.Script.Models.User{id=1999999994};ChallengeConstants.s_isChallenge=false;
        string[] keys={"chain.completed.v1.1999999994","content.choice.v1.1999999994","memory.settings.v1.1999999994.deckMode","achievements.verified.v2.1999999994"};
        var had=keys.Select(PlayerPrefs.HasKey).ToArray();var prior=keys.Select(k=>PlayerPrefs.GetInt(k)).ToArray();
        string priorBadge=PlayerPrefs.GetString(keys[3],"");
        // int preferences are restored separately; validation uses an isolated owner.
        GameObject root=null,camObj=null;
        try {
            foreach(var k in keys)PlayerPrefs.DeleteKey(k);
            CardFaces.Selected=CardFaces.Choice.Deck;MemorySettings.DeckMode=DeckDisplay.Cards;
            root=new GameObject("Chain review canvas",typeof(RectTransform),typeof(Canvas));
            var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1422);
            var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            camObj=new GameObject("Chain review camera",typeof(Camera));var cam=camObj.GetComponent<Camera>();
            cam.orthographic=true;cam.orthographicSize=711;cam.transform.position=new Vector3(0,0,-20);
            cam.nearClipPlane=.1f;cam.farClipPlane=100;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.white;cam.cullingMask=1<<30;canvas.worldCamera=cam;
            var screen=(RectTransform)ChainGame.Panel(rect,"Training",Vector2.zero,Vector2.one,Color.white).transform;
            ChainGame.Attach(screen);var game=screen.GetComponentInChildren<ChainGame>();game.Open();
            var exact=root.GetComponentsInChildren<Image>(true).Where(i=>i.name.StartsWith("Chain Level ")).ToArray();
            Check(exact.Length==20,"Exact Flash template must contain20 real level tiles");
            Check(ChainProgress.Unlocked(1) && ChainProgress.Unlocked(20) && !ChainProgress.Unlocked(2),"New account must only open1and20");
            Check(exact.Where(i=>i.name!="Chain Level 1" && i.name!="Chain Level 20").All(i=>!i.transform.GetChild(1).GetComponent<Button>().interactable),"Locked Chain buttons must not accept clicks");
            game.StartLevel(2);Check(typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game)==null,"Locked level bypass");
            Check(exact.All(i=>Mathf.Abs(i.transform.position.z)<1),"Copied Canvas retained source camera depth");
            var locked=root.GetComponentsInChildren<Button>(true).Where(b=>b.transform.parent.name=="Training Memory Athlete").ToArray();
            Check(locked.Length>0 && locked.All(b=>!b.interactable),"Athlete must be locked before level20clear");
            game.StartLevel(MemoryAthlete.Level);Check(typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game)==null,"Locked Athlete bypass");
            Capture(root,cam,800,1422,"chain-levels-phone");
            game.StartLevel(1);Capture(root,cam,800,1422,"chain-study-phone");
            var state=(ChainState)typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            for(int i=0;i<state.Sequence.Length;i++)game.NextCard();
            Capture(root,cam,800,1422,"chain-recall-phone");
            game.Choose(state.Sequence[1]);Check(state.Hearts==4 && state.Position==0,"UI wrong selection must not advance");
            foreach(int id in state.Sequence)game.Choose(id);
            Check(ChainProgress.Completed(1),"UI win not saved");Capture(root,cam,800,1422,"chain-win-phone");
            Check(ChainProgress.Unlocked(2) && !ChainProgress.Unlocked(3),"Win1mustopen2only");
            ChallengeConstants.s_isChallenge=true;
            game.StartLevel(20);Check(!ChallengeConstants.s_isChallenge,"Chain must not inherit challenge content");state=(ChainState)typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            Check(state.Sequence.Length==52 && state.Sequence.Distinct().Count()==52,"Full deck must contain each card once");
            for(int i=0;i<52;i++)game.NextCard();
            Capture(root,cam,800,1422,"chain-52-recall-phone");
            var scroll=root.GetComponentsInChildren<ScrollRect>(true).First(s=>s.name=="Flash grid viewport");
            Check(scroll.content.rect.height>scroll.viewport.rect.height,"52 cards must scroll on phone");
            foreach(int id in state.Sequence)game.Choose(id);
            Check(ChainProgress.Completed(20) && !ChainProgress.All,"UI must not grant all levels from just endpoints");
            rect.sizeDelta=new Vector2(1200,1600);cam.orthographicSize=800;
            game.ShowLevels();Capture(root,cam,1200,1600,"chain-levels-tablet");
            game.StartLevel(20);for(int i=0;i<52;i++)game.NextCard();Capture(root,cam,1200,1600,"chain-52-recall-tablet");
            int unchangedMask=ChainProgress.Mask;
            game.StartLevel(MemoryAthlete.Level);state=(ChainState)typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            Check(state.Sequence.Length==52 && state.Hearts==1 && state.MaxHearts==1,"Chain Athlete must be same52cards/1heart");
            for(int i=0;i<52;i++)game.NextCard();game.Choose(state.Sequence[1]);Check(state.Finished && !state.Won,"One Chain error must end Athlete");
            game.StartLevel(MemoryAthlete.Level);state=(ChainState)typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);for(int i=0;i<52;i++)game.NextCard();foreach(int id in state.Sequence)game.Choose(id);
            Check(TrainingAthleteTile.Completed("Chain") && ChainProgress.Mask==unchangedMask,"Athlete completion changed ordinary20levels");
            game.StartLevel(1);
            state=(ChainState)typeof(ChainGame).GetField("state",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
            UserConstants.s_user=new Assets.Script.Models.User{id=1999999995};
            int untouched=ChainProgress.Mask;
            game.Choose(state.Sequence[0]);
            Check(root.transform.Find("Chain overlay")==null && ChainProgress.Mask==untouched,"Account change must abort, not credit the next user");
            UserConstants.s_user=new Assets.Script.Models.User{id=1999999994};
            // Inspect the actual Training screen, including the fourth game and the deck submodes.
            EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity",OpenSceneMode.Additive);
            var menu=UnityEngine.Object.FindObjectOfType<MenuNavigation>();
            var challenge=Resources.FindObjectsOfTypeAll<ChallengeNavigation>().Where(n=>n.gameObject.scene.IsValid()).ToArray().FirstOrDefault();
            Check(challenge!=null,"Challenge navigation missing");TrainingHubLayout.ApplyChallenge(challenge);TrainingHubLayout.ApplyChallenge(challenge);
            foreach(var picture in new[]{challenge.pairsImage,challenge.boxesImage,challenge.flashImage}) {
                Check(picture.color==new Color32(36,189,224,255) || picture.color==new Color32(234,24,100,255) || picture.color==new Color32(254,191,75,255),"Challenge must use its original game colour");
                Check(picture.transform.Find("Approved game icon")!=null,"Challenge is missing approved shared symbol");
                Check(picture.transform.Cast<Transform>().Count(t=>t.name=="Approved game icon")==1,"Challenge icon duplication");
            }
            var training=(RectTransform)menu.trainingScreen.transform;training.SetParent(rect,false);training.anchorMin=Vector2.zero;training.anchorMax=Vector2.one;training.offsetMin=training.offsetMax=Vector2.zero;training.gameObject.SetActive(true);
            foreach(Transform c in rect)if(c!=training)c.gameObject.SetActive(false);
            rect.sizeDelta=new Vector2(945,2048);cam.orthographicSize=1024;
            BuildTrainingBar(training);Capture(root,cam,945,2048,"training-phone");
            UnityEngine.Object.DestroyImmediate(training.Find("ContentSelectorBar").gameObject);
            rect.sizeDelta=new Vector2(1200,1600);cam.orthographicSize=800;
            BuildTrainingBar(training);Capture(root,cam,1200,1600,"training-tablet");
            Debug.Log("CHAIN_UI_VALIDATION_PASSED: real Next/click callbacks, wrong heart, complete win, account-local save, 52 unique scrollable cards, phone/tablet captures.");
        } finally {
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);if(camObj!=null)UnityEngine.Object.DestroyImmediate(camObj);
            CardFaces.EndSession();for(int i=0;i<keys.Length;i++){if(!had[i])PlayerPrefs.DeleteKey(keys[i]);else if(i==3)PlayerPrefs.SetString(keys[i],priorBadge);else PlayerPrefs.SetInt(keys[i],prior[i]);}
            UserConstants.s_user=oldUser;ChallengeConstants.s_isChallenge=oldChallenge;PlayerPrefs.Save();
        }
    }
    static void BuildTrainingBar(RectTransform training) {
        var host=new GameObject("ContentSelectorBar",typeof(RectTransform));host.transform.SetParent(training,false);
        var bar=host.AddComponent<ContentSelectorBar>();
        var build=(System.Collections.IEnumerator)typeof(ContentSelectorBar).GetMethod("Build",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(bar,new object[]{training});
        while(build.MoveNext()){}
        
        Check(training.Find("Chain")!=null && training.Find("FOCUS section")==null && training.Find("ContentSelectorBar/Deck modes")==null,"Training integration missing or unreleased FOCUS visible");
        var nav=UnityEngine.Object.FindObjectOfType<TrainingNavigation>();
        foreach(string name in new[]{"PAIRS","BOXES","FLASH","CHAIN"}) {
            Check(training.Find("Focus "+name)==null,"Unreleased Focus placeholder visible");
        }
        var corners=new Vector3[4];
        foreach(var tile in new[]{nav.pairsImage.rectTransform,nav.boxesImage.rectTransform,nav.flashImage.rectTransform,(RectTransform)training.Find("Chain")}) {
            Check(tile.GetComponentsInChildren<Image>(true).Any(i=>i.name=="Symbol stroke") || tile.GetComponentInChildren<TrainingFlashSymbol>(true)!=null,"Game symbol missing: "+tile.name);
            tile.GetWorldCorners(corners);
            foreach(var point in corners) {
                var local=training.InverseTransformPoint(point);
                Check(local.x>=training.rect.xMin-1 && local.x<=training.rect.xMax+1 && local.y>=training.rect.yMin-1 && local.y<=training.rect.yMax+1,"Game button outside Training area: "+tile.name);
            }
        }
    }
    static void Capture(GameObject root,Camera cam,int w,int h,string name) {
        foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;
        foreach(var g in root.GetComponentsInChildren<Graphic>(true))g.SetAllDirty();Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(w,h,24);cam.targetTexture=target;cam.Render();
        var prior=RenderTexture.active;RenderTexture.active=target;
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();
        Check(tex.GetPixels32().Count(p=>p.r<220 || p.g<220 || p.b<220)>1000,"Blank Chain render");
        Directory.CreateDirectory("../HubRevision-20261020/review");File.WriteAllBytes("../HubRevision-20261020/review/"+name+".png",tex.EncodeToPNG());
        cam.targetTexture=null;RenderTexture.active=prior;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(target);
    }
}

