using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class VideoTipsValidation
{
    private static void Require(bool valid, string message)
    { if (!valid) throw new Exception("Video/tips: " + message); }

    private static float Bottom(RectTransform rect)
    { var c = new Vector3[4]; rect.GetWorldCorners(c); return c[0].y; }
    private static float Top(RectTransform rect)
    { var c = new Vector3[4]; rect.GetWorldCorners(c); return c[1].y; }

    public static void Run()
    {
        try
        {
            var scene = EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
            var menu = Resources.FindObjectsOfTypeAll<MenuNavigation>().First(x => x.gameObject.scene == scene);
            var canvas = menu.videoScreen.GetComponentInParent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) scaler.enabled = false;
            canvas.transform.localScale = Vector3.one;
            var canvasRect = (RectTransform)canvas.transform;
            canvasRect.sizeDelta = new Vector2(800, 1422);
            canvasRect.ForceUpdateRectTransforms();
            VideosEnhancer.Ensure(menu);
            TipsEnhancer.Ensure(menu);
            VideosEnhancer.Ensure(menu);
            TipsEnhancer.Ensure(menu);
            var content = menu.videoScreen.transform.Find("Scroll View/Viewport/Content") as RectTransform;
            string[] order = { "IntroVideo", "ExtraVideo0", "ExtraVideo1", "ExtraVideo2", "ExtraVideo3", "LinkingMethodVideo", "AssociationsVideo", "MemoryPalaceVideo" };
            Require(content.childCount == 8, "eight rows, no duplicates on reopening");
            for (int i = 0; i < order.Length; i++)
            {
                var row = (RectTransform)content.GetChild(i);
                Require(row.name == order[i], "row order " + i);
                Require(Mathf.Abs(row.anchoredPosition.y - (-80 - 200 * i)) < .1f, "row placement " + i);
            }
            var atBottom = typeof(TipReadBar).GetMethod("AtBottom", BindingFlags.NonPublic | BindingFlags.Static);
            foreach (float height in new[] { 1200f, 1422f, 1778f })
            {
                canvasRect.sizeDelta = new Vector2(800, height);
                menu.videoScreen.SetActive(true);
                Canvas.ForceUpdateCanvases();
                var videoScroll = menu.videoScreen.GetComponentInChildren<ScrollRect>(true);
                videoScroll.verticalNormalizedPosition = 0;
                Canvas.ForceUpdateCanvases();
                var last = (RectTransform)content.Find("MemoryPalaceVideo");
                Require(Bottom(last) - Bottom(videoScroll.viewport) >= 170, "last video bottom clearance at " + height);
                Require(Top(last) <= Top(videoScroll.viewport) + 1, "last video fully inside viewport");
                menu.videoScreen.SetActive(false);
                menu.tipsScreen.SetActive(true);
                int pages = 0;
                foreach (Transform page in menu.tipsScreen.transform.Find("Screens"))
                {
                    var bar = page.Find("ReadBar") as RectTransform;
                    if (bar == null) continue;
                    pages++;
                    page.gameObject.SetActive(true);
                    Canvas.ForceUpdateCanvases();
                    var scroll = page.GetComponentInChildren<ScrollRect>(true);
                    var clearance = scroll.GetComponent<ScrollAboveFooter>();
                    Require(clearance != null, "footer configured: " + page.name);
                    clearance.Apply();
                    Canvas.ForceUpdateCanvases();
                    float firstBottom = Bottom(scroll.viewport);
                    clearance.Apply();
                    Require(Mathf.Abs(firstBottom - Bottom(scroll.viewport)) < .1f, "clearance must not accumulate");
                    scroll.verticalNormalizedPosition = 1;
                    Canvas.ForceUpdateCanvases();
                    if (scroll.content.rect.height > scroll.viewport.rect.height + 1)
                        Require(!(bool)atBottom.Invoke(null, new object[] { scroll }), "read gate at top: " + page.name);
                    scroll.verticalNormalizedPosition = 0;
                    Canvas.ForceUpdateCanvases();
                    Require(Bottom(scroll.viewport) >= Top(bar) + 23, "viewport above bar: " + page.name);
                    Require(Bottom(scroll.content) >= Top(bar) + 23, "text end above bar: " + page.name);
                    foreach (var text in scroll.content.GetComponentsInChildren<Text>(true))
                        Require(Bottom(text.rectTransform) >= Top(bar) + 20, "last text visible: " + page.name + "/" + text.name);
                    Require((bool)atBottom.Invoke(null, new object[] { scroll }), "read gate at end: " + page.name);
                    page.gameObject.SetActive(false);
                }
                Require(pages == 12, "all twelve tips checked");
                menu.tipsScreen.SetActive(false);
            }
            Debug.Log("VIDEO_TIPS_VALIDATION_PASSED: eight ordered videos, bottom clearance, twelve tips at three portrait sizes, repeated setup, read gate.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
}
