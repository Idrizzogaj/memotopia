using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Account deletion required by App Store and Google Play. The whole UI is built in code and attached
/// to the account screen, so no scene or prefab edits are needed. Sizes derive from the canvas width.
/// </summary>
public class DeleteAccountPanel : MonoBehaviour
{
    UserAPIController _userApi;
    MenuNavigation _menu;
    GameObject _overlay;
    InputField _password;
    Text _status;
    Button _confirmButton;
    Font _font;
    float _unit;
    bool _busy;

    public void Build(MenuNavigation menu, GameObject accountScreen)
    {
        if (accountScreen == null || accountScreen.transform.Find("DeleteAccountLink") != null) return;
        _menu = menu;
        _userApi = gameObject.GetComponent<UserAPIController>();
        if (_userApi == null) _userApi = gameObject.AddComponent<UserAPIController>();
        #if UNITY_2022_2_OR_NEWER
        _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif

        var canvas = accountScreen.GetComponentInParent<Canvas>();
        var root = canvas != null ? canvas.rootCanvas.transform as RectTransform : accountScreen.transform as RectTransform;
        _unit = Mathf.Max(300f, root.rect.width);

        BuildLink(accountScreen.transform as RectTransform);
        BuildOverlay(root);
    }

    void BuildLink(RectTransform parent)
    {
        var go = new GameObject("DeleteAccountLink", typeof(RectTransform), typeof(Text), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(_unit * 0.6f, _unit * 0.1f);
        rt.anchoredPosition = new Vector2(0f, _unit * 0.03f);
        var text = go.GetComponent<Text>();
        Style(text, "Delete account", Mathf.RoundToInt(_unit * 0.034f), new Color(0.85f, 0.2f, 0.2f), TextAnchor.MiddleCenter);
        text.fontStyle = FontStyle.Italic;
        go.GetComponent<Button>().targetGraphic = text;
        go.GetComponent<Button>().onClick.AddListener(Open);
    }

    void BuildOverlay(RectTransform root)
    {
        _overlay = new GameObject("DeleteAccountOverlay", typeof(RectTransform), typeof(Image));
        var ort = _overlay.GetComponent<RectTransform>();
        ort.SetParent(root, false);
        Stretch(ort);
        _overlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.88f);

        var card = new GameObject("Card", typeof(RectTransform), typeof(Image));
        var crt = card.GetComponent<RectTransform>();
        crt.SetParent(ort, false);
        crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(_unit * 0.88f, _unit * 0.95f);
        card.GetComponent<Image>().color = Color.white;

        float w = _unit * 0.88f, h = _unit * 0.95f, pad = _unit * 0.04f, y = h / 2f - pad;
        y = AddText(crt, "Delete your account?", Mathf.RoundToInt(_unit * 0.05f), Color.black, FontStyle.Bold, w - pad * 2, _unit * 0.09f, y);
        y = AddText(crt, "This permanently deletes your account, your progress, achievements and challenge history. It cannot be undone. A subscription is not cancelled by this - cancel it in your App Store / Google Play subscriptions.",
            Mathf.RoundToInt(_unit * 0.032f), new Color(0.25f, 0.25f, 0.25f), FontStyle.Normal, w - pad * 2, _unit * 0.3f, y);

        _password = BuildInput(crt, "Password (leave empty if you use Apple login)", w - pad * 2, _unit * 0.1f, y);
        y -= _unit * 0.12f;
        _status = AddTextRef(crt, "", Mathf.RoundToInt(_unit * 0.03f), new Color(0.8f, 0.1f, 0.1f), w - pad * 2, _unit * 0.08f, ref y);

        _confirmButton = AddButton(crt, "Delete permanently", new Color(0.8f, 0.15f, 0.15f), Color.white, w - pad * 2, _unit * 0.11f, -h / 2f + pad + _unit * 0.13f, Confirm);
        AddButton(crt, "Cancel", new Color(0.9f, 0.9f, 0.9f), Color.black, w - pad * 2, _unit * 0.11f, -h / 2f + pad, Close);

        _overlay.SetActive(false);
    }

