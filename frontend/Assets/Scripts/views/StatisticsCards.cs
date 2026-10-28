using UnityEngine;
using UnityEngine.UI;

// The Focus statistics layout, shared verbatim by all three statistics sections.
public static class StatisticsCards
{
    public static readonly Color Paper = new Color32(241, 248, 248, 255);
    public static Text Label(Transform parent, string text, Vector2 min, Vector2 max, int size, Color color)
    {
        var label = ChainGame.Label(parent, text, min, max, size, color);
        label.fontStyle = FontStyle.Bold;
        return label;
    }
    public static GameObject Block(Transform parent, string name, float height, Color color)
    {
        var block = ChainGame.Panel(parent, name, Vector2.zero, Vector2.one, color);
        block.GetComponent<Image>().raycastTarget = false;
        block.AddComponent<LayoutElement>().preferredHeight = height;
        return block;
    }
    public static GameObject Section(Transform parent, string name, string title, Color tone)
    {
        var old = parent.Find(name);
        if (old != null) { old.name += " retired"; old.gameObject.SetActive(false); CardFaces.DestroyGenerated(old.gameObject); }
        var root = ChainGame.Panel(parent, name, Vector2.zero, Vector2.one, Paper);
        root.GetComponent<Image>().raycastTarget = false;
        SprintUi.Edges(root.GetComponent<Image>());
        var layout = root.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 20, 20); layout.spacing = 14;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        var heading = Block(root.transform, "Heading", 58, tone);
        Label(heading.transform, title, Vector2.zero, Vector2.one, 34, Color.white);
        return root;
    }
    public static Transform Grid(Transform parent, string name, float cellHeight = 286)
    {
        var root = ChainGame.Panel(parent, name, Vector2.zero, Vector2.one, Color.clear);
        root.GetComponent<Image>().raycastTarget = false;
        var grid = root.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.spacing = new Vector2(14, 14);
        var responsive = root.AddComponent<StatisticsCardGrid>(); responsive.CellHeight = cellHeight;
        Canvas.ForceUpdateCanvases(); responsive.Refresh();
        return root.transform;
    }
    public static GameObject Card(Transform parent, string title, Color tone, int kind = -1)
    {
        var card = ChainGame.Panel(parent, title, Vector2.zero, Vector2.one, tone);
        card.GetComponent<Image>().raycastTarget = false;
        SprintUi.Edges(card.GetComponent<Image>());
        Label(card.transform, title.ToUpperInvariant(), new Vector2(.06f, .77f), new Vector2(.69f, .96f), 30, Color.white);
        if (kind >= 0)
        {
            var icon = (RectTransform)ChainGame.Panel(card.transform, "Existing game symbol", new Vector2(.83f, .85f), new Vector2(.83f, .85f), Color.clear).transform;
            icon.GetComponent<Image>().raycastTarget = false; icon.sizeDelta = new Vector2(50, 50);
            Canvas.ForceUpdateCanvases();
            if (kind < 4) TrainingGameSymbol.Draw(icon, kind, Color.white);
            else
            {
                var image = ChainGame.Panel(icon, "Existing elephant", Vector2.zero, Vector2.one, Color.white).GetComponent<Image>();
                image.sprite = Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01"); image.preserveAspect = true; image.raycastTarget = false;
            }
        }
        return card;
    }
    public static void Reflow(Transform root)
    {
        Canvas.ForceUpdateCanvases();
        foreach (var grid in root.GetComponentsInChildren<StatisticsCardGrid>(true)) grid.Refresh();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root);
    }
}

// Width belongs to the scroll viewport, not Screen.width (important on iPad/safe areas).
public sealed class StatisticsCardGrid : MonoBehaviour
{
    public float CellHeight = 286;
    float previousWidth = -1;
    public void Refresh()
    {
        var grid = GetComponent<GridLayoutGroup>();
        float width = ((RectTransform)transform).rect.width;
        if (grid == null || width <= 0 || Mathf.Abs(width - previousWidth) < .5f) return;
        previousWidth = width;
        grid.constraintCount = width >= 620 ? 2 : 1;
        grid.cellSize = new Vector2((width - (grid.constraintCount - 1) * 14) / grid.constraintCount, CellHeight);
    }
    void LateUpdate() { Refresh(); }
}
