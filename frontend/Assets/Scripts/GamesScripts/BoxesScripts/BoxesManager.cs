using Assets.Script.Constants;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        if(SprintRuntime.Active){countDownText.text=SprintRuntime.Current.ClockText;return;}
        if (shouldCountTime && (countdownTime > 0 || PracticeMode.Untimed))
        {
            countdownTime -= Time.deltaTime;
            countDownText.text = PracticeMode.Untimed?PracticeSession.Clock(this):Mathf.Round(countdownTime).ToString();
        }
        else if(shouldCountTime && countdownTime <= 0 && !PracticeMode.Untimed)
        {
            shouldCountTime = false;
            TimeUp();
        }
    }

    public void FitAbove(GameObject lowerControl){BoxesBoardFit.Fit(panelLvl,lowerControl,topPanel);}

    public void InitializeGame()
    {
        InitializeGameScript();
        tutorialController.DisableTutorialObjects(true);
        CardFaces.BeginSession(); // what the cards show: pictures, numbers or words
        LevelSelector();
        inGame = false;
        InitArrays();
        SpawnImages();
        BoxesBoardFit.Fit(panelLvl,recallBtn,topPanel);
        EnableHeartsPerLevel();
        StartCoroutine(SequenceStart());
    }

    IEnumerator SequenceStart()
    {
        yield return StartCoroutine(ShowRSG(rsg));
        countdownTime = preGameTimeAllocation;
        PracticeSession.Begin(this, "Memory.Boxes", LevelScript._selectedLevel);
        shouldCountTime = true;
    }

    void InitArrays()
    {
        numbersOfImages = new int[CardFaces.ContentCount];
    }

    /// <summary>
    /// Based on selected level calls appropriate attributes for this level
    /// </summary>
    public void LevelSelector()
    {
        if(SprintRuntime.Active){
            int wanted=SprintRuntime.Current.Size,best=levelPanels.Length-1,count=int.MaxValue;
            for(int i=0;i<levelPanels.Length;i++){int available=levelPanels[i].GetComponentsInChildren<Image>(true).Count(image=>image.name=="ImgPref");if(available>=wanted && available<count){best=i;count=available;}}
            LevelAttributes(wanted,5,best);return;
        }
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
            case MemoryAthlete.Level:
                if(!ChallengeConstants.s_isChallenge){LevelAttributes(22,1,19);break;}
                LevelAttributes(MemoryAthlete.BoxesBoxes, MemoryAthlete.BoxesMistakes, 19);
                AddExtraBoxes(MemoryAthlete.BoxesBoxes - 22);
                gameTimeAllocation = MemoryAthlete.BoxesRecallSeconds;
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

    // The level-20 panel has 22 boxes in a honeycomb of four columns. Memory Athlete needs 24, so two more are
    // cloned at the top of the two inner columns (there is room above them).
    void AddExtraBoxes(int extra)
    {
        var cells = panelLvl.GetComponentsInChildren<DragAndDropCell>(true).Select(c => (RectTransform)c.transform).ToList();
        if (cells.Count == 0 || extra <= 0) return;
        float innerX = cells.Min(c => Mathf.Abs(c.anchoredPosition.x));
        var inner = cells.Where(c => Mathf.Abs(Mathf.Abs(c.anchoredPosition.x) - innerX) < 30f).ToList();
        var columns = new List<List<RectTransform>>
        {
            inner.Where(c => c.anchoredPosition.x < 0f).OrderByDescending(c => c.anchoredPosition.y).ToList(),
            inner.Where(c => c.anchoredPosition.x >= 0f).OrderByDescending(c => c.anchoredPosition.y).ToList(),
        };
        int added = 0;
        foreach (var column in columns)
        {
            if (added >= extra || column.Count < 2) continue;
            float step = column[0].anchoredPosition.y - column[1].anchoredPosition.y;
            var clone = Instantiate(column[0].gameObject, column[0].parent);
            clone.name = "BoxesCell (" + (200 + added) + ")";
            ((RectTransform)clone.transform).anchoredPosition = column[0].anchoredPosition + new Vector2(0f, step);
            added++;
        }
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

                imagesArray[i].GetComponentsInChildren<Image>()[0].sprite = CardFaces.SmallFace(imageNumbers[j]);
                BoxesPicturePerspective.Attach(imagesArray[i].GetComponentsInChildren<Image>()[0]);
                imagesArray[i - 1].GetComponent<DragAndDropCell>().enabled = false;
                imagesArray[i].GetComponent<DragAndDropItem>().enabled = false;

                GameObject g = Instantiate(imagePrefab) as GameObject;
                g.GetComponentsInChildren<Image>()[1].name = imageNumbers[j].ToString();
                g.GetComponentsInChildren<Image>()[1].sprite = CardFaces.SmallFace(imageNumbers[j]);
                BoxesPicturePerspective.Attach(g.GetComponentsInChildren<Image>()[1]);
                g.transform.SetParent(scrollViewContent, false);
                g.SetActive(true);

                j++;
            }
        }
        if(SprintRuntime.Active)foreach(var cell in panelLvl.GetComponentsInChildren<DragAndDropCell>(true))if(!cell.name.EndsWith(" Number"))cell.gameObject.SetActive(false);
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
        if(SprintRuntime.Active && !SprintRuntime.Current.Ready)return;
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
        if(SprintRuntime.Active){foreach(var heart in hearts)heart.gameObject.SetActive(false);return;}
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].gameObject.SetActive(i<maxMistakes);
        }
    }

    IEnumerator SetBottomPanelToPlace()
    {
        yield return MovePanel(bottomIconsContainer.transform, nextPosBottom.transform);
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

        // Stop timers and animations FIRST: in a challenge the result panel is shown by a coroutine that
        // RoundFailed starts, and stopping afterwards kills it (the round then seemed to go on at 0:00).
        shouldCountTime = false;
        inGame = false;
        StopAllCoroutines();

        RoundFailed(challengeFirstPlayPanel, challengeLossPanel, challengeWinPanel, challengeDrawPanel, losePanel, boxesScene, "Boxes",
            achievementsPrefab, achievementsList, achievementsController, countForWin, countdownTime);
    }

    // Use case of You Won is handled in the DragAndDropCell script
    public void YouWon()
    {
        if(SprintRuntime.Active)return;
        shouldCountTime = false;

        float gameTimeSpent = gameTimeAllocation - countdownTime;
        IncreaseTime(gameTimeSpent);

        if (ChallengeConstants.s_isChallenge)
        {
            ChallangeFinished(countdownTime.ToString(), challengeFirstPlayPanel, challengeLossPanel, challengeWinPanel,
                challengeDrawPanel, "Boxes", achievementsPrefab, achievementsList, achievementsController,
                spawnNumber, countdownTime);
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