    void Open()
    {
        _password.text = "";
        _status.text = "";
        _busy = false;
        _confirmButton.interactable = true;
        _overlay.transform.SetAsLastSibling();
        _overlay.SetActive(true);
    }

    void Close()
    {
        if (_busy) return;
        _overlay.SetActive(false);
    }

    void Confirm()
    {
        if (_busy) return;
        _busy = true;
        _confirmButton.interactable = false;
        _status.color = new Color(0.25f, 0.25f, 0.25f);
        _status.text = "Deleting...";
        _userApi.DeleteAccount(_password.text,
            () =>
            {
                _busy = false;
                _overlay.SetActive(false);
                UserConstants.s_user = null;
                _menu.Logout();
            },
            failure =>
            {
                _busy = false;
                _confirmButton.interactable = true;
                _status.color = new Color(0.8f, 0.1f, 0.1f);
                if (failure != null && failure.ErrorMessage == ErrorMessages.s_invalidCredentials)
                    _status.text = "Wrong password.";
                else if (failure != null && failure.ErrorMessage == ErrorMessages.s_tooManyRequestsCode)
                    _status.text = ErrorMessages.s_tooManyRequestsMessage;
                else
                    _status.text = "Could not delete the account. Check your connection and try again.";
            });
    }

    #region UI helpers
    void Style(Text t, string value, int size, Color color, TextAnchor anchor)
    {
        t.font = _font;
        t.text = value;
        t.fontSize = size;
        t.color = color;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    float AddText(RectTransform parent, string value, int size, Color color, FontStyle style, float width, float height, float y)
    {
        var t = AddTextRef(parent, value, size, color, width, height, ref y);
        t.fontStyle = style;
        return y;
    }

    Text AddTextRef(RectTransform parent, string value, int size, Color color, float width, float height, ref float y)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, y);
        var t = go.GetComponent<Text>();
        Style(t, value, size, color, TextAnchor.UpperCenter);
        y -= height;
        return t;
    }

    InputField BuildInput(RectTransform parent, string placeholder, float width, float height, float y)
    {
        var go = new GameObject("PasswordInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, y);
        go.GetComponent<Image>().color = new Color(0.94f, 0.94f, 0.94f);

        int size = Mathf.RoundToInt(_unit * 0.034f);
        var text = ChildText(rt, "Text", "", size, Color.black, FontStyle.Normal);
        var ph = ChildText(rt, "Placeholder", placeholder, Mathf.RoundToInt(size * 0.85f), new Color(0.5f, 0.5f, 0.5f), FontStyle.Italic);
        var input = go.GetComponent<InputField>();
        input.textComponent = text;
        input.placeholder = ph;
        input.contentType = InputField.ContentType.Password;
        input.lineType = InputField.LineType.SingleLine;
        input.targetGraphic = go.GetComponent<Image>();
        return input;
    }

    Text ChildText(RectTransform parent, string name, string value, int size, Color color, FontStyle style)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        Stretch(rt);
        rt.offsetMin = new Vector2(_unit * 0.025f, 0f);
        rt.offsetMax = new Vector2(-_unit * 0.025f, 0f);
        var t = go.GetComponent<Text>();
        Style(t, value, size, color, TextAnchor.MiddleLeft);
        t.fontStyle = style;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        return t;
    }

    Button AddButton(RectTransform parent, string label, Color back, Color fore, float width, float height, float y, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, y);
        var img = go.GetComponent<Image>();
        img.color = back;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);
        var text = ChildText(rt, "Label", label, Mathf.RoundToInt(_unit * 0.04f), fore, FontStyle.Bold);
        text.alignment = TextAnchor.MiddleCenter;
        return btn;
    }
    #endregion
}
