using UnityEngine;
using UnityEngine.UI;

/// <summary>Uses the existing Settings entry; does not modify scenes or account data.</summary>
public class MemorySettingsPanel : MonoBehaviour
{
    private GameObject overlay;
    private RectTransform content;
    private Font font;
    private const string Marker = "MemorySettingsEntry";
    public static void Ensure(MenuNavigation menu)
    {
        if (menu == null || menu.sideMenu == null) return;
        var host=menu.GetComponent<MemorySettingsPanel>() ?? menu.gameObject.AddComponent<MemorySettingsPanel>();
        foreach (var entry in menu.sideMenu.GetComponentsInChildren<RectTransform>(true))
        {
            if (entry.name != "Settings") continue;
            var image=entry.GetComponent<Image>() ?? entry.gameObject.AddComponent<Image>();
            image.color=Color.clear;
            var b=entry.GetComponent<Button>() ?? entry.gameObject.AddComponent<Button>();
            b.targetGraphic=image;
            b.onClick = new Button.ButtonClickedEvent();
            b.onClick.AddListener(host.Open);
            if (b.transform.Find(Marker)==null)
            {
                var marker=new GameObject(Marker);
                marker.transform.SetParent(b.transform,false);
            }
            entry.gameObject.SetActive(true);
        }
    }
    public static bool IsEntry(Button button) { return button.transform.Find(Marker)!=null; }

