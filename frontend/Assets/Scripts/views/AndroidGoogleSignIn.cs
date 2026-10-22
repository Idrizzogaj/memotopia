using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Scripting;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models.ResponseModels;

public class AndroidGoogleSignIn : MonoBehaviour
{
    [Serializable] private class Providers { public string googleWebClientId; }
    [Serializable] private class TokenRequest { public string idToken; }
    [Serializable] private class ApiError { public string message; }
    private AuthView view;
    private Button button;
    private Text status;
    private string clientId;
    private bool busy;
    private float deadline;
    private int generation;
    private readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
#if UNITY_ANDROID && !UNITY_EDITOR
    private CredentialCallback callback;
#endif
    private string AuthBase { get { return new APIConstants().SocialLogInRoute.Replace("/social-signin", ""); } }

    private IEnumerator Start()
    {
        view = GetComponent<AuthView>();
#if !UNITY_ANDROID || UNITY_EDITOR
        yield break;
#else
        using (var request = UnityWebRequest.Get(AuthBase + "/providers"))
        {
            request.timeout = 8;
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success) yield break;
            Providers config = null;
            try { config = JsonUtility.FromJson<Providers>(request.downloadHandler.text); } catch (Exception) { }
            if (config == null || string.IsNullOrEmpty(config.googleWebClientId) || !config.googleWebClientId.EndsWith(".apps.googleusercontent.com")) yield break;
            clientId = config.googleWebClientId;
        }
        CreateButton();
#endif
    }

    private void CreateButton()
    {
        var panel = view.AndroidSocialLogin;
        if (panel == null) return;
        // Do not revive any legacy Facebook controls in this container.
        foreach (Transform child in panel.transform) child.gameObject.SetActive(false);
        panel.SetActive(true);
        var root = new GameObject("GoogleSignIn", typeof(RectTransform), typeof(Image), typeof(Button));
        root.transform.SetParent(panel.transform, false);
        var rect = (RectTransform)root.transform;
        rect.anchorMin = new Vector2(.05f, .15f); rect.anchorMax = new Vector2(.95f, .85f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        root.GetComponent<Image>().color = Color.white;
        button = root.GetComponent<Button>();
        button.onClick.AddListener(SignIn);
        var label = new GameObject("Label", typeof(RectTransform), typeof(Text));
        label.transform.SetParent(root.transform, false);
        var labelRect = (RectTransform)label.transform;
        labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(8, 0); labelRect.offsetMax = new Vector2(-8, 0);
        status = label.GetComponent<Text>();
        status.font = view.usernameLogIn.textComponent.font;
        status.fontSize = 26; status.resizeTextForBestFit = true; status.resizeTextMinSize = 16; status.resizeTextMaxSize = 26;
        status.alignment = TextAnchor.MiddleCenter; status.color = new Color(.12f, .12f, .12f);
        var artwork = Resources.Load<Texture2D>("Auth/GoogleSignInButton");
        if (artwork != null)
        {
            var image = root.GetComponent<Image>();
            image.sprite = Sprite.Create(artwork, new Rect(0, 0, artwork.width, artwork.height), new Vector2(.5f, .5f));
            image.preserveAspect = true;
            status.text = ""; // Official Google artwork contains the label and logo.
        }
        else { root.SetActive(false); }
        status.raycastTarget = false;
    }

    public void SignIn()
    {
        if (busy || string.IsNullOrEmpty(clientId)) return;
        busy = true; deadline = Time.realtimeSinceStartup + 120;
        int run = ++generation;
        button.interactable = false;
        view.errorLogin.SetActive(false);
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            callback = new CredentialCallback(this, run);
            using (var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var bridge = new AndroidJavaClass("com.zogajretention.auth.GoogleSignInBridge"))
                bridge.CallStatic("signIn", activity, clientId, callback);
        }
        catch (Exception) { Complete("Google sign-in could not start. Please use email or try again."); }
#else
        Complete("Google sign-in is available on Android.");
#endif
    }

    private IEnumerator Exchange(string token, int run)
    {
        if (!busy || run != generation) yield break;
        var loading = FindObjectOfType<LoadingManager>();
        if (loading != null) loading.StartLoading();
        using (var request = new UnityWebRequest(AuthBase + "/google", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(new TokenRequest { idToken = token })));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;
            yield return request.SendWebRequest();
            if (!busy || run != generation) yield break;
            if (!request.isNetworkError && !request.isHttpError)
            {
                LogInResponsePayload result = null;
                try { result = JsonUtility.FromJson<LogInResponsePayload>(request.downloadHandler.text); } catch (Exception) { }
                if (result != null && result.user != null && result.user.ID > 0 && !string.IsNullOrEmpty(result.accessToken))
                {
                    UserConstants.s_user = result.user;
                    GetComponent<DataController>().Token = "Bearer " + result.accessToken;
                    Complete(null);
                    LoadingManager.Navigate(SceneName.s_gameMenu);
                    yield break;
                }
            }
            string message = "Google sign-in failed. Please try again or sign in with email.";
            if (request.responseCode == 409) message = "This email already has a Memotopia account. Use your existing sign-in method.";
            Complete(message);
        }
    }

    private void Update()
    {
        Action action;
        while (callbacks.TryDequeue(out action)) action();
        if (busy && Time.realtimeSinceStartup > deadline) Complete("Sign-in timed out. Please try again.");
    }
    private void Complete(string error)
    {
        busy = false; generation++;
        if (button != null) button.interactable = true;
        var loading = FindObjectOfType<LoadingManager>();
        if (loading != null) loading.EndLoading();
        CancelNative();
        if (!string.IsNullOrEmpty(error) && view != null)
        {
            view.errorLogin.GetComponent<Text>().text = error;
            view.errorLogin.SetActive(true);
        }
    }
    private void OnDestroy() { busy = false; generation++; CancelNative(); }
    private void CancelNative()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try { using (var bridge = new AndroidJavaClass("com.zogajretention.auth.GoogleSignInBridge")) bridge.CallStatic("cancel"); } catch (Exception) { }
        callback = null;
#endif
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    [Preserve]
    private class CredentialCallback : AndroidJavaProxy
    {
        private readonly AndroidGoogleSignIn owner;
        private readonly int run;
        public CredentialCallback(AndroidGoogleSignIn owner, int run) : base("com.zogajretention.auth.GoogleSignInBridge$Callback") { this.owner = owner; this.run = run; }
        [Preserve] public void onSuccess(string token)
        {
            owner.callbacks.Enqueue(() => { if (owner != null && owner.busy && run == owner.generation) owner.StartCoroutine(owner.Exchange(token, run)); });
        }
        [Preserve] public void onError(string code)
        {
            owner.callbacks.Enqueue(() => { if (owner != null && owner.busy && run == owner.generation) owner.Complete(code == "cancelled" ? null : "Google sign-in is unavailable. Check your connection or use email."); });
        }
    }
#endif
}