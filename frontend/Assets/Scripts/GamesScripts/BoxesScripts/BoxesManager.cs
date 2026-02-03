using Assets.Script.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxesManager : GamesScript, IGameable
{
    #region Instances and UI Components
    public TutorialController tutorialController;
    private AchievementsController achievementsController;

    [Header("UI/Canvas Components")]
    public Transform scrollViewContent;
    public Text countDownText;
    public GameObject topPanel;
    public GameObject timer;
    public GameObject lives;
    public GameObject imagePrefab;
    public GameObject recallBtn;
    public GameObject bottomIconsContainer;
    public GameObject nextPosBottom;
    public GameObject rsg;
    
    [Header("Prefabs")]
    public GameObject midGameQuitPanel;
    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject challengeFirstPlayPanel;
    public GameObject challengeWinPanel;
    public GameObject challengeLossPanel;
    public GameObject challengeDrawPanel;
    
    public GameObject achievementsPrefab;

    [Header("Lists")]
    public GameObject[] levelPanels;
    public Image[] hearts;
    public List<Achievement> achievementsList;

    public int spawnNumber { get; set; }
    public int countForWin { get; set; }
    public int countForLose { get; set; }
    public int maxMistakes { get; set; }
    public bool inGame { get; set; }

    readonly float sTime;
    readonly float timeSpeed = 10f; // Sa ma i madh speed aq ma kadale shkon

    private Image[] imagesArray;
    private GameObject panelLvl;

    private int[] imageNumbers;
    private int[] numbersOfImages;

    private string boxesScene;

    private bool shouldCountTime;
    private float countdownTime;

    private float preGameTimeAllocation = 90;
    private float gameTimeAllocation = 180;

    #endregion

    void Start()
    {
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
            boxesScene = "BoxesSceneIpad";
        else
            boxesScene = "BoxesScene";

        if (!ChallengeConstants.s_isChallenge && LevelScript._selectedLevel == 1 && GameLevelConstants.s_boxesLevels.Length == 0)
            tutorialController.InitializeTutorial(false);
        else
            InitializeGame();

        achievementsController = GetComponent<AchievementsController>();
    }

    private void Update()
    {
        if (shouldCountTime && countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            countDownText.text = Mathf.Round(countdownTime).ToString();
        }
        else if(shouldCountTime && countdownTime <= 0)
        {
            shouldCountTime = false;
            TimeUp();
        }
    }

    public void InitializeGame()
    {
        InitializeGameScript();
        tutorialController.DisableTutorialObjects(true);
        LevelSelector();
        inGame = false;
        InitArrays();
        SpawnImages();
        EnableHeartsPerLevel();
        StartCoroutine(SequenceStart());
    }

    IEnumerator SequenceStart()
    {
        yield return StartCoroutine(ShowRSG(rsg));
        countdownTime = preGameTimeAllocation;
        shouldCountTime = true;
    }

    void InitArrays()
    {
        numbersOfImages = new int[95];
    }

    /// <summary>
    /// Based on selected level calls appropriate attributes for this level
    /// </summary>
    public void LevelSelector()
    {
        switch (LevelScript._selectedLevel)
        {
            case 1:
                LevelAttributes(2, 3, 0);
                break;
            case 2:
                LevelAttributes(3, 3, 1);
                break;
            case 3:
                LevelAttributes(4, 3, 2);
                break;
            case 4:
                LevelAttributes(4, 3, 3);
                break;
            case 5:
                LevelAttributes(5, 3, 4);
                break;
            case 6:
                LevelAttributes(6, 5, 5);
                break;
            case 7:
                LevelAttributes(6, 5, 6);
                break;
            case 8:
                LevelAttributes(8, 5, 7);
                break;
            case 9:
                LevelAttributes(8, 5, 8);
                break;
            case 10:
                LevelAttributes(9, 5, 9);
                break;
            case 11:
                LevelAttributes(10, 5, 10);
                break;
            case 12:
                LevelAttributes(12, 5, 11);
                break;
            case 13:
                LevelAttributes(12, 5, 12);
                break;
            case 14:
                LevelAttributes(15, 5, 13);
                break;
            case 15:
                LevelAttributes(15, 5, 14);
                break;
            case 16:
                LevelAttributes(15, 5, 15);
                break;
            case 17:
                LevelAttributes(16, 5, 16);
                break;
            case 18:
                LevelAttributes(20, 5, 17);
                break;
            case 19:
                LevelAttributes(20, 5, 18);
                break;
            case 20:
                LevelAttributes(22, 5, 19);
                break;
        }
    }

    /// <summary>
    /// Adds appropriate attributes to a specific level
    /// </summary>
    /// <param name="spawn"></param>
    /// <param name="mistakes"></param>
    /// <param name="panel"></param>
    private void LevelAttributes(int spawn, int mistakes, int panel)
    {
        spawnNumber = spawn;
        maxMistakes = mistakes;
        levelPanels[panel].SetActive(true);
        panelLvl = levelPanels[panel];
    }

    void SpawnImages()
    {
        imageNumbers = new int[spawnNumber];
        SetNumbersOnArray();
        imagesArray = panelLvl.GetComponentsInChildren<Image>();
        int j = 0;

        for (int i = 0; i < imagesArray.Length; i++)
        {
            if (imagesArray[i].name == "ImgPref" && j < spawnNumber)
            {
                imagesArray[i - 1].GetComponentsInChildren<Image>()[0].name = imageNumbers[j].ToString() + " Number";

                imagesArray[i].GetComponentsInChildren<Image>()[0].sprite = Resources.Load<Sprite>("3DImages\\Artboard  (" + imageNumbers[j] + ")") as Sprite;
                imagesArray[i - 1].GetComponent<DragAndDropCell>().enabled = false;
                imagesArray[i].GetComponent<DragAndDropItem>().enabled = false;

                GameObject g = Instantiate(imagePrefab) as GameObject;
                g.GetComponentsInChildren<Image>()[1].name = imageNumbers[j].ToString();
                g.GetComponentsInChildren<Image>()[1].sprite = Resources.Load<Sprite>("3DImages\\Artboard  (" + imageNumbers[j] + ")") as Sprite;
                g.transform.SetParent(scrollViewContent, false);
                g.SetActive(true);

                j++;
            }
        }
    }

    /// <summary>
    /// Populate the array with numbers
    /// </summary>
    void SetNumbersOnArray()
    {
        for (int i = 1; i <= numbersOfImages.Length; i++)
            numbersOfImages[i - 1] = i;
        Shuffle(numbersOfImages);
        for (int i = 0; i < spawnNumber; i++)
            imageNumbers[i] = numbersOfImages[i];
    }

    public void Recall()
    {
        recallBtn.SetActive(false);
        RemoveImagesAfterRecall();
        StartCoroutine(SetBottomPanelToPlace());

        float preGameTimeSpent = preGameTimeAllocation - countdownTime;
        IncreaseTime(preGameTimeSpent);

        countdownTime = gameTimeAllocation;
        shouldCountTime = true;
        inGame = true;
    }

    /// <summary>
    /// Activate hart panel
    /// </summary>
    void EnableHeartsPerLevel()
    {
        for (int i = 0; i < maxMistakes; i++)
        {
            hearts[i].gameObject.SetActive(true);
        }
    }

    IEnumerator SetBottomPanelToPlace()
    {
        float t = (Time.time - sTime) / timeSpeed;
        while (true)
        {
            bottomIconsContainer.transform.position = Vector2.Lerp(bottomIconsContainer.transform.position, nextPosBottom.transform.position, t);
            yield return null;
        }
    }

    void RemoveImagesAfterRecall()
    {
        imagesArray = panelLvl.GetComponentsInChildren<Image>();

        for (int i = 0; i < imagesArray.Length; i++)
        {
            if (imagesArray[i].name == "ImgPref")
            {
                imagesArray[i].enabled = false;
                imagesArray[i - 1].GetComponent<DragAndDropCell>().enabled = true;
                imagesArray[i].GetComponent<DragAndDropItem>().enabled = true;
            }
        }
    }

    /// <summary>
    /// Called on GameOver
    /// </summary>
    public void Finished()
    {
        float gameTimeSpent = gameTimeAllocation - countdownTime;
        IncreaseTime(gameTimeSpent);

        GameOver(challengeLossPanel, losePanel, boxesScene);

        StopAllCoroutines();
    }

    // Use case of You Won is handled in the DragAndDropCell script
    public void YouWon()
    {
        shouldCountTime = false;

        float gameTimeSpent = gameTimeAllocation - countdownTime;
        IncreaseTime(gameTimeSpent);

        if (ChallengeConstants.s_isChallenge)
        {
            ChallangeFinished(countdownTime.ToString(), challengeFirstPlayPanel, challengeLossPanel, challengeWinPanel,
                challengeDrawPanel, "Boxes", achievementsPrefab, achievementsList, achievementsController);
        }
        else
        {
            LevelCompleted(countDownText.text, GameLevelConstants.s_boxesLevels, "Boxes");
            StartCoroutine(ActivateWinPanelDelayOneSecond());
        }
    }

    IEnumerator ActivateWinPanelDelayOneSecond()
    {
        yield return new WaitForSeconds(0.5f);

        achievementsController.LevelCompletedAchievements(countDownText.text, 0, winPanel,
            boxesScene, "Boxes", achievementsPrefab, achievementsList);

        StopAllCoroutines();
    }

    public void MidGameQuitBtn()
    {
        MidGameQuit(midGameQuitPanel);
    }

    /// <summary>
    /// Called when any of the coroutines is finished
    /// Will start or finish the game
    /// </summary>
    private void TimeUp()
    {
        if (!inGame)
            Recall();
        else
            Finished();
    }
}
