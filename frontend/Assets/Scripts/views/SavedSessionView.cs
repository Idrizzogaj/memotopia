using System;
using UnityEngine;
using UnityEngine.UI;

// A local presentation of session restoration, never a substitute for server authentication.
public sealed class SavedSessionView : MonoBehaviour
{
    private Text status;
    private Button retry, signIn;

    public static SavedSessionView Show(Transform parent, Action onRetry, Action onSignIn)
    {
        // Reuse the existing opaque loading screen and animated Memotopia logo.
        var root = Instantiate(Resources.Load<GameObject>("Prefabs/LoadingScreen"), parent, false);
        root.name = "Restoring saved sign-in";
        var rect = (RectTransform)root.transform;
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        root.SetActive(true);
        var view = root.AddComponent<SavedSessionView>();
        view.status = ChainGame.Label(root.transform, "", new Vector2(.12f, .29f), new Vector2(.88f, .38f), 28, ChainGame.Ink);
        view.retry = SprintUi.Button(root.transform, "Retry", new Vector2(.18f, .18f), new Vector2(.82f, .25f), () => onRetry());
        view.signIn = SprintUi.Button(root.transform, "Sign in", new Vector2(.18f, .08f), new Vector2(.82f, .15f), () => onSignIn());
        view.Pending();
        return view;
    }

    public void Pending()
    {
        status.text = "Connecting…";
        retry.gameObject.SetActive(false);
        signIn.gameObject.SetActive(false);
    }

    public void RetryAvailable()
    {
        status.text = "Your sign-in is saved.\nWaiting for a connection…";
        retry.gameObject.SetActive(true);
        signIn.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (Application.isPlaying) Destroy(gameObject); else DestroyImmediate(gameObject);
    }
}