    public void Open()
    {
        if (overlay!=null) { overlay.SetActive(true); overlay.transform.SetAsLastSibling(); Rebuild(); return; }
#if UNITY_2022_2_OR_NEWER
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        font=Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        var canvas=GetComponent<MenuNavigation>().sideMenu.GetComponentInParent<Canvas>().rootCanvas;
        overlay=new GameObject("MemorySettings",typeof(RectTransform),typeof(Image));
        overlay.transform.SetParent(canvas.transform,false);
        Stretch((RectTransform)overlay.transform);
        overlay.GetComponent<Image>().color=Color.white;
        var heading=Label(overlay.transform,"Settings",54,90);
        var hr=heading.rectTransform; hr.anchorMin=new Vector2(.1f,.87f); hr.anchorMax=new Vector2(.9f,.96f); hr.offsetMin=hr.offsetMax=Vector2.zero;
        var close=Button(overlay.transform,"Done",()=> { overlay.SetActive(false); RefreshBars(); },72);
        var br=(RectTransform)close.transform; br.anchorMin=new Vector2(.2f,.03f); br.anchorMax=new Vector2(.8f,.11f); br.offsetMin=br.offsetMax=Vector2.zero;
        var scrollGo=new GameObject("Scroll",typeof(RectTransform),typeof(Image),typeof(ScrollRect));
        scrollGo.transform.SetParent(overlay.transform,false);
        var sr=(RectTransform)scrollGo.transform; sr.anchorMin=new Vector2(.06f,.14f); sr.anchorMax=new Vector2(.94f,.86f); sr.offsetMin=sr.offsetMax=Vector2.zero;
        scrollGo.GetComponent<Image>().color=Color.clear;
        var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(RectMask2D)); viewport.transform.SetParent(sr,false); Stretch((RectTransform)viewport.transform);
        var body=new GameObject("Content",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(ContentSizeFitter)); body.transform.SetParent(viewport.transform,false);
        content=(RectTransform)body.transform; content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1); content.sizeDelta=Vector2.zero;
        var layout=body.GetComponent<VerticalLayoutGroup>(); layout.spacing=18;layout.childControlWidth=layout.childControlHeight=true;layout.childForceExpandHeight=false; layout.padding=new RectOffset(12,12,12,32);
        body.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        var scroll=scrollGo.GetComponent<ScrollRect>();scroll.viewport=(RectTransform)viewport.transform;scroll.content=content;scroll.horizontal=false;scroll.scrollSensitivity=35;
        Rebuild();
    }

    private void Rebuild()
    {
        for (int i=content.childCount-1;i>=0;i--) { var old=content.GetChild(i).gameObject;old.SetActive(false);Destroy(old); }
        var practice=Button(content,(MemorySettings.Practice?"[x] ":"[ ] ")+"Practice",()=>{MemorySettings.Practice=!MemorySettings.Practice;Rebuild();},86);
        practice.image.color=MemorySettings.Practice?(Color)new Color32(254,191,75,255):(Color)new Color32(238,234,255,255);
        Label(content,"Practice counts time upwards and saves Level + Time. No achievements. Challenges always stay timed.",28,110);
        Label(content,"Major System",40,62);
        Button(content,(MemorySettings.Digits==2?"[x] ":"[ ] ")+"2 numbers (00-99)",()=>SetDigits(2),82);
        var three=Button(content,(MemorySettings.Digits==3?"[x] ":"[ ] ")+"3 numbers (000-999)",()=>SetDigits(3),82);
        three.interactable=CardFaces.AdvancedUnlocked;
        Label(content,CardFaces.AdvancedUnlocked ? "Three-digit mode uses words and numbers. The 100 picture associations stay at 00-99." : "Words, numbers and three-digit mode unlock with the Memory Athlete badge, as in Training.",28,145);
        Button(content,(MemorySettings.ShowImageNumbers?"[x] ":"[ ] ")+"Show numbers on pictures",()=>{MemorySettings.ShowImageNumbers=!MemorySettings.ShowImageNumbers;Rebuild();},86);
        Button(content,(MemorySettings.ShowWordNumbers?"[x] ":"[ ] ")+"Show word codes ("+(MemorySettings.Digits==3?"000-999":"00-99")+")",()=>{MemorySettings.ShowWordNumbers=!MemorySettings.ShowWordNumbers;Rebuild();},86);
        Label(content,"Training content",40,64);
        Label(content,"These choices also update the buttons in Training. Changes apply to the next round.",28,100);
        var choices=new[]{CardFaces.Choice.Images,CardFaces.Choice.Words,CardFaces.Choice.Numbers,CardFaces.Choice.Random,CardFaces.Choice.Deck};
        foreach (var choice in choices)
        {
            var captured=choice;
            string name=choice==CardFaces.Choice.Deck?"The Deck (52 cards)":choice.ToString();
            var button=Button(content,((CardFaces.Selected&choice)!=0?"[x] ":"[ ] ")+name,()=>{SelectContent(captured);Rebuild();RefreshBars();},76);
            button.interactable=(choice==CardFaces.Choice.Images || choice==CardFaces.Choice.Deck || CardFaces.AdvancedUnlocked) && !(MemorySettings.Digits==3 && choice==CardFaces.Choice.Images);
        }
        Label(content,"000-999: English starter pegs, including short phrases for difficult sound combinations. The Deck uses its own 52 card associations.",26,145);
        Button(content,"Open-source credits",Credits,72);
    }
    private void SelectContent(CardFaces.Choice choice)
    {
        var current=CardFaces.Selected;
        if(choice==CardFaces.Choice.Deck) { CardFaces.Selected=choice;return; }
        if(choice==CardFaces.Choice.Random)
        {
            CardFaces.Selected=(current&choice)!=0 ? CardFaces.Choice.Images : (current&CardFaces.Choice.Help)|choice;
            return;
        }
        current &= ~(CardFaces.Choice.Deck|CardFaces.Choice.Random);
        CardFaces.Selected=current^choice;
    }
    private void Credits()
    {
        if(content.Find("LicenseCredits")!=null) return;
        foreach (var path in new[]{"Licenses/CMUDICT-LICENSE","Licenses/WORDNET-LICENSE","Licenses/CLASSIC-DECK-CC0"})
        {
            var asset=Resources.Load<TextAsset>(path);
            if (asset==null) continue;
            var text=Label(content,asset.text,22,100);
            if(content.Find("LicenseCredits")==null) text.name="LicenseCredits";
            text.gameObject.GetComponent<LayoutElement>().preferredHeight=-1;
        }
        Canvas.ForceUpdateCanvases();
    }
    private void SetDigits(int digits) { MemorySettings.Digits=digits;CardFaces.Selected=CardFaces.Selected;Rebuild();RefreshBars(); }
    private void RefreshBars() { foreach(var bar in FindObjectsOfType<ContentSelectorBar>()) bar.RefreshFromSettings(); }
    private Text Label(Transform parent,string value,int size,float height)
    {
        var go=new GameObject("Label",typeof(RectTransform),typeof(Text),typeof(LayoutElement));go.transform.SetParent(parent,false);
        var t=go.GetComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=new Color32(43,43,58,255);t.alignment=TextAnchor.MiddleLeft;t.raycastTarget=false;
        go.GetComponent<LayoutElement>().preferredHeight=height;return t;
    }
    private Button Button(Transform parent,string value,UnityEngine.Events.UnityAction action,float height)
    {
        var go=new GameObject(value,typeof(RectTransform),typeof(Image),typeof(Button),typeof(LayoutElement));go.transform.SetParent(parent,false);
        go.GetComponent<Image>().color=new Color32(238,234,255,255);go.GetComponent<LayoutElement>().preferredHeight=height;
        var b=go.GetComponent<Button>();b.onClick.AddListener(action);
        var t=Label(go.transform,value,32,height); Stretch(t.rectTransform); t.rectTransform.offsetMin=new Vector2(18,4);t.rectTransform.offsetMax=new Vector2(-18,-4);
        t.resizeTextForBestFit=true;t.resizeTextMinSize=24;t.resizeTextMaxSize=32;
        return b;
    }
    private static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero; }
}
