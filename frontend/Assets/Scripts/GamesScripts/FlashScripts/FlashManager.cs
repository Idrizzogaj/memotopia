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
    void Start()
    {
        if (!ChallengeConstants.s_isChallenge && LevelScript._selectedLevel == 1 && GameLevelConstants.s_flashLevels.Length == 0)
            tutorialController.InitializeTutorial();
        else
            InitializeGame();

        achievementsController = GetComponent<AchievementsController>();
    }

    private void Update()
    {
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
            GameOver(challengeLossPanel, losePanel, "FlashScene");
            runOnceCondition = false;
        }
    }

    public void InitializeGame()
    {
        InitializeGameScript();
        tutorialController.DisableTutorialObjects(true);
        LevelSelector();
        InitArrays();
        SetNumbersOnArray();
        StartCoroutine(ShowImagesInOrder());
    }

    void InitArrays()
    {
        numbersOfImages = new int[100];
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
        int randomCellNumber = UnityEngine.Random.Range(0, activeCellsPanel.transform.childCount - imagesToShowCount);
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
                Resources.Load<Sprite>("ImagesWithBackground\\Artboard-" + imagesNumberForRecall[i].ToString()) as Sprite;

            newRecallImageCellBtn.GetComponent<Button>().onClick.AddListener(() => CompareImages(newRecallImageCellBtn));

        }
        recallImageCellBtn.SetActive(false);
    }

    /// <summary>
    /// Compare if the clicked image is the correct one 
    /// </summary>
    /// <param name="newRecallImageCellBtn"></param>
    void CompareImages(GameObject newRecallImageCellBtn)
    {
        if (int.Parse(newRecallImageCellBtn.name) == imagesNumberForRecallNoShuffle[correctSelected])
        {
            correctSelected++;
            newRecallImageCellBtn.SetActive(false);
        }
        else
        {
            wrongSelected++;
            hearts[maxMistakes - wrongSelected].gameObject.SetActive(false);
        }
        if (wrongSelected == maxMistakes)
        {
            float gameTimeSpent = gameTimeAllocation - countdownTime;
            IncreaseTime(gameTimeSpent);

            GameOver(challengeLossPanel, losePanel, "FlashScene");

            StopAllCoroutines();
        }
        else if (correctSelected == imagesToShowNumber)
        {
            shouldCountTime = false;
            if (ChallengeConstants.s_isChallenge)
            {
                ChallangeFinished(countdownTime.ToString(), challengeFirstPlayPanel, challengeLossPanel,
                    challengeWinPanel, challengeDrawPanel, "Flash", achievementsPrefab, achievementsList, achievementsController);
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
        for (int i = 0; i < maxMistakes; i++)
        {
            hearts[i].gameObject.SetActive(true);
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
        float t = (Time.time - sTime) / timeSpeed;
        while (true)
        {
            topPanel.transform.position = Vector2.Lerp(topPanel.transform.position, topPanelNext.transform.position, t);
            yield return null;
        }
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
