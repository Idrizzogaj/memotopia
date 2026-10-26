using System;
using System.Linq;
using System.Reflection;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using Assets.Script.Models;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class StartupSessionValidation
{
    const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
    static object Get(object o, string name) { return o.GetType().GetField(name, Flags).GetValue(o); }
    static void Call(object o, string name, params object[] args) { o.GetType().GetMethod(name, Flags).Invoke(o, args); }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }

    public static void Review()
    {
        bool hadToken = PlayerPrefs.HasKey("token");
        string token = PlayerPrefs.GetString("token");
        var user = UserConstants.s_user;
        const string fake = "session-validation-only-not-a-credential";
        try
        {
            PlayerPrefs.SetString("token", fake);
            EditorSceneManager.OpenScene("Assets/_Scenes/LoginScene.unity");
            var auth = UnityEngine.Object.FindObjectOfType<AuthView>();
            Call(auth, "Awake");
            Check(!auth.logInScreen.activeSelf && !auth.signUpScreen.activeSelf && !auth.forgotPasswordScreen.activeSelf,
                "All credential forms hidden before first frame with saved session");
            var view = (SavedSessionView)Get(auth, "savedSessionView");
            Check(view != null && view.gameObject.activeInHierarchy, "Existing loading presentation visible");
            Check(view.GetComponentsInChildren<Image>().Any(i => i.sprite == Resources.Load<Sprite>("GamesResources/Loading/Untitled-3-01")), "Reuse Memotopia logo");
            Capture(auth, "saved-session-connecting-phone", 945, 2048);

            Call(auth, "RestoreFailed", fake, new AlertMessageContainer { StatusCode = "RETRY" });
            Check(PlayerPrefs.GetString("token") == fake && !auth.logInScreen.activeSelf, "Network errors keep saved sign-in and hide login form");
            Check((bool)Get(auth, "retrySavedSession"), "Retry scheduled automatically");
            Check(view.GetComponentsInChildren<Button>().Any(b => b.name == "Retry"), "Visible Retry, no need to restart app");
            Capture(auth, "saved-session-retry-phone", 945, 2048);
            Capture(auth, "saved-session-retry-tablet", 1024, 1366);

            var api = auth.gameObject.AddComponent<UserAPIController>();
            int successes = 0, failures = 0;
            string error = null;
            Action<User> success = value => successes++;
            Action<AlertMessageContainer> failure = value => { failures++; error = value.StatusCode; };
            Action<long, bool, string> response = (code, failed, body) => Call(api, "ApplyUserResponse", fake, code, failed, body, success, failure);
            response(200, false, "{\"id\":1999999916,\"username\":\"Session review\"}");
            Check(successes == 1 && UserConstants.s_user.ID == 1999999916, "Valid session installs the authenticated profile");
            response(200, false, "{}"); response(200, false, "not JSON"); response(503, true, ""); response(0, true, "");
            Check(failures == 4 && error == "RETRY" && PlayerPrefs.GetString("token") == fake, "Malformed/server/network failures never delete credentials");
            response(401, true, ""); Check(error == "401", "Actual unauthorized response requires sign-in");

            PlayerPrefs.SetString("token", fake + "-other-account");
            response(401, true, ""); response(200, false, "{\"id\":1,\"username\":\"Old account\"}");
            Call(auth, "RestoreFailed", fake, new AlertMessageContainer { StatusCode = "401" });
            Check(successes == 1 && failures == 5 && UserConstants.s_user.ID == 1999999916 && PlayerPrefs.GetString("token").EndsWith("-other-account"), "Late responses cannot restore or log out another account");

            PlayerPrefs.SetString("token", fake);
            Call(auth, "RestoreFailed", fake, new AlertMessageContainer { StatusCode = "401" });
            Check(!PlayerPrefs.HasKey("token") && auth.logInScreen.activeSelf && UserConstants.s_user == null && !(bool)Get(auth, "retrySavedSession"), "Expired/revoked session cleanly opens login and stops retries");
            Call(auth, "Awake");
            Check(Get(auth, "savedSessionView") == null && auth.logInScreen.activeSelf, "Fresh install/explicit logout goes directly to login");
            PlayerPrefs.SetString("token", fake); Call(auth, "ShowSavedSession");
            Call(auth, "UseAnotherAccount");
            Check(!PlayerPrefs.HasKey("token") && auth.logInScreen.activeSelf, "Account switch deliberately clears saved session");
            Debug.Log("STARTUP_SESSION_VALIDATION_PASSED: before-frame hiding, native logo, retained network session, retry, valid/invalid response, logout and stale-account guards");
        }
        finally
        {
            if (hadToken) PlayerPrefs.SetString("token", token); else PlayerPrefs.DeleteKey("token");
            PlayerPrefs.Save(); UserConstants.s_user = user;
        }
    }

    static void Capture(AuthView auth, string name, int width, int height)
    {
        var canvas = auth.logInScreen.GetComponentInParent<Canvas>(true).rootCanvas;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.position = Vector3.zero; canvas.transform.localScale = Vector3.one;
        ((RectTransform)canvas.transform).sizeDelta = new Vector2(800, height * 800f / width);
        canvas.GetComponent<CanvasScaler>().enabled = false;
        var root = new GameObject("Session preview camera", typeof(Camera)); var cam = root.GetComponent<Camera>();
        cam.orthographic = true; cam.transform.position = new Vector3(0, 0, -20); cam.nearClipPlane = .1f; cam.farClipPlane = 1000;
        cam.cullingMask = 1 << 30; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.white;
        try { typeof(ChainReuseValidation).GetMethod("Capture", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { canvas.gameObject, cam, name, width, height }); }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

}
