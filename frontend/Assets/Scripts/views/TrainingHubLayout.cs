using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shared, flat visual language for Memory and the forthcoming Focus games.
public static class TrainingHubLayout
{
    public static void Arrange(RectTransform screen,List<RectTransform> memory,float top)
    {
        foreach(var label in new[]{"MEMORY section","Memory section","FOCUS section","Focus PAIRS","Focus BOXES","Focus FLASH","Focus CHAIN"}) {
            var prior=screen.Find(label);if(prior!=null)CardFaces.DestroyGenerated(prior.gameObject);
        }
        float width=screen.rect.width*.90f, gap=width*.04f;
        float side=Mathf.Min((width-gap)/2f,(top-screen.rect.yMin-390f-3*gap)/4f);
        float start=top-238f;
        Header(screen,"Memory",top,width);
        for(int i=0;i<memory.Count;i++) Tile(memory[i],i,false,screen,side,gap,start);
        float focusTop=start-2*(side+gap)-12f;
        Header(screen,"FOCUS",focusTop,width);
        for(int i=0;i<4;i++) {
            var go=ChainGame.Panel(screen,"Focus "+Names[i],Vector2.zero,Vector2.one,Color.white);
            var button=go.AddComponent<Button>();button.interactable=false;button.transition=Selectable.Transition.None;
            Tile((RectTransform)go.transform,i,true,screen,side,gap,focusTop-66f);
        }
    }
    private static readonly string[] Names={"PAIRS","BOXES","FLASH","CHAIN"};
    private static readonly Color[] GameTones={new Color32(36,189,224,255),new Color32(234,24,100,255),new Color32(254,191,75,255),new Color32(123,31,255,255)};
    public static readonly Color MemoryTone=new Color32(106,28,225,255), FocusTone=new Color32(0,112,122,255);
    private static void Header(RectTransform screen,string caption,float top,float width) {
        var go=ChainGame.Panel(screen,caption+" section",Vector2.zero,Vector2.one,caption=="FOCUS"?FocusTone:MemoryTone);
        Place((RectTransform)go.transform,screen,0,top,width,44);
        ChainGame.Label(go.transform,caption.ToUpperInvariant(),Vector2.zero,Vector2.one,30,Color.white).fontStyle=FontStyle.Bold;
    }
    private static void Tile(RectTransform tile,int index,bool focus,RectTransform screen,float side,float gap,float top) {
        foreach(var old in tile.GetComponentsInChildren<Graphic>(true))if(old.transform!=tile)old.enabled=false;
        Place(tile,screen,(index%2==0?-1:1)*(side+gap)/2,top-(index/2)*(side+gap),side,side);
        var back=tile.GetComponent<Image>();back.enabled=true;back.sprite=ContentSelectorBar.Rounded();back.type=Image.Type.Sliced;back.color=focus?FocusTone:GameTones[index];back.canvasRenderer.SetColor(Color.white);var selectable=tile.GetComponent<Button>();if(focus && selectable!=null){var colors=selectable.colors;colors.disabledColor=Color.white;selectable.colors=colors;selectable.transition=Selectable.Transition.None;}
        var border=tile.GetComponent<Outline>();if(border==null)border=tile.gameObject.AddComponent<Outline>();border.effectColor=Color.clear;border.effectDistance=new Vector2(2,-2);
        var art=new GameObject("Flat game symbol",typeof(RectTransform));art.transform.SetParent(tile,false);var ar=(RectTransform)art.transform;ar.anchorMin=new Vector2(.22f,.35f);ar.anchorMax=new Vector2(.78f,.87f);ar.offsetMin=ar.offsetMax=Vector2.zero;
        Canvas.ForceUpdateCanvases();TrainingGameSymbol.Draw((RectTransform)art.transform,index,Color.white,focus);
        var label=ChainGame.Label(tile,Names[index],new Vector2(.05f,.10f),new Vector2(.95f,.32f),46,Color.white);label.fontStyle=FontStyle.Bold;
        if(focus)ChainGame.Label(tile,"90 s • Coming soon",new Vector2(.04f,.02f),new Vector2(.96f,.12f),20,Color.white);
    }
    public static void ApplyChallenge(ChallengeNavigation navigation) {
        if(navigation==null)return;
        var chain=navigation.challengeGameScreen.transform.Find("Chain challenge");
        if(chain==null){var clone=UnityEngine.Object.Instantiate(navigation.flashImage.gameObject,navigation.flashImage.transform.parent,false);clone.name="Chain challenge";chain=clone.transform;foreach(Transform child in chain){child.name="Retired "+child.name;child.gameObject.SetActive(false);CardFaces.DestroyGenerated(child.gameObject);}}
        var images=new[]{navigation.pairsImage,navigation.boxesImage,navigation.flashImage,chain.GetComponent<Image>()};
        for(int i=0;i<images.Length;i++) {
            var back=images[i];if(back==null || back.transform.Find("Approved game icon")!=null)continue;
            foreach(var old in back.GetComponentsInChildren<Graphic>(true))if(old!=back)old.enabled=false;
            back.sprite=ContentSelectorBar.Rounded();back.type=Image.Type.Sliced;back.color=GameTones[i];
            // The old full-card Button must not get a last-sibling TouchTarget over the tier buttons.
            var parentButton=back.GetComponent<Button>();
            if(parentButton!=null){
                parentButton.onClick=new Button.ButtonClickedEvent();parentButton.enabled=false;
                var oldHit=parentButton.transform.Find("Touch area");if(oldHit!=null)oldHit.gameObject.SetActive(false);
            }
            var tile=back.rectTransform;
            Canvas.ForceUpdateCanvases();
            float height=Mathf.Min(tile.rect.height*.62f,tile.rect.width*.27f);
            var art=new GameObject("Approved game icon",typeof(RectTransform));var ar=(RectTransform)art.transform;ar.SetParent(tile,false);
            ar.anchorMin=ar.anchorMax=new Vector2(.77f,.53f);ar.sizeDelta=new Vector2(height*.56f/.52f,height);
            TrainingGameSymbol.Draw(ar,i,Color.white);
            var text=ChainGame.Label(tile,Names[i],new Vector2(.07f,.34f),new Vector2(.59f,.66f),60,Color.white);
            text.fontStyle=FontStyle.Bold;text.alignment=TextAnchor.MiddleLeft;
            string game=new[]{"Pairs","Boxes","Flash","Chain"}[i];
            Corner(tile,navigation,game,"Easy",6,new Vector2(.58f,.72f),new Vector2(.97f,.98f));
            Corner(tile,navigation,game,"Medium",12,new Vector2(.56f,.02f),new Vector2(.97f,.28f));
            Corner(tile,navigation,game,"Hard",18,new Vector2(.03f,.02f),new Vector2(.42f,.28f));
            var athlete=Corner(tile,navigation,game,"",MemoryAthlete.Level,new Vector2(.03f,.72f),new Vector2(.30f,.98f));
            var logo=ChainGame.Panel(athlete.transform,"Existing elephant logo",new Vector2(.15f,.1f),new Vector2(.85f,.9f),Color.white).GetComponent<Image>();
            logo.sprite=Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01");logo.preserveAspect=true;logo.raycastTarget=false;logo.color=i==2?Color.black:Color.white;
        }
        for(int i=0;i<images.Length;i++){var r=images[i].rectTransform;r.anchorMin=new Vector2(.05f,.64f-i*.20f);r.anchorMax=new Vector2(.95f,.82f-i*.20f);r.offsetMin=r.offsetMax=Vector2.zero;}
        SprintLobby.Attach(navigation);
        foreach(var text in navigation.challengeGameScreen.GetComponentsInChildren<Text>(true))if(text.text=="Choose game")text.text="Choose game and level";
    }

