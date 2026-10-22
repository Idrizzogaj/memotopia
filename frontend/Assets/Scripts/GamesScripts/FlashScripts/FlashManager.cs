using Assets.Script.Constants;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlashManager : GamesScript, IGameable
{
    #region Instances and UI Components
    public TutorialController tutorialController;
    private AchievementsController achievementsController;

    [Header("UI/Canvas Components")]
    public Text timerText;
    public GameObject activeCellsPanel;
    public GameObject deactiveCellsPanel;
    public GameObject recallPanel;
    public GameObject topPanel;
    public GameObject topPanelNext;
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
    public Image[] hearts;
    public List<Achievement> achievementsList;

    private int imagesToShowCount;
    private int imagesToShowNumber;
    private int correctSelected;
    private int wrongSelected;
    private int maxMistakes;

    readonly float sTime;
    readonly float timeSpeed = 250f; // Sa ma i madh speed aq ma kadale shkon

    private int[] numbersOfImages;
    private int[] imagesNumberForRecall;
    private int[] imagesNumberForRecallNoShuffle;

    private bool runOnceCondition = true;
    private bool shouldCountTime;
    private bool shouldCountPreGameTime;
    private float countdownTime;

    private float gameTimeAllocation = 180;
    private float preGameTimeSpent = 0;
    #endregion

    private void OnEnable()
    {
        CellController.onShowImage += GetChoosenImageNumber;
        CellController.afterShownImage += NextImageNumber;
        CellController.onAnimationFinished += ShouldAnimationGetTriggered;
    }

    private void OnDisable()
    {
        CellController.onShowImage -= GetChoosenImageNumber;
        CellController.afterShownImage -= NextImageNumber;
        CellController.onAnimationFinished -= ShouldAnimationGetTriggered;
    }

    #region Initializations
    public void PositionHeader()
    {
        var panel = (RectTransform)topPanel.transform;
        panel.anchoredPosition = ((RectTransform)topPanelNext.transform).anchoredPosition;
    }

    void Start()
    {
        PositionHeader();
        if (!ChallengeConstants.s_isChallenge && LevelScript._selectedLevel == 1 && GameLevelConstants.s_flashLevels.Length == 0)
            tutorialController.InitializeTutorial();
        else
            InitializeGame();

        achievementsController = GetComponent<AchievementsController>();
    }

    private void Update()
    {
        if(SprintRuntime.Active){timerText.text=SprintRuntime.Current.ClockText;return;}
        if(shouldCountPreGameTime)
            preGameTimeSpent += Time.deltaTime;

        if (shouldCountTime && countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;
            timerText.text = Mathf.Round(countdownTime).ToString();
        }
        else if (shouldCountTime && countdownTime <= 0)
        {
            shouldCountTime = false;
            TimeUp();
        }
    }


    /// <summary>
    /// Called when the coroutine is finished
    /// Will trigger game over
    /// </summary>
    private void TimeUp()
    {
        if (runOnceCondition)
        {
            float gameTimeSpent = gameTimeAllocation - countdownTime;
            IncreaseTime(gameTimeSpent);
            RoundFailed(challengeFirstPlayPanel, challengeLossPanel, challengeWinPanel, challengeDrawPanel, losePanel, "FlashScene", "Flash",
                achievementsPrefab, achievementsList, achievementsController, correctSelected, countdownTime);
            runOnceCondition = false;
        }
    }

    public void InitializeGame()
    {
        InitializeGameScript();
        tutorialController.DisableTutorialObjects(true);
        CardFaces.BeginSession(); // what the cards show: pictures, numbers or words
        LevelSelector();
        InitArrays();
        SetNumbersOnArray();
        StartCoroutine(ShowImagesInOrder());
    }

    void InitArrays()
    {
        numbersOfImages = new int[CardFaces.ContentCount];
        imagesNumberForRecall = new int[imagesToShowNumber];
        imagesNumberForRecallNoShuffle = new int[imagesToShowNumber];
    }

    /// <summary>
    /// Set a name on each gameObject card
    /// </summary>
    void SetNumbersOnArray()
    {
        for (int i = 1; i <= numbersOfImages.Length; i++)
            numbersOfImages[i - 1] = i;

        Shuffle(numbersOfImages);
    }

    /// <summary>
    /// Based on selected level calls appropriate attributes for this level
    /// </summary>
    public void LevelSelector()
    {
        if(SprintRuntime.Active){LevelAttributes(SprintRuntime.Current.Size,5);return;}
        switch (LevelScript._selectedLevel)
        {
            case 1:
                LevelAttributes(3, 3);
                break;
            case 2:
                LevelAttributes(4, 3);
                break;
            case 3:
                LevelAttributes(5, 3);
                break;
            case 4:
                LevelAttributes(6, 3);
                break;
            case 5:
                LevelAttributes(7, 3);
                break;
            case 6:
                LevelAttributes(8, 3);
                break;
            case 7:
                LevelAttributes(9, 3);
                break;
            case 8:
                LevelAttributes(10, 4);
                break;
            case 9:
                LevelAttributes(11, 4);
                break;
            case 10:
                LevelAttributes(12, 4);
                break;
            case 11:
                LevelAttributes(14, 4);
                break;
            case 12:
                LevelAttributes(16, 4);
                break;
            case 13:
                LevelAttributes(18, 4);
                break;
            case 14:
                LevelAttributes(20, 5);
                break;
            case 15:
                LevelAttributes(21, 5);
                break;
            case 16:
                LevelAttributes(22, 5);
                break;
            case 17:
                LevelAttributes(24, 5);
                break;
            case 18:
                LevelAttributes(26, 5);
                break;
            case 19:
                LevelAttributes(28, 5);
                break;
            case 20:
                LevelAttributes(32, 5);
                break;
            case MemoryAthlete.Level:
                if(!ChallengeConstants.s_isChallenge){LevelAttributes(32,1);break;}
                LevelAttributes(MemoryAthlete.FlashImages, MemoryAthlete.FlashMistakes);
                gameTimeAllocation = MemoryAthlete.FlashRecallSeconds;
                break;
        }
    }

    /// <summary>
    /// Adds appropriate attributes to a specific level
    /// </summary>
    /// <param name="numberOfCards"></param>
    private void LevelAttributes(int imagesToShow, int mistakes)
    {
        imagesToShowNumber = imagesToShow;
        maxMistakes = mistakes;
    }
    #endregion

    #region Spawn Images
    /// <summary>
    /// Gradually show images in order that need to be found.
    /// </summary>
    /// <returns></returns>
    IEnumerator ShowImagesInOrder()
    {
        yield return StartCoroutine(ShowRSG(rsg));
        ShouldAnimationGetTriggered();
        shouldCountPreGameTime = true;
    }

    /// <summary>
    /// Check if ther are more cells/images that need to be shown or
    /// should we start the game
    /// </summary>
    private void ShouldAnimationGetTriggered()
    {
        if (imagesToShowCount < imagesToShowNumber)
            TriggerAnimationOnRandomCell();

        else if (imagesToShowCount >= imagesToShowNumber)
        {
            imagesToShowCount--;
            print("Finish Spawn Images");
            StartGame();
        }
    }

    /// <summary>
    /// We will get a random number based on available cells
    /// Trigger the animation on that cell.
    /// In animation we have two events that are called and those
    /// events will show the proper image. Look CellController.cs
    /// </summary>
    void TriggerAnimationOnRandomCell()
    {
        int available=activeCellsPanel.transform.childCount-imagesToShowCount;
        int randomCellNumber = SprintRuntime.Active?(int)(SprintRuntime.Current.Random.Next()%(uint)available):UnityEngine.Random.Range(0,available);
        GameObject cell = activeCellsPanel.transform.GetChild(Mathf.Abs(randomCellNumber)).gameObject;
        cell.GetComponent<Animator>().SetTrigger("Play");
        cell.transform.SetAsLastSibling();
        imagesNumberForRecall[imagesToShowCount] = numbersOfImages[imagesToShowCount];
        imagesNumberForRecallNoShuffle[imagesToShowCount] = numbersOfImages[imagesToShowCount];
    }

    /// <summary>
    /// Get the name/number of the image that need to be show
    /// </summary>
    /// <returns></returns>
    private int GetChoosenImageNumber()
    {
        return numbersOfImages[imagesToShowCount];
    }

    /// <summary>
    /// Increase the number of the image that need to be show
    /// it is called in an Action event from CellController
    /// </summary>
    private void NextImageNumber()
    {
        imagesToShowCount++;
    }

    #endregion

    #region Game

    /// <summary>
    /// Prepare the game to start.
    /// </summary>
    void StartGame()
    {
        activeCellsPanel.SetActive(false);
        deactiveCellsPanel.SetActive(false);
        recallPanel.SetActive(true);

        PopulateGamePanel();
        EnableHeartsPerLevel();
        StartCoroutine(SetTopPanelToPlace());
        countdownTime = gameTimeAllocation;

        shouldCountPreGameTime = false;
        IncreaseTime(preGameTimeSpent);
        preGameTimeSpent = 0;

        shouldCountTime = true;
    }

    /// <summary>
    /// Add on the panel images that wher shown before starting the game.
    /// </summary>
    void PopulateGamePanel()
    {
        Shuffle(imagesNumberForRecall);

        GameObject recallImageCellBtn = recallPanel.transform.GetChild(0).gameObject;

        for (int i = 0; i < imagesToShowNumber; i++)
        {
            GameObject newRecallImageCellBtn = Instantiate(recallImageCellBtn, recallPanel.transform) as GameObject;
            newRecallImageCellBtn.name = imagesNumberForRecall[i].ToString();

            newRecallImageCellBtn.GetComponent<Button>().image.overrideSprite =
                CardFaces.SmallFace(imagesNumberForRecall[i]);
            newRecallImageCellBtn.GetComponent<Button>().image.preserveAspect=true;

            newRecallImageCellBtn.GetComponent<Button>().onClick.AddListener(() => CompareImages(newRecallImageCellBtn));

        }
        recallImageCellBtn.SetActive(false);
        if (!SprintRuntime.Active && MemoryAthlete.IsRound && ChallengeConstants.s_isChallenge) ArrangeAthleteGrid();
    }

    // Memory Athlete shows 42 images: lay them out evenly (6 x 7 or 7 x 6), never a lone image on the last row.
    void ArrangeAthleteGrid()
    {
        var grid = recallPanel.GetComponent<GridLayoutGroup>();
        var rect = recallPanel.transform as RectTransform;
        if (grid == null || rect == null) return;
        Canvas.ForceUpdateCanvases();
        float w = rect.rect.width - grid.padding.left - grid.padding.right;
        float h = rect.rect.height - grid.padding.top - grid.padding.bottom;
        if (w <= 0f || h <= 0f) return;
        float gap = Mathf.Max(grid.spacing.x, grid.spacing.y);
        int bestColumns = 6;
        float best = 0f;
        foreach (int columns in new[] { 6, 7 })
        {
            int rows = Mathf.CeilToInt(imagesToShowNumber / (float)columns);
            float cell = Mathf.Min((w - gap * (columns - 1)) / columns, (h - gap * (rows - 1)) / rows);
            if (cell > best) { best = cell; bestColumns = columns; }
        }
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = bestColumns;
        grid.cellSize = new Vector2(best, best);
        grid.childAlignment = TextAnchor.UpperCenter;
    }

    /// <summary>
    /// Compare if the clicked image is the correct one 
    /// </summary>
    /// <param name="newRecallImageCellBtn"></param>
    void CompareImages(GameObject newRecallImageCellBtn)
    {
        if(SprintRuntime.Active && !SprintRuntime.Current.Ready)return;
        if (int.Parse(newRecallImageCellBtn.name) == imagesNumberForRecallNoShuffle[correctSelected])
        {
            correctSelected++;
            if(SprintRuntime.Active)SprintRuntime.Current.Solve(newRecallImageCellBtn.name);
            newRecallImageCellBtn.SetActive(false);
        }
        else
        {
            if(SprintRuntime.Active){SprintRuntime.Current.Error();return;}
            wrongSelected++;
            hearts[maxMistakes - wrongSelected].gameObject.SetActive(false);
        }
        if (wrongSelected == maxMistakes)
        {
            float gameTimeSpent = gameTimeAllocation - countdownTime;
            IncreaseTime(gameTimeSpent);

            // stop first: the result panel is shown by a coroutine that RoundFailed starts
            shouldCountTime = false;
            StopAllCoroutines();

            RoundFailed(challengeFirstPlayPanel, challengeLossPanel, challengeWinPanel, challengeDrawPanel, losePanel, "FlashScene", "Flash",
                achievementsPrefab, achievementsList, achievementsController, correctSelected, countdownTime);
        }
        else if (correctSelected == imagesToShowNumber)
        {
            if(SprintRuntime.Active)return;
            shouldCountTime = false;
            if (ChallengeConstants.s_isChallenge)
            {
                ChallangeFinished(countdownTime.ToString(), challengeFirstPlayPanel, challengeLossPanel,
                    challengeWinPanel, challengeDrawPanel, "Flash", achievementsPrefab, achievementsList, achievementsController,
                    imagesToShowNumber, countdownTime);
            }
            else
            {
                LevelCompleted(timerText.text, GameLevelConstants.s_flashLevels, "Flash");
                StartCoroutine(ActivateWinPanelDelayHalfSecond());
            }
            float gameTimeSpent = gameTimeAllocation - countdownTime;
            IncreaseTime(gameTimeSpent);
        }
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

    IEnumerator ActivateWinPanelDelayHalfSecond()
    {
        yield return new WaitForSeconds(0.5f);
        //if (!IsAchievementCompleated(timerText.text, 0, winPanel, "FlashScene", "Flash", achievementsPanel, achivement))
        //    ActivateWinPanel(timerText.text, 0, winPanel, "FlashScene", "Flash");

        achievementsController.LevelCompletedAchievements(timerText.text, 0, winPanel,
            "FlashScene", "Flash", achievementsPrefab, achievementsList);

        StopAllCoroutines();
    }

    IEnumerator SetTopPanelToPlace()
    {
        yield return MovePanel(topPanel.transform, topPanelNext.transform);
    }

    #endregion

    /// <summary>
    /// Called when quite/back button is pressed while on the game
    /// Referenced outside on a button
    /// </summary>
    public void MidGameQuitBtn()
    {
        MidGameQuit(midGameQuitPanel);
    }
}
