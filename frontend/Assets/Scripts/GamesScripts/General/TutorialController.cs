using UnityEngine.UI;
using UnityEngine;
using System;

public class TutorialController : MonoBehaviour
{
    [SerializeField]
    private GameObject gameManagers;

    [SerializeField]
    private GameObject[] tutorialGameObjects;
    
    private IGameable gameable;
    private int tutorialIndex;

    private string SeenKey { get { return "tutorial.v1." + LevelProgress.Owner + "." + Assets.Script.Constants.StaticVar.s_game; } }
    private void BeginGame()
    {
        PlayerPrefs.SetInt(SeenKey, 1); PlayerPrefs.Save();
        DisableTutorialObjects(true);
        gameable.InitializeGame();
    }

    /// <summary>
    /// Initializes tutorial objects 
    /// </summary>
    public void InitializeTutorial(bool iPadIncluded = true)
    {
        gameable = gameManagers.GetComponent<IGameable>();

        if (PlayerPrefs.GetInt(SeenKey, 0) == 1) { BeginGame(); return; }
        DisableTutorialObjects(false);
        for (int i = 0; i < tutorialGameObjects.Length; i++)
        {
            tutorialGameObjects[i].transform.GetChild(3).GetComponent<Button>().onClick.AddListener(() => NextTutorialObject());
            tutorialGameObjects[i].transform.GetChild(4).GetComponent<Button>().onClick.AddListener(() => BeginGame());

            if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3 && iPadIncluded)
            {
                tutorialGameObjects[i].transform.GetChild(2).gameObject.SetActive(false);
                tutorialGameObjects[i].transform.GetChild(5).gameObject.SetActive(true);
            }
        }
        tutorialGameObjects[tutorialIndex].SetActive(true);
    }

    /// <summary>
    /// This function will show the next tutorial in que.
    /// Or initialize the game if no more tutorials available
    /// </summary>
    void NextTutorialObject()
    {
        if (tutorialIndex == tutorialGameObjects.Length - 1)
        {
            DisableTutorialObjects(false);
            tutorialGameObjects[tutorialIndex].SetActive(false);
            BeginGame();
        }
        else
        {
            tutorialIndex++;
            DisableTutorialObjects(false);
            tutorialGameObjects[tutorialIndex].SetActive(true);
        }
    }

    /// <summary>
    /// Handles disabling of tutorial game objects. Used to navigate and to close tutorials
    /// </summary>
    /// <param name="disableAll"></param>
    public void DisableTutorialObjects(bool disableAll)
    {
        for (int i = 0; i < tutorialGameObjects.Length; i++)
        {
            //Disable other objects except current one
            if(!disableAll)
            {
                if (i != tutorialIndex)
                    tutorialGameObjects[i].SetActive(false);
            }
            //Disable all objects, close tutorials
            else
            {
                tutorialGameObjects[i].SetActive(false);
            }
        }
    }
}