    private static Button Corner(RectTransform tile,ChallengeNavigation navigation,string game,string caption,int level,Vector2 min,Vector2 max){
        var area=ChainGame.Panel(tile,caption==""?"Memory Athlete":caption,min,max,new Color(1,1,1,.16f));
        var image=area.GetComponent<Image>();image.sprite=ContentSelectorBar.Rounded();image.type=Image.Type.Sliced;
        var button=area.AddComponent<Button>();button.onClick.AddListener(()=>navigation.ChooseGameAndLevel(game,level));
        var label=ChainGame.Label(area.transform,caption,new Vector2(.02f,.02f),new Vector2(.98f,.98f),32,Color.white);label.fontStyle=FontStyle.Bold;
        return button;
    }

    private static void Place(RectTransform rect,RectTransform screen,float x,float top,float width,float height) {
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);
        rect.localScale=Vector3.one;rect.localRotation=Quaternion.identity;rect.sizeDelta=new Vector2(width,height);rect.anchoredPosition3D=new Vector3(x,top-screen.rect.yMax,0);
    }
}

public static class TrainingGameSymbol
{
    public static void Draw(RectTransform parent,int kind,Color color,bool preserveFocus=false) {
        // Focus keeps its original placeholders while Memory and Challenge share the revised symbols.
        if(!preserveFocus && kind==2){
            var bolt=new GameObject("Compact lightning",typeof(RectTransform),typeof(CanvasRenderer),typeof(TrainingFlashSymbol));
            var rect=(RectTransform)bolt.transform;rect.SetParent(parent,false);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            var graphic=bolt.GetComponent<TrainingFlashSymbol>();graphic.color=color;graphic.raycastTarget=false;return;
        }
        if(!preserveFocus && kind==1){
            // A closed lid keeps the silhouette below the Easy corner without moving any controls.
            var points=new[]{new Vector2(.2f,.29f),new Vector2(.5f,.13f),new Vector2(.8f,.29f),new Vector2(.8f,.60f),new Vector2(.5f,.75f),new Vector2(.2f,.60f),new Vector2(.2f,.29f)};
            for(int i=1;i<points.Length;i++)Line(parent,points[i-1],points[i],color);
            Line(parent,new Vector2(.2f,.60f),new Vector2(.5f,.44f),color);Line(parent,new Vector2(.8f,.60f),new Vector2(.5f,.44f),color);Line(parent,new Vector2(.5f,.44f),new Vector2(.5f,.13f),color);return;
        }
        if(kind==2) {Line(parent,new Vector2(.63f,.94f),new Vector2(.26f,.48f),color);Line(parent,new Vector2(.26f,.48f),new Vector2(.70f,.48f),color);Line(parent,new Vector2(.70f,.48f),new Vector2(.38f,.06f),color);return;}
        if(kind==1) {
            var points=new[]{new Vector2(.2f,.25f),new Vector2(.5f,.07f),new Vector2(.8f,.25f),new Vector2(.8f,.65f),new Vector2(.5f,.48f),new Vector2(.2f,.65f),new Vector2(.2f,.25f)};
            for(int i=1;i<points.Length;i++)Line(parent,points[i-1],points[i],color);
            Line(parent,new Vector2(.5f,.07f),new Vector2(.5f,.48f),color);Line(parent,new Vector2(.2f,.65f),new Vector2(.37f,.90f),color);Line(parent,new Vector2(.8f,.65f),new Vector2(.63f,.90f),color);return;
        }
        int count=kind==3?3:2;
        for(int i=0;i<count;i++) {float x=count==3?.20f+i*.30f:.28f+i*.44f;float radius=count==3?.20f:.25f;
            for(int k=0;k<6;k++){float a=k*Mathf.PI/3,b=(k+1)*Mathf.PI/3;Line(parent,new Vector2(x,.5f)+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,new Vector2(x,.5f)+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*radius,color);}
        }
    }
    private static void Line(RectTransform parent,Vector2 a,Vector2 b,Color color) {
        var line=new GameObject("Symbol stroke",typeof(RectTransform),typeof(Image));var r=(RectTransform)line.transform;r.SetParent(parent,false);
        r.anchorMin=r.anchorMax=(a+b)*.5f;r.pivot=new Vector2(.5f,.5f);
        Vector2 delta=new Vector2((b.x-a.x)*parent.rect.width,(b.y-a.y)*parent.rect.height);
        r.sizeDelta=new Vector2(delta.magnitude+3f,Mathf.Max(4f,parent.rect.width*.055f));r.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
        var image=line.GetComponent<Image>();image.sprite=null;image.type=Image.Type.Simple;image.color=color;image.raycastTarget=false;
    }
}
