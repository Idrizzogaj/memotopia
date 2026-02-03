using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Script.Controllers;
using Assets.Script.Constants;

public class LevelScript : MonoBehaviour
{
    public Image BackgroundImage;
    public Image TopGameName;
    public Image[] LevelsCell;

    public enum Games
    {
        Flash,
        Boxes,
        Pairs
    }
    public Games game;
    public static int _selectedLevel;

    byte r, g, b, c;
    Sprite CellImage;
    Sprite OpenCellImage;
    GameObject CurrentObject;
    LoadingManager _loadingScreen;

    void Start()
    {
        _loadingScreen = GameObject.Find("LoadingCanvas").GetComponent<LoadingManager>();
        _loadingScreen.EndLoadingWithDelay();
        InitGame();
        OpenCellImage = Resources.Load<Sprite>("GamesResources/Levels/AAA-11") as Sprite;
        switch (game)
        {
            case Games.Boxes:
                CellImage = Resources.Load<Sprite>("GamesResources/Levels/BoxesLevelCell") as Sprite;
                BackgroundImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Levels\\BoxesBackground") as Sprite;
                TopGameName.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Levels\\BoxesTop") as Sprite;
                r = 234;
                g = 24;
                b = 100;
                c = 255;

                break;

            case Games.Pairs:
                CellImage = Resources.Load<Sprite>("GamesResources/Levels/PairsLevelCell") as Sprite;
                BackgroundImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Levels\\PairsBackground") as Sprite;
                TopGameName.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Levels\\PairsTop") as Sprite;
                r = 36;
                g = 189;
                b = 224;
                c = 255;

                break;

            case Games.Flash:
                CellImage = Resources.Load<Sprite>("GamesResources/Levels/FlashLevelCell") as Sprite;
                BackgroundImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Levels\\FlashBackground") as Sprite;
                TopGameName.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources\\Levels\\FlashTop") as Sprite;
                r = 254;
                g = 191;
                b = 75;
                c = 255;

                break;
        }

        for (int i = 0; i < LevelsCell.Length; i++)
        {
            if (game == Games.Pairs && i <= GameLevelConstants.s_pairsLevels.Length)
            {             
                LevelsCell[i].GetComponent<Image>().sprite = OpenCellImage;
                LevelsCell[i].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () { StartGame(); });

                if(i != GameLevelConstants.s_pairsLevels.Length)
                    InitStars(LevelsCell[i].transform.GetChild(0), GameLevelConstants.s_pairsLevels[i].stars);
                else
                    LevelsCell[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else if (game == Games.Boxes && i <= GameLevelConstants.s_boxesLevels.Length)
            {
                LevelsCell[i].GetComponent<Image>().sprite = OpenCellImage;
                LevelsCell[i].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () { StartGame(); });

                if(i != GameLevelConstants.s_boxesLevels.Length)
                    InitStars(LevelsCell[i].transform.GetChild(0), GameLevelConstants.s_boxesLevels[i].stars);
                else
                    LevelsCell[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else if(game == Games.Flash && i <= GameLevelConstants.s_flashLevels.Length)
            {
                LevelsCell[i].GetComponent<Image>().sprite = OpenCellImage;
                LevelsCell[i].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () { StartGame(); });

                if(i != GameLevelConstants.s_flashLevels.Length)
                    InitStars(LevelsCell[i].transform.GetChild(0), GameLevelConstants.s_flashLevels[i].stars);
                else
                    LevelsCell[i].transform.GetChild(0).gameObject.SetActive(false);
            }
            else if(i == 0)
            {
                LevelsCell[i].GetComponent<Image>().sprite = OpenCellImage;
                LevelsCell[i].transform.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate () { StartGame(); });
            }
            else
            {
                LevelsCell[i].GetComponent<Image>().sprite = CellImage;
                LevelsCell[i].transform.GetChild(0).gameObject.SetActive(false);
            }


            LevelsCell[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().color = new Color32(r, g, b, c);
            LevelsCell[i].transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = (i + 1).ToString();
        }
    }

    public void StartGame()
    {
        CurrentObject = EventSystem.current.currentSelectedGameObject;
        _selectedLevel = int.Parse(CurrentObject.transform.GetChild(0).GetComponent<Text>().text);

        switch (game)
        {
            case Games.Boxes:
                if((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
                {
                    SceneManager.LoadScene(SceneName.s_boxesSceneIpad);
                }
                else
                {
                    SceneManager.LoadScene(SceneName.s_boxesScene);
                }

                break;

            case Games.Pairs:
                SceneManager.LoadScene(SceneName.s_pairsScene);

                break;

            case Games.Flash:
                SceneManager.LoadScene(SceneName.s_flashScene);

                break;
        }
    }

    public static void ChallengeStartGame(string gameName, int level)
    {
        _selectedLevel = level;
        switch (gameName)
        {
            case "Boxes":
                if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
                {
                    SceneManager.LoadScene(SceneName.s_boxesSceneIpad);
                }
                else
                {
                    SceneManager.LoadScene(SceneName.s_boxesScene);
                }
                break;
            case "Pairs":
                SceneManager.LoadScene(SceneName.s_pairsScene);
                break;
            case "Flash":
                SceneManager.LoadScene(SceneName.s_flashScene);
                break;
        }
    }

    public void InitGame()
    {
        switch (StaticVar.s_game)
        {
            case "Flash":
                game = Games.Flash;
                break;
            case "Paris":
                game = Games.Pairs;
                break;
            case "Boxes":
                game = Games.Boxes;
                break;
        }
    }

    public void LoadGameMenu()
    {
        //SceneManager.LoadScene("GameMenu");
        _loadingScreen.GoToSceneWithLoadingInstantly(SceneName.s_gameMenu);
    }

    public void InitStars(Transform StarsCell, int starsNr)
    {
        for (int j = 0 ; j < starsNr ; j++)
        {
            string starImage = "";

            if(j == 0){
                starImage = "GamesResources\\Levels\\Asset 15";
            } else if (j == 1) {
                starImage = "GamesResources\\Levels\\Asset 14";
            } else if (j == 2) {
                starImage = "GamesResources\\Levels\\Asset 13";
            }

            StarsCell.transform.GetChild(j).GetComponent<Image>().sprite = Resources.Load<Sprite>(starImage) as Sprite;
        }
    }
}
