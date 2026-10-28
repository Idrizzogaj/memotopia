using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class StatisticsDeckValidation
{
    const int Owner=1999999901;
    static void Check(bool okay,string why){if(!okay)throw new Exception(why);}
    static object Field(object target,string name){return target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);}
    static void SelectScreen(GameObject screen,Canvas canvas){for(var node=screen.transform;node!=canvas.transform;node=node.parent){foreach(Transform sibling in node.parent)if(sibling!=node)sibling.gameObject.SetActive(false);node.gameObject.SetActive(true);}}
    static void Capture(Canvas canvas,Camera camera,string name,int width,int height){typeof(ChainReuseValidation).GetMethod("Capture",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{canvas.gameObject,camera,name+(TrainingHubLayout.ShowFocus?"":"-release20"),width,height});}
    static void ScrollTo(ScrollRect scroll,Transform section){StatisticsCards.Reflow(scroll.content);var item=(RectTransform)section;float top=-item.anchoredPosition.y-item.rect.height*(1-item.pivot.y);scroll.verticalNormalizedPosition=1-Mathf.Clamp01(top/Mathf.Max(1,scroll.content.rect.height-scroll.viewport.rect.height));Canvas.ForceUpdateCanvases();}
    static void Clear(){PlayerPrefs.DeleteKey("levels.v1."+Owner);PlayerPrefs.DeleteKey("levels.v1."+(Owner+1));PlayerPrefs.DeleteKey("practice.records.v1."+Owner);PlayerPrefs.DeleteKey("chain.completed.v1."+Owner);PlayerPrefs.DeleteKey("says.preview.v1."+Owner);for(int i=1;i<=20;i++){PlayerPrefs.DeleteKey("chain.stars.v1."+Owner+"."+i);PlayerPrefs.DeleteKey("says.preview.v1."+Owner+".stars."+i);}}
    public static void Review(){var prior=UserConstants.s_user;Clear();try{
        UserConstants.s_user=new User{id=Owner,username="Statistics review",userStatistics=new UserStatistics{numberOfChallenges=12,numberOfMatchChallenges=4,numberOfSingleChallenges=8,numberOfLossChallenges=2,numberOfDrawChallenges=1,challengeFormatsAvailable=true}};
        Check(!MemoryStatisticsPanel.Read("Pairs").known,"Unfetched is unknown, not zero");
        LevelProgress.Merge("Pairs",new[]{new GameLevel{level=1,stars=2},new GameLevel{level=2,stars=3},new GameLevel{level=2,stars=1}});
        LevelProgress.Merge("Boxes",new GameLevel[0]);LevelProgress.Merge("Flash",new[]{new GameLevel{level=20,stars=1}});
        var pairs=MemoryStatisticsPanel.Read("Pairs");Check(pairs.highest==2 && pairs.levels==2 && pairs.stars==5 && pairs.perfect==1,"Actual per-level awards, no duplicate counting");
        Check(MemoryStatisticsPanel.Read("Boxes").known && MemoryStatisticsPanel.Read("Boxes").stars==0,"Known empty progress");
        ChainProgress.Complete(3);ChainProgress.SaveStars(3,5);PlayerPrefs.SetInt("says.preview.v1."+Owner,3);PlayerPrefs.SetInt("says.preview.v1."+Owner+".stars.2",2);
        Check(MemoryStatisticsPanel.Read("Chain").stars==3,"Chain uses saved stars");Check(MemoryStatisticsPanel.Read("Says").stars==3,"Says uses its existing legacy minimum star rule");
        PracticeStats.Save(Owner,"Memory.Pairs",1,4,140);PracticeStats.Save(Owner,"Focus.Boxes",1,5,235);
        UserConstants.s_user=new User{id=Owner+1};Check(!MemoryStatisticsPanel.Read("Pairs").known && MemoryStatisticsPanel.Read("Chain").levels==0 && MemoryStatisticsPanel.Read("Says").levels==0,"No cross-account records");
        UserConstants.s_user=new User{id=Owner,username="Statistics review",userStatistics=new UserStatistics{numberOfChallenges=12,numberOfMatchChallenges=4,numberOfSingleChallenges=8,numberOfLossChallenges=2,numberOfDrawChallenges=1,challengeFormatsAvailable=true}};
        EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");var menu=Resources.FindObjectsOfTypeAll<MenuNavigation>().First(m=>m.gameObject.scene.IsValid());var stats=menu.GetComponent<StatisticsView>();
        var canvas=menu.statisticsScreen.GetComponentsInParent<Canvas>(true).Last().rootCanvas;canvas.renderMode=RenderMode.WorldSpace;canvas.transform.position=Vector3.zero;canvas.transform.localScale=Vector3.one;
        var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;
        var camera=new GameObject("Statistics review camera",typeof(Camera)).GetComponent<Camera>();camera.transform.position=new Vector3(0,0,-20);camera.orthographic=true;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.white;
        foreach(bool tablet in new[]{false,true}){
            int width=tablet?1024:945,height=tablet?1366:2048;((RectTransform)canvas.transform).sizeDelta=new Vector2(800,height*800f/width);
            SelectScreen(menu.statisticsScreen,canvas);stats.ApplyGameStatistics(false);stats.ApplyGameStatistics(false);
            var root=((GameObject)Field(stats,"statisticsGameObject")).transform;var scroll=root.Find("Statistics scroll").GetComponent<ScrollRect>();StatisticsCards.Reflow(scroll.content);
            Check(scroll.content.Cast<Transform>().Count(t=>t.name=="Memory statistics"&&t.gameObject.activeSelf)==1,"Idempotent Memory section");
            var memory=scroll.content.Find("Memory statistics");var focus=scroll.content.Find("Focus statistics");
            Check(memory.Find("Game records").childCount==MemoryStatisticsPanel.Games(TrainingHubLayout.ShowFocus).Length,"Available Memory games");
            if(TrainingHubLayout.ShowFocus)Check(focus.Find("Game records").childCount==4,"Four Focus games");else Check(focus==null,"No Focus section in2.0");
            Check(memory.Find("Practice records/Memory Pairs practice record")!=null && memory.Find("Practice records/Focus Boxes practice record")==null,"Only Memory practice below Memory");
            if(focus!=null)Check(focus.Find("Practice records/Focus Boxes practice record")!=null && focus.Find("Practice records/Memory Pairs practice record")==null,"Only Focus practice below Focus");
            var host=((GameObject)Field(stats,"challengeStatsPanel")).transform;var formats=host.Find("Challenges statistics/Challenge format stats");
            Check(formats.childCount==5 && formats.Find("Matches/Value").GetComponent<Text>().text=="4" && formats.Find("Single/Value").GetComponent<Text>().text=="8","Actual five challenge counters");
            Check(formats.GetComponentsInChildren<Text>().All(t=>t.color==Color.white),"White Challenge type");
            foreach(var grid in scroll.GetComponentsInChildren<StatisticsCardGrid>()){grid.Refresh();var layout=grid.GetComponent<GridLayoutGroup>();Check(layout.cellSize.x*layout.constraintCount+layout.spacing.x*(layout.constraintCount-1)<=((RectTransform)grid.transform).rect.width+.1f,"Cards fit viewport width");}
            scroll.verticalNormalizedPosition=1;Capture(canvas,camera,"statistics-overview-"+(tablet?"ipad":"phone"),width,height);
            ScrollTo(scroll,memory);Capture(canvas,camera,"statistics-memory-"+(tablet?"ipad":"phone"),width,height);
            ScrollTo(scroll,host);Capture(canvas,camera,"statistics-challenges-"+(tablet?"ipad":"phone"),width,height);
            if(focus!=null){ScrollTo(scroll,focus);Capture(canvas,camera,"statistics-focus-"+(tablet?"ipad":"phone"),width,height);}
            stats.ApplyChallengeCounters(new UserStatistics{numberOfChallenges=12});formats=host.Find("Challenges statistics/Challenge format stats");Check(formats.Find("Matches/Value").GetComponent<Text>().text=="—","Old server formats must stay unknown");
            // New width must adjust column count and card width without rebuilding the section.
            var gridRect=(RectTransform)memory.Find("Game records");var gridSizer=gridRect.GetComponent<StatisticsCardGrid>();var gridLayout=gridRect.GetComponent<GridLayoutGroup>();
            SelectScreen(menu.tipsScreen,canvas);TipsEnhancer.Ensure(menu);TipsEnhancer.Ensure(menu);
            var list=menu.tipsScreen.transform.Find("Scroll View/Viewport/Content");var deck=list.Find("TheDeckGuide");var major=list.Find("MajorSystem");
            Check(deck.GetSiblingIndex()==major.GetSiblingIndex()+1 && list.Find("MemoryPalace").GetSiblingIndex()>deck.GetSiblingIndex(),"Deck immediately after Major and before Palace");
            Check(deck.GetComponent<LayoutElement>().preferredHeight==LayoutUtility.GetPreferredHeight((RectTransform)major) && ((RectTransform)deck).rect.height>180 && deck.GetComponent<Image>().color==Color.white,"Exact effective tip card height, visible and white");
            Check(deck.Find("Deck icon").GetComponentsInChildren<Image>().Length==3,"Existing CC0 card artwork");
            var tipScroll=list.GetComponentInParent<ScrollRect>();ScrollTo(tipScroll,major);Capture(canvas,camera,"tips-deck-list-"+(tablet?"ipad":"phone"),width,height);
            deck.GetComponent<Button>().onClick.Invoke();var page=menu.tipsScreen.transform.Find("Screens/DeckGuidePage");Check(page.gameObject.activeInHierarchy,"Deck opens from the list");
            var guideScroll=page.Find("Scroll").GetComponent<ScrollRect>();StatisticsCards.Reflow(guideScroll.content);page.GetComponentInChildren<ScrollAboveFooter>(true).Apply();guideScroll.verticalNormalizedPosition=1;
            Capture(canvas,camera,"tips-deck-guide-"+(tablet?"ipad":"phone"),width,height);
            int cards=0;foreach(string suit in new[]{"Clubs","Diamonds","Hearts","Spades"}){var toggle=guideScroll.content.Find("Expand "+suit).GetComponent<Button>();var group=guideScroll.content.Find("Group "+suit);if(!group.gameObject.activeSelf)toggle.onClick.Invoke();Check(group.childCount==13,"All thirteen "+suit);cards+=group.childCount;toggle.onClick.Invoke();}
            Check(cards==52 && page.GetComponentInChildren<TipReadBar>(true)!=null,"All 52 associations and existing read achievement");
            guideScroll.content.Find("Expand Diamonds").GetComponent<Button>().onClick.Invoke();ScrollTo(guideScroll,guideScroll.content.Find("Expand Diamonds"));Capture(canvas,camera,"tips-deck-diamonds-"+(tablet?"ipad":"phone"),width,height);
            page.Find("Back").GetComponent<Button>().onClick.Invoke();Check(!page.gameObject.activeSelf && menu.tipsScreen.activeSelf,"Original back returns to Tips");
        }
        UnityEngine.Object.DestroyImmediate(camera.gameObject);
        if(TrainingHubLayout.ShowFocus)typeof(StatisticsDeckValidation).Assembly.GetType("FocusStatisticsValidation").GetMethod("Review").Invoke(null,null);
        Debug.Log("STATISTICS_DECK_VALIDATION_PASSED: actual awards/account isolation, responsive shared cards, separate practice, five account counters, Deck placement/back/all52/read badge and phone/tablet renders");
    }finally{Clear();UserConstants.s_user=prior;LevelProgress.EnsureUser();}}
}
