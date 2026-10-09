using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>Opens the phone's own share sheet with a piece of text (WhatsApp, SMS, mail ...).</summary>
public static class NativeShare
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void _MemotopiaShareText(string text);
#endif

    public static void Share(string text, string chooserTitle)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = new AndroidJavaObject("android.content.Intent", "android.intent.action.SEND"))
            {
                intent.Call<AndroidJavaObject>("setType", "text/plain");
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", text);
                using (var intentClass = new AndroidJavaClass("android.content.Intent"))
                using (var chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intent, chooserTitle))
                    activity.Call("startActivity", chooser);
            }
        }
        catch (System.Exception e) { Debug.LogWarning("Share failed: " + e.Message); }
#elif UNITY_IOS && !UNITY_EDITOR
        _MemotopiaShareText(text);
#else
        Debug.Log("Share (editor): " + text);
#endif
    }
}
