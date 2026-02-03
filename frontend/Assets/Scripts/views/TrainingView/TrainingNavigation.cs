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

    public void GoToLevels(string game)
    {
        dataController.MenuLastPage = NavigationConstants.s_trainingNav;
        StaticVar.s_game = game;

        SetGameLevelsConstants(StaticVar.s_gameBoxes);
        SetGameLevelsConstants(StaticVar.s_gameFlash);
        SetGameLevelsConstants(StaticVar.s_gamePairs);

        
    }

    public void SetGameLevelsConstants(string game)
    {
        try
        {
            gameApiController.GetLevels(game,
                (OnSuccess) =>
                {
                    var responseObject = OnSuccess;

                    if (responseObject.levels.Length > -1)
                    {
                        switch (game)
                        {
                            case StaticVar.s_gamePairs:
                                GameLevelConstants.s_pairsLevels = responseObject.levels;
                                break;
                            case StaticVar.s_gameBoxes:
                                GameLevelConstants.s_boxesLevels = responseObject.levels;
                                break;
                            case StaticVar.s_gameFlash:
                                GameLevelConstants.s_flashLevels = responseObject.levels;
                                break;
                        }
                    }

                    if (GameLevelConstants.s_pairsLevels != null && GameLevelConstants.s_boxesLevels != null && GameLevelConstants.s_flashLevels != null)
                    {
                        goToScene(SceneName.s_levelsScene);
                    }
                },
                (OnFailure) =>
                {
                    print("fail");
                }
            );
        }
        catch (UserException e)
        {
            Debug.Log(e.Message);
        }
    }

    public static void goToScene(string sceneName) {
        SceneManager.LoadScene(sceneName);
    }
}
