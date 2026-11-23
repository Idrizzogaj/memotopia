using UnityEngine;
using UnityEngine.UI;

// Saga reuses Chain and keeps its own local progress, independent of Training.
public class SagaPage : MonoBehaviour
{
    private GameObject page;
    private Text selection;
    private SagaMapGraphic map;
    private ChainGame chain;
    private int owner;
    private readonly System.Collections.Generic.List<Text> status = new System.Collections.Generic.List<Text>();

    public static void Attach(MenuNavigation menu)
    {
        if (menu.homeScreen == null || menu.trainingBackground == null || menu.challengeBackground == null) return;
        if (menu.homeScreen.transform.Find("Memotopia Saga") != null) return;
        var controller = menu.gameObject.AddComponent<SagaPage>();
        var training = menu.trainingBackground.rectTransform;
        // All existing home buttons remain, including Statistics, Videos and Tips.
        // Only the three tall rows move to make room below the unchanged title.
        if(menu.statisticsBackground!=null)Place(menu.statisticsBackground.rectTransform,.578f,.734f);
        Place(training, .406f, .562f);
        Place(menu.challengeBackground.rectTransform, .234f, .390f);
        var tile = ChainGame.Panel(training.parent, "Memotopia Saga", new Vector2(.049f, .750f), new Vector2(.950f, .906f), ChainGame.Theme);
        tile.GetComponent<Image>().sprite = ContentSelectorBar.Rounded();
        tile.GetComponent<Image>().type = Image.Type.Sliced;
        tile.AddComponent<Button>().onClick.AddListener(() => controller.Open(menu));
        var symbol=ChainGame.Panel(tile.transform,"Saga neuron",new Vector2(.01f,.02f),new Vector2(.28f,.98f),Color.white).GetComponent<Image>();
        symbol.sprite=Resources.Load<Sprite>("Saga/Neuron");symbol.preserveAspect=true;symbol.raycastTarget=false;
        ChainGame.Label(tile.transform, "Memotopia Saga", new Vector2(.27f, .12f), new Vector2(.96f, .88f), 48, Color.white).fontStyle = FontStyle.Bold;
        var host=new GameObject("Saga Chain",typeof(RectTransform));
        host.transform.SetParent(menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas.transform,false);
        controller.chain=host.AddComponent<ChainGame>();
    }

    private static void Place(RectTransform rect, float bottom, float top)
    {
        rect.anchorMin = new Vector2(.049f, bottom);
        rect.anchorMax = new Vector2(.950f, top);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private void Open(MenuNavigation menu)
    {
        owner=GameManager.AchievementUserId;
        if (page != null) { ShowMap(); return; }
        var canvas = menu.homeScreen.GetComponentInParent<Canvas>().rootCanvas;
        var area = canvas.GetComponent<MobileSafeArea>();
        Transform parent = area != null && area.Content != null ? area.Content : canvas.transform;
        page = ChainGame.Panel(parent, "Saga map", Vector2.zero, Vector2.one, Color.white);
        var safe = (RectTransform)page.transform;
        ChainGame.Label(safe, "Memotopia Saga", new Vector2(.18f, .92f), new Vector2(.94f, .99f), 44, ChainGame.Theme).fontStyle = FontStyle.Bold;
        ChainGame.Button(safe, "Back", new Vector2(.03f, .925f), new Vector2(.19f, .985f), Close);
        selection = ChainGame.Label(safe, "CHAIN · Choose a level", new Vector2(.03f, .015f), new Vector2(.97f, .09f), 28, ChainGame.Theme);

        var scrollObject = ChainGame.Panel(safe, "Map scroll", new Vector2(.02f, .11f), new Vector2(.98f, .91f), Color.clear);
        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D));
        viewport.transform.SetParent(scrollObject.transform, false);
        var viewRect = (RectTransform)viewport.transform;
        viewRect.anchorMin = Vector2.zero; viewRect.anchorMax = Vector2.one;
        viewRect.offsetMin = viewRect.offsetMax = Vector2.zero;
        var content = new GameObject("Levels 1-20", typeof(RectTransform), typeof(CanvasRenderer), typeof(SagaMapGraphic));
        content.transform.SetParent(viewport.transform, false);
        var contentRect = (RectTransform)content.transform;
        contentRect.anchorMin = new Vector2(0, 1); contentRect.anchorMax = Vector2.one;
        contentRect.pivot = new Vector2(.5f, 1);
        Canvas.ForceUpdateCanvases();
        float width = Mathf.Max(320, viewRect.rect.width);
        contentRect.sizeDelta = new Vector2(0, width * 2.8f);
        map = content.GetComponent<SagaMapGraphic>(); map.raycastTarget = false;
        var neuron=Resources.Load<Sprite>("Saga/Neuron");
        for (int i = 0; i < SagaMapGraphic.Points.Length; i++)
        {
            int level = i + 1;
            var point = SagaMapGraphic.Points[i];
            var node = ChainGame.Panel(contentRect, "Saga level " + level, point, point, Color.white);
            var rect = (RectTransform)node.transform;
            rect.sizeDelta = new Vector2(width * .28f, width * .28f);
            var image=node.GetComponent<Image>();image.sprite=neuron;image.preserveAspect=true;
            var button = node.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => SelectLevel(level));
            ChainGame.Label(rect, level.ToString(), new Vector2(.32f,.32f), new Vector2(.68f,.68f), 36, Color.white).fontStyle = FontStyle.Bold;
            status.Add(ChainGame.Label(rect, "",new Vector2(.10f,-.12f),new Vector2(.90f,.10f),22,ChainGame.Purple));
        }
        var scroll = scrollObject.AddComponent<ScrollRect>();
        scroll.viewport = viewRect; scroll.content = contentRect;
        scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40;
        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 0;
        RefreshProgress();
    }

    private void SelectLevel(int level)
    {
        map.SelectedLevel = level;
        map.SetVerticesDirty();
        if(!SagaProgress.Unlocked(level) || chain.IsSagaOpen)return;
        page.SetActive(false);
        chain.OpenSaga(level,ShowMap);
        if(!chain.IsSagaOpen)ShowMap();
    }

    private void ShowMap()
    {
        if(page==null || owner!=GameManager.AchievementUserId)return;
        page.SetActive(true);page.transform.SetAsLastSibling();RefreshProgress();
    }
    private void RefreshProgress()
    {
        for(int i=0;i<status.Count;i++){
            int stars=SagaProgress.Stars(i+1);
            status[i].text=stars>0?new string('★',stars):"";
            status[i].transform.parent.GetComponent<Button>().interactable=SagaProgress.Unlocked(i+1);
        }
        selection.text="CHAIN · Choose a level";
    }

    private void Close() { if (page != null) page.SetActive(false); }
    private void Update() {
        if(page==null || !page.activeSelf)return;
        if(owner!=GameManager.AchievementUserId || Input.GetKeyDown(KeyCode.Escape))Close();
    }
    private void OnDestroy() { if (page != null) Destroy(page); }
}
