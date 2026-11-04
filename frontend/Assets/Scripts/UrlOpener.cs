using UnityEngine;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

/// <summary>Opens a link in the phone's browser or store. iOS uses the current native call (see MemotopiaOpenUrl.m).</summary>
public static class UrlOpener
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void Memotopia_OpenURL(string url);
#endif

    public static void Open(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        Debug.Log("UrlOpener: opening " + url);
#if UNITY_IOS && !UNITY_EDITOR
        Memotopia_OpenURL(url);
#else
        Application.OpenURL(url);
#endif
    }
}