using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MemoryExpansionValidation
{
    private static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
    public static void Validate()
    {
        // Isolate preferences from all real accounts; restore even after a failed check.
        var oldUser=UserConstants.s_user;
        bool oldChallenge=ChallengeConstants.s_isChallenge;
        UserConstants.s_user=new Assets.Script.Models.User { id=1999999991 };
        var achievementKey="achievements.verified.v2.1999999991";
        bool hadAchievements=PlayerPrefs.HasKey(achievementKey);
        string priorAchievements=PlayerPrefs.GetString(achievementKey,"");
        PlayerPrefs.SetString(achievementKey,"{\"completed\":[\"memory-athlete\"],\"pending\":[]}");
        var keys=new[]{"content.choice.v1.1999999991","memory.settings.v1.1999999991.digits","memory.settings.v1.1999999991.imageNumbers","memory.settings.v1.1999999991.wordNumbers"};
        var had=keys.Select(PlayerPrefs.HasKey).ToArray();var previous=keys.Select(k=>PlayerPrefs.GetInt(k)).ToArray();
        try
        {
            ChallengeConstants.s_isChallenge=false;
            var words=new HashSet<string>();
            for(int code=0;code<1000;code++) Check(words.Add(MajorWordCatalog.Word(code)),"Duplicate word "+code);
            Check(MajorWordCatalog.Word(925)=="PANEL","925 example changed unexpectedly");
            var deckWords=new HashSet<string>();
            for(int i=0;i<52;i++)
            {
                Check(deckWords.Add(DeckCatalog.Get(i).Word),"Duplicate deck word");
                Check(DeckCatalog.Face(i)!=null && DeckCatalog.CompactFace(i)!=null,"Missing full/compact card "+i);
            }
            Check(DeckCatalog.Get(17).Word=="DOLL","Diamond 5 must be DOLL");
            MemorySettings.Digits=3;MemorySettings.ShowWordNumbers=true;
            CardFaces.Selected=CardFaces.Choice.Numbers;
            bool foundLeadingZero=false,foundAbove99=false;
            for(int round=0;round<12;round++)
            {
                CardFaces.BeginSession();var numbers=new HashSet<string>();
                for(int id=1;id<=100;id++)
                {
                    var number=CardFaces.NumberText(id);
                    Check(number.Length==3 && numbers.Add(number),"Three-digit uniqueness/format");
                    Check(number==CardFaces.WordCode(id).ToString("D3"),"Number/word association drift");
                    foundLeadingZero|=number[0]=='0';foundAbove99|=int.Parse(number)>99;
                    Check(CardFaces.KindOf(id)==CardKind.Number,"Wrong kind");
                    if(id<=3) Check(CardFaces.Face(id)!=null,"Number render");
                }
                // Mid-round preferences must not mutate the current round.
                var frozen=CardFaces.NumberText(1);MemorySettings.Digits=2;
                Check(CardFaces.NumberText(1)==frozen,"Settings mutated active round");MemorySettings.Digits=3;
            }
            Check(foundLeadingZero && foundAbove99,"Three-digit range never exercised");
            CardFaces.Selected=CardFaces.Choice.Images;
            Check((CardFaces.Selected&CardFaces.Choice.Images)==0,"Three-digit images must not be falsely relabelled");
            CardFaces.BeginSession();Check(CardFaces.KindOf(1)==CardKind.Word,"Three-digit picture fallback must be words");
            Check(CardFaces.Face(1)!=null,"Word/number overlay render");
            MemorySettings.Digits=2;CardFaces.Selected=CardFaces.Choice.Numbers;CardFaces.BeginSession();
            Check(CardFaces.NumberText(1)=="00" && CardFaces.NumberText(100)=="99","Two-digit regression");
            CardFaces.Selected=CardFaces.Choice.Words;MemorySettings.ShowWordNumbers=true;CardFaces.BeginSession();
            Check(CardFaces.WordCode(1)==0 && CardFaces.WordCode(100)==99,"Two-digit word labels must use 00-99, not thousand pegs");
            Check(CardFaces.Face(1)!=null && CardFaces.WideFace(100)!=null,"Two-digit word render");
            Debug.Log("TWO_DIGIT_WORDS_VALIDATION_PASSED");
            CardFaces.Selected=CardFaces.Choice.Deck;CardFaces.BeginSession();
            Check(CardFaces.ContentCount==52,"Deck range");
            for(int id=1;id<=52;id++) Check(CardFaces.KindOf(id)==CardKind.PlayingCard && CardFaces.Face(id)!=null && CardFaces.WideFace(id)!=null,"Deck face mapping");
            Check(MemoryAthlete.PairsCards<=52 && MemoryAthlete.FlashImages<=52 && MemoryAthlete.BoxesBoxes<=52,"Deck too small for top level");
            ChallengeConstants.s_isChallenge=true;CardFaces.BeginSession();
            Check(CardFaces.ContentCount==100 && CardFaces.KindOf(1)==CardKind.Image,"Challenge rules regression");
            ChallengeConstants.s_isChallenge=false;CardFaces.EndSession();

            EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
            var menu=UnityEngine.Object.FindObjectOfType<MenuNavigation>();Check(menu!=null,"Menu missing");
            TipsEnhancer.Ensure(menu);TipsEnhancer.Ensure(menu);
            var screens=menu.tipsScreen.transform.Find("Screens");
            var major=screens.Find("MajorSystemPage");var deck=screens.Find("DeckGuidePage");
            Check(major!=null && deck!=null,"Tip pages missing");
            var guideList=menu.tipsScreen.transform.Find("Scroll View/Viewport/Content");
            Check(guideList.Find("TheDeckGuide")!=null,"Visible Deck guide entry missing");
            var link=screens.Find("LinkGuidePage");Check(link!=null && link.Find("ReadBar")!=null,"Link Method instructions/read badge missing");
            guideList.Find("LinkMethod").GetComponent<Button>().onClick.Invoke();Check(link.gameObject.activeSelf,"Link card opens wrong page");link.gameObject.SetActive(false);
            Check(major.Find("ReadBar")!=null && deck.Find("ReadBar")!=null,"Read bar missing");
            foreach(var page in new[]{major,deck})
            {
                var groups=page.GetComponentsInChildren<Button>(true).Where(b=>b.name.StartsWith("Expand ")).ToArray();
                Check(groups.Length==(page==major?10:13),"Wrong group count");
                foreach(var group in groups) group.onClick.Invoke();
                var pictureCount=page.GetComponentsInChildren<Image>(true).Count(i=>i.name=="Picture" || i.name=="Card");
                Check(pictureCount==(page==major?106:52),"Missing gallery rows: "+pictureCount);
            }
            MemorySettingsPanel.Ensure(menu);MemorySettingsPanel.Ensure(menu);
            var entries=menu.sideMenu.GetComponentsInChildren<Button>(true).Where(MemorySettingsPanel.IsEntry).ToArray();
            Check(entries.Length==1,"Settings must be wired once");
            entries[0].onClick.Invoke();
            Check(menu.GetComponent<MemorySettingsPanel>()!=null,"Settings panel not attached");
            Check(ExtraAchievements.All.Count(d=>d.Key=="tip-the-deck")==1,"Deck achievement missing/duplicate");
            RenderUi(menu,major,deck);
            Debug.Log("MEMORY_EXPANSION_VALIDATION_PASSED: 1000 words, 52 cards, leading zeroes, round identity, challenge isolation, full galleries, Settings, read badges.");
        }
        finally
        {
            CardFaces.EndSession();UserConstants.s_user=oldUser;ChallengeConstants.s_isChallenge=oldChallenge;
            if(hadAchievements)PlayerPrefs.SetString(achievementKey,priorAchievements);else PlayerPrefs.DeleteKey(achievementKey);
            for(int i=0;i<keys.Length;i++) { if(had[i])PlayerPrefs.SetInt(keys[i],previous[i]);else PlayerPrefs.DeleteKey(keys[i]); }
            PlayerPrefs.Save();
        }
    }

    private static void RenderUi(MenuNavigation menu,Transform major,Transform deck)
    {
        string folder="../HubRevision-20261020/tips-review";Directory.CreateDirectory(folder);
        var settings=menu.sideMenu.GetComponentInParent<Canvas>().rootCanvas.transform.Find("MemorySettings");
        Check(settings!=null,"Settings overlay missing");
        var root=new GameObject("ReviewCanvas",typeof(RectTransform),typeof(Canvas));
        root.layer=30;
        var rect=(RectTransform)root.transform;rect.sizeDelta=new Vector2(800,1422);
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
        var cameraGo=new GameObject("ReviewCamera",typeof(Camera));var camera=cameraGo.GetComponent<Camera>();
        camera.orthographic=true;camera.orthographicSize=711;camera.transform.position=new Vector3(0,0,-20);
        camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;camera.cullingMask=1<<30;canvas.worldCamera=camera;
        foreach(var page in new[]{settings,major,deck})
        {
            page.SetParent(rect,false);var pr=(RectTransform)page;pr.anchorMin=Vector2.zero;pr.anchorMax=Vector2.one;pr.offsetMin=pr.offsetMax=Vector2.zero;
            foreach(var t in page.GetComponentsInChildren<Transform>(true)) t.gameObject.layer=30;
            page.gameObject.SetActive(false);
        }
        settings.gameObject.SetActive(true);Capture(camera,800,1422,folder+"/settings-phone.png");settings.gameObject.SetActive(false);
        foreach(var page in new[]{major,deck})
        {
            page.gameObject.SetActive(true);
            var scroll=page.GetComponentInChildren<ScrollRect>(true);scroll.verticalNormalizedPosition=1;
            Capture(camera,800,1422,folder+"/"+page.name+"-phone.png");
            Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
            // Scroll to the first expanded catalogue group, immediately below the article.
            var header=scroll.content.GetComponentsInChildren<Button>(true).First(b=>b.name.StartsWith("Expand "));
            float y=-((RectTransform)header.transform).anchoredPosition.y-((RectTransform)header.transform).rect.height*.5f;
            scroll.content.anchoredPosition=new Vector2(0,y);
            Capture(camera,800,1422,folder+"/"+page.name+"-catalogue-phone.png");
            page.gameObject.SetActive(false);
        }
        rect.sizeDelta=new Vector2(1200,1600);camera.orthographicSize=800;
        settings.gameObject.SetActive(true);Capture(camera,1200,1600,folder+"/settings-tablet.png");
        UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraGo);
    }
    private static void Capture(Camera camera,int width,int height,string path)
    {
        foreach(var graphic in UnityEngine.Object.FindObjectsOfType<Graphic>()) graphic.SetAllDirty();
        Canvas.ForceUpdateCanvases();
        var target=new RenderTexture(width,height,24);camera.targetTexture=target;
        camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
        var image=new Texture2D(width,height,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
        int ink=image.GetPixels32().Count(p=>p.r<230 || p.g<230 || p.b<230);
        Check(ink>1000,"Blank UI render: "+path);
        File.WriteAllBytes(path,image.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);
    }
}
