using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Assets.Script.Constants;
using Assets.Script.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class AchievementValidation
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new Exception("Achievement validation: " + message);
    }

    public static void Validate()
    {
        var first19 = Enumerable.Range(1, 19).Select(n => new GameLevel { level = n }).ToArray();
        Require(AchievementRules.Eligible("Boxes", 20, first19, null, null, 399, 0).Contains("boxes-all-levels"), "20th level before server refresh");
        Require(AchievementRules.Eligible("Boxes", 20, first19.Concat(new[] { new GameLevel { level = 20 } }).ToArray(), null, null, 0, 0).Contains("boxes-all-levels"), "20th level after server refresh");
        Require(!AchievementRules.Eligible("Boxes", 20, first19.Where(x => x.level != 4).Concat(new[] { new GameLevel { level = 1 } }).ToArray(), null, null, 0, 0).Contains("boxes-all-levels"), "duplicates must not replace a missing level");
        var one = new[] { new GameLevel { level = 1 } };
        var keys = AchievementRules.Eligible("Flash", 1, one, one, null, 400, 11);
        Require(keys.Count(k => k == "looking-around") == 1 && keys.Contains("unlock-challenge") && keys.Contains("first-challenge") && keys.Contains("win-ten-challenge"), "thresholds and unique awards");
        Require(!AchievementRules.Eligible(null, 0, null, null, null, 399, 0).Any(), "empty progress must not unlock anything");
        Require(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset") != null, "essential font must exist");
        Debug.Log("Achievement rules and font validation passed.");
    }

    public static void ReviewAndBuild()
    {
        Review();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MemotopiaBuild.BuildIOS();
    }

    public static void Review()
    {
        Validate();
        var originalUser = UserConstants.s_user;
        const int testUser = 2147483600;
        PlayerPrefs.DeleteKey("achievements.v1." + testUser);
        PlayerPrefs.DeleteKey("achievements.v1." + (testUser - 1));
        try
        {
            UserConstants.s_user = new User { id = testUser };
            Require(GameManager.UnlockAchievement("pairing-up"), "first unlock");
            Require(!GameManager.UnlockAchievement("pairing-up"), "duplicate unlock");
            GameManager.completedAchievements = new List<string>();
            Require(GameManager.completedAchievements.Contains("pairing-up") && GameManager.PendingAchievements.Contains("pairing-up"), "stale response must retain pending award");
            UserConstants.s_user = new User { id = testUser - 1 };
            Require(GameManager.completedAchievements.Count == 0, "account isolation");
            UserConstants.s_user = new User { id = testUser };
            Require(GameManager.PendingAchievements.Contains("pairing-up"), "pending survives reload");
            GameManager.ConfirmAchievement("pairing-up");
            Require(GameManager.PendingAchievements.Count == 0, "confirmation clears retry queue");
            EditorSceneManager.OpenScene("Assets/_Scenes/GameMenu.unity");
            var view = Resources.FindObjectsOfTypeAll<AchievementsView>().First(v => v.gameObject.scene.IsValid());
            var canvas = view.GetComponentInParent<Canvas>();
            Require(canvas != null, "menu canvas");
            foreach (Transform child in canvas.transform) child.gameObject.SetActive(false);
            for (Transform t = view.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
            view.achievementPopupView.SetActive(false);
            view.Refresh();
            Require(view.content.transform.Cast<Transform>().Count(t => t.gameObject.activeSelf) == 12, "12 visible rows");
            GameManager.completedAchievements = new List<string> { "in-the-box", "unknown-legacy-award" };
            view.Refresh();
            Require(view.content.transform.Cast<Transform>().Count(t => t.gameObject.activeSelf) == 13, "refresh with unknown legacy award");
            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null) scaler.enabled = false;
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.transform.position = Vector3.zero;
            canvas.transform.localScale = Vector3.one;
            canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 1733);
            var cameraObject = new GameObject("Achievement review camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 866.5f;
            camera.transform.position = new Vector3(0, 0, -100);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.white;
            camera.cullingMask = 1 << 30;
            canvas.worldCamera = camera;
            var target = new RenderTexture(800, 1733, 24); camera.targetTexture = target;
            Directory.CreateDirectory("../docs/achievements/review");
            Save(canvas, camera, target, "list");
            var longest = view.achievements.First(a => a.constantString == "boxes-all-levels");
            view.OnAchievementClick(longest);
            Require(view.popupDescription.text == longest.description && view.popupIcon.material != view.popupIcon.defaultMaterial, "locked details");
            Save(canvas, camera, target, "details-locked");
            GameManager.completedAchievements = new List<string> { "boxes-all-levels" };
            view.Refresh();
            Require(view.popupDescription.text == longest.description && view.popupIcon.material == view.popupIcon.defaultMaterial, "details refresh");
            Save(canvas, camera, target, "details-unlocked");
            var gestures = view.achievementPopupView.GetComponent<AchievementDetailGestures>();
            var pointer = new UnityEngine.EventSystems.PointerEventData(null);
            pointer.pressPosition = new Vector2(600, 600); pointer.position = new Vector2(0, 610);
            gestures.OnPointerDown(pointer); gestures.OnBeginDrag(pointer); gestures.OnEndDrag(pointer);
            Require(view.popupTitle.text != longest.title, "left swipe advances achievement");
            pointer.pressPosition = new Vector2(0, 600); pointer.position = new Vector2(600, 610);
            gestures.OnPointerDown(pointer); gestures.OnBeginDrag(pointer); gestures.OnEndDrag(pointer);
            Require(view.popupTitle.text == longest.title, "right swipe restores previous achievement");
            pointer.position = new Vector2(-10000, -10000);
            gestures.OnPointerDown(pointer); gestures.OnPointerClick(pointer);
            Require(!view.achievementPopupView.activeSelf, "outside tap closes details after swiping");
            view.OnClosePopupClick();
            var popup = UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Prefabs/GamePopUps/AchievementPanel"), canvas.transform, false);
            popup.GetComponent<AchievementPopupView>().Bind(longest, view.achievements);
            Save(canvas, camera, target, "earned");
            UnityEngine.Object.DestroyImmediate(popup);
            UnityEngine.Object.DestroyImmediate(cameraObject);
            UnityEngine.Object.DestroyImmediate(target);
            Debug.Log("Achievement state, account isolation, retry, list, detail and popup checks passed. Reviews saved.");
        }
        finally
        {
            UserConstants.s_user = originalUser;
            PlayerPrefs.DeleteKey("achievements.v1." + testUser);
            PlayerPrefs.DeleteKey("achievements.v1." + (testUser - 1));
            PlayerPrefs.Save();
        }
    }

    private static void Save(Canvas canvas, Camera camera, RenderTexture target, string name)
    {
        foreach (var transform in canvas.GetComponentsInChildren<Transform>(true)) transform.gameObject.layer = 30;
        Canvas.ForceUpdateCanvases();
        foreach (var rect in canvas.GetComponentsInChildren<RectTransform>()) LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        Canvas.ForceUpdateCanvases();
        foreach (var text in canvas.GetComponentsInChildren<TextMeshProUGUI>())
        {
            text.ForceMeshUpdate();
            if (!string.IsNullOrWhiteSpace(text.text)) Require(text.font != null && text.textInfo.characterCount > 0, "renderable text: " + text.name);
        }
        camera.Render();
        var previous = RenderTexture.active; RenderTexture.active = target;
        var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
        File.WriteAllBytes("../docs/achievements/review/" + name + ".png", image.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(image); RenderTexture.active = previous;
    }
}
