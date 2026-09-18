using System;
using Assets.Script.Constants;
using Assets.Script.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TrainingNavigation : MonoBehaviour
{
    public GameAPIController gameApiController;
    public DataController dataController;

    public Image pairsImage;
    public Image flashImage;
    public Image boxesImage;

    // Start is called before the first frame update
    void Start()
    {
        // For Ipad
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            pairsImage.sprite = Resources.Load<Sprite>("GamesResources/Training/Mask Group 57");
            flashImage.sprite = Resources.Load<Sprite>("GamesResources/Training/Mask Group 55");
            boxesImage.sprite = Resources.Load<Sprite>("GamesResources/Training/Mask Group 56");

            pairsImage.rectTransform.anchorMin = new Vector2((float)0.5, (float)0.5);
            pairsImage.rectTransform.anchorMax = new Vector2((float)0.5, (float)0.5);
            flashImage.rectTransform.anchorMin = new Vector2((float)0.5, (float)0.5);
            flashImage.rectTransform.anchorMax = new Vector2((float)0.5, (float)0.5);
            boxesImage.rectTransform.anchorMin = new Vector2((float)0.5, (float)0.5);
            boxesImage.rectTransform.anchorMax = new Vector2((float)0.5, (float)0.5);

            pairsImage.rectTransform.sizeDelta = new Vector2(720, 285);
            flashImage.rectTransform.sizeDelta = new Vector2(720, 285);
            boxesImage.rectTransform.sizeDelta = new Vector2(720, 285);
        }

        gameApiController = gameObject.AddComponent<GameAPIController>();
        dataController = gameObject.AddComponent<DataController>();
    }

    private bool opening;
    private Text connectionMessage;

    public void GoToLevels(string game)
    {
        if (opening) return;
        opening = true;
        if (connectionMessage != null) connectionMessage.gameObject.SetActive(false);
        dataController.MenuLastPage = NavigationConstants.s_trainingNav;
        StaticVar.s_game = game;
        var sync = ProgressSync.Instance;
        if (LevelProgress.Get(game) == null) ShowConnectionMessage("Loading levels…");
        sync.GetLevels(game, success =>
        {
            if (this == null) return;
            if (success) LoadingManager.Navigate(SceneName.s_levelsScene);
            else
            {
                opening = false;
                ShowConnectionMessage();
            }
        });
        foreach (var other in new[] { "Boxes", "Pairs", "Flash" })
            if (other != game) sync.GetLevels(other, null);
    }

    private void ShowConnectionMessage(string message = "Could not load levels. Check your connection and tap the game to retry.")
    {
        if (connectionMessage == null)
        {
            var obj = new GameObject("Connection message", typeof(RectTransform), typeof(Text));
            obj.transform.SetParent(pairsImage.canvas.transform, false);
            connectionMessage = obj.GetComponent<Text>();
            connectionMessage.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            connectionMessage.fontSize = 24; connectionMessage.color = new Color(.5f, .1f, .1f);
            connectionMessage.alignment = TextAnchor.MiddleCenter;
            connectionMessage.raycastTarget = false;
            var rect = connectionMessage.rectTransform;
            rect.anchorMin = new Vector2(.05f, .02f); rect.anchorMax = new Vector2(.95f, .10f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
        connectionMessage.text = message;
        connectionMessage.gameObject.SetActive(true);
    }

    public void SetGameLevelsConstants(string game) { ProgressSync.Instance.GetLevels(game, null); }
    public static void goToScene(string sceneName) { LoadingManager.Navigate(sceneName); }
}
