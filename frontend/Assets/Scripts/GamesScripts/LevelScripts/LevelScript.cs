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
                BackgroundImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources/Levels/BoxesBackground") as Sprite;
                TopGameName.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources/Levels/BoxesTop") as Sprite;
                r = 234;
                g = 24;
                b = 100;
                c = 255;

                break;

            case Games.Pairs:
                CellImage = Resources.Load<Sprite>("GamesResources/Levels/PairsLevelCell") as Sprite;
                BackgroundImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources/Levels/PairsBackground") as Sprite;
                TopGameName.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources/Levels/PairsTop") as Sprite;
                r = 36;
                g = 189;
                b = 224;
                c = 255;

                break;

            case Games.Flash:
                CellImage = Resources.Load<Sprite>("GamesResources/Levels/FlashLevelCell") as Sprite;
                BackgroundImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources/Levels/FlashBackground") as Sprite;
                TopGameName.GetComponent<Image>().sprite = Resources.Load<Sprite>("GamesResources/Levels/FlashTop") as Sprite;
                r = 254;
                g = 191;
                b = 75;
                c = 255;

                break;
        }

        for (int i = 0; i < LevelsCell.Length; i++)
        {
            var levels = LevelProgress.Get(StaticVar.s_game) ?? new Assets.Script.Models.GameLevel[0];
            int levelNumber = i + 1;
            var completed = System.Array.Find(levels, entry => entry.level == levelNumber);
            bool unlocked = levelNumber == 1 || completed != null || System.Array.Exists(levels, entry => entry.level == levelNumber - 1);
            var levelButton = LevelsCell[i].transform.GetChild(1).GetComponent<Button>();
            levelButton.interactable = unlocked;
            LevelsCell[i].sprite = unlocked ? OpenCellImage : CellImage;
            if (unlocked) levelButton.onClick.AddListener(() => StartGame());
            if (completed != null) InitStars(LevelsCell[i].transform.GetChild(0), completed.stars);
            else LevelsCell[i].transform.GetChild(0).gameObject.SetActive(false);


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
                    LoadingManager.Navigate(SceneName.s_boxesSceneIpad);
                }
                else
                {
                    LoadingManager.Navigate(SceneName.s_boxesScene);
                }

                break;

            case Games.Pairs:
                LoadingManager.Navigate(SceneName.s_pairsScene);

                break;

            case Games.Flash:
                LoadingManager.Navigate(SceneName.s_flashScene);

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
                    LoadingManager.Navigate(SceneName.s_boxesSceneIpad);
                }
                else
                {
                    LoadingManager.Navigate(SceneName.s_boxesScene);
                }
                break;
            case "Pairs":
                LoadingManager.Navigate(SceneName.s_pairsScene);
                break;
            case "Flash":
                LoadingManager.Navigate(SceneName.s_flashScene);
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
            case "Pairs":
                game = Games.Pairs;
                break;
            case "Boxes":
                game = Games.Boxes;
                break;
        }
    }

    public void LoadGameMenu()
    {
        //LoadingManager.Navigate("GameMenu");
        _loadingScreen.GoToSceneWithLoadingInstantly(SceneName.s_gameMenu);
    }

    public void InitStars(Transform StarsCell, int starsNr)
    {
        for (int j = 0 ; j < starsNr ; j++)
        {
            string starImage = "";

            if(j == 0){
                starImage = "GamesResources/Levels/Asset 15";
            } else if (j == 1) {
                starImage = "GamesResources/Levels/Asset 14";
            } else if (j == 2) {
                starImage = "GamesResources/Levels/Asset 13";
            }

            StarsCell.transform.GetChild(j).GetComponent<Image>().sprite = Resources.Load<Sprite>(starImage) as Sprite;
        }
    }
}
