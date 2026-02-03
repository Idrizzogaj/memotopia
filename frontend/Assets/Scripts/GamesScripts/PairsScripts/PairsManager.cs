using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Script.Constants;
using UnityEngine;
using UnityEngine.UI;

public class PairsManager : GamesScript, IGameable
{
    public TutorialController tutorialController;
    private AchievementsController achievementsController;

    [Header("UI/Canvas Components")]
    public Text firstCountDown;
    public Text firstCountDownIpad;
    public Text secondCountDown;
    public Image img1;
    public Image img2;
    public Image img1Ipad;
    public Image img2Ipad;
    public GameObject linkedPairsScreen;
    public GameObject linkedPairsScreenIpad;
    public GameObject gameScreen;
    public GameObject rsg;
    public GameObject cardContent;

    [Header("Prefabs")]
    public GameObject midGameQuitPanel;
    public GameObject losePanel;
    public GameObject winPanel;
    public GameObject challengeFirstPlayPanel;
    public GameObject challengeWinPanel;
    public GameObject challengeLossPanel;
    public GameObject challengeDrawPanel;
    public GameObject achievementsPrefab;
    public GameObject card;

    [Header("Lists")]
    public Dictionary<string, string> pairs = new Dictionary<string, string>();
    public List<Achievement> achievementsList;

    private int numberOfPairs = 0;
    private int gameOverCount = 0;
    private int nextCount = 0;
    private bool cardFaceShown;

    private string[] nameOfImages = new string[100];
    private string[] currentCards;

    private Button firstBtn;
    private Button secondBtn;
    private Button[] buttons;

    private bool shouldCountTime;
    private float countdownTime;

    private float preGameTimeAllocation = 90;
    private float gameTimeAllocation = 180;

    private void Start()
    {
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            linkedPairsScreenIpad.SetActive(true);
            linkedPairsScreen.SetActive(false);
        }
        else
        {
            linkedPairsScreen.SetActive(true);
            linkedPairsScreenIpad.SetActive(false);
        }

        if (!ChallengeConstants.s_isChallenge && LevelScript._selectedLevel == 1 && GameLevelConstants.s_pairsLevels.Length == 0)
            tutorialController.InitializeTutorial();
        else
            InitializeGame();

        achievementsController = GetComponent<AchievementsController>();
    }

    private void Update()
    {
        if ((int)(Math.Round((float)Screen.width / (float)Screen.height * 4)) == 3)
        {
            firstCountDownIpad.text = firstCountDown.text;
        }

        if (shouldCountTime && countdownTime > 0)
        {
            countdownTime -= Time.deltaTime;

            if (firstCountDown != null)
                firstCountDown.text = Mathf.Round(countdownTime).ToString();
            if (secondCountDown != null)
                secondCountDown.text = Mathf.Round(countdownTime).ToString();
        }

        else if (shouldCountTime && countdownTime <= 0)
        {
            shouldCountTime = false;
            TimeUp();
        }

    }

    /// <summary>
    /// Called when any of the coroutines is finished
    /// Will start or finish the game
    /// </summary>
    private void TimeUp()
    {
        float gameTimeSpent = gameTimeAllocation - countdownTime;
        IncreaseTime(gameTimeSpent);

        GameOver(challengeLossPanel, losePanel, "PairsScene");
    }

    public void InitializeGame()
    {
        InitializeGameScript();
        tutorialController.DisableTutorialObjects(true);
        LevelSelector();
        SetNames();
        NextPair();
        StartCoroutine(SequenceStart());
    }

    private IEnumerator SequenceStart()
    {
        yield return StartCoroutine(ShowRSG(rsg));
        countdownTime = preGameTimeAllocation;
        shouldCountTime = true;
    }

    /// <summary>
    /// Set a name on each gameObject card
    /// </summary>
    public void SetNames()
    {
        for (int i = 0; i < nameOfImages.Length; i++)
            nameOfImages[i] = "Artboard-" + (i + 1);
        Shuffle(nameOfImages);
    }

    /// <summary>
    /// Instantiates card(buttons) and adds them in the appropriate container
    /// </summary>
    public void LoadButtonsToCardContent()
    {
        for (int i = 0; i < currentCards.Length; i++)
        {
            GameObject btn = Instantiate(card, cardContent.transform, false);
            btn.name = "" + currentCards[i];
            buttons[i] = btn.GetComponent<Button>();
            buttons[i].image.overrideSprite = Resources.Load<Sprite>("GamesResources/Pairs/M") as Sprite;
            buttons[i].onClick.AddListener(() => CompareCards(btn));
        }
    }

    /// <summary>
    /// Handles navigation on the linked pairs. The process that is shown before starting the game
    /// It is called also outside when next button is pressed on that view
    /// </summary>
    public void NextPair()
    {
        if (nextCount >= numberOfPairs)
        {
            PrepareGameScreen();
            return;
        }

        img1.sprite = Resources.Load("ImagesWithoutBackground\\" + nameOfImages[nextCount], typeof(Sprite)) as Sprite;
        img2.sprite = Resources.Load("ImagesWithoutBackground\\" + nameOfImages[nextCount + 1], typeof(Sprite)) as Sprite;
        img1Ipad.sprite = Resources.Load("ImagesWithoutBackground\\" + nameOfImages[nextCount], typeof(Sprite)) as Sprite;
        img2Ipad.sprite = Resources.Load("ImagesWithoutBackground\\" + nameOfImages[nextCount + 1], typeof(Sprite)) as Sprite;

        currentCards[nextCount] = nameOfImages[nextCount];
        currentCards[nextCount + 1] = nameOfImages[nextCount + 1];

        pairs.Add(nameOfImages[nextCount], nameOfImages[nextCount + 1]);
        pairs.Add(nameOfImages[nextCount + 1], nameOfImages[nextCount]);
        nextCount += 2;
    }

    /// <summary>
    /// Check if pair is found by comparing shown cards
    /// </summary>
    /// <param name="selectedGameObject"></param>
    public void CompareCards(GameObject selectedGameObject)
    {
        selectedGameObject.GetComponent<Animator>().SetTrigger("Pressed");
        if (cardFaceShown)
        {
            //Get selected gameObject and change its image based on the object name
            secondBtn = selectedGameObject.GetComponent<Button>();
            secondBtn.image.overrideSprite = Resources.Load<Sprite>("ImagesWithBackground\\" + secondBtn.name) as Sprite;
            MakeButtonsInteractable(false);
            ChangeCardsColor(false);
            if (pairs[firstBtn.name] == secondBtn.name || pairs[secondBtn.name] == firstBtn.name)
            {
                gameOverCount -= 2;

                //If all pairs are found. Game won or challenge completed
                if (gameOverCount == 0)
                {
                    shouldCountTime = false;
                    if (ChallengeConstants.s_isChallenge)
                    {
                        ChallangeFinished(countdownTime.ToString(), challengeFirstPlayPanel, challengeLossPanel,
                            challengeWinPanel, challengeDrawPanel, "Pairs", achievementsPrefab, achievementsList, achievementsController);
                    }
                    else
                    {
                        LevelCompleted(secondCountDown.text, GameLevelConstants.s_pairsLevels, "Pairs");
                        StartCoroutine(ActivateWinPanelDelayHalfSecond());
                    }

                    float gameTimeSpent = gameTimeAllocation - countdownTime;
                    IncreaseTime(gameTimeSpent);
                }

                //Remove from the array buttons that are part of a found pair
                buttons = buttons.Where(val => val != firstBtn).ToArray();
                buttons = buttons.Where(val => val != secondBtn).ToArray();
                MakeButtonsInteractable(true);
            }
            else
            {
                StartCoroutine(Wait500MillisecondToReturn());
            }
        }
        else
        {
            //Get selected gameObject and change its image based on the object name
            firstBtn = selectedGameObject.GetComponent<Button>();
            firstBtn.image.overrideSprite = Resources.Load<Sprite>("ImagesWithBackground\\" + firstBtn.name) as Sprite;
        }
        cardFaceShown = !cardFaceShown;
    }

    private void ChangeCardsColor(bool on)
    {
        if (!on)
        {
            foreach (Transform item in cardContent.transform)
                item.GetComponent<Image>().color = new Color32(240, 240, 240, 255);
        }
        else
            foreach (var item in buttons)
                item.image.color = new Color32(255, 255, 255, 255);
    }

    private IEnumerator Wait500MillisecondToReturn()
    {
        yield return new WaitForSeconds(0.5F);
        firstBtn.image.overrideSprite = Resources.Load<Sprite>("GamesResources/Pairs/M") as Sprite;
        secondBtn.image.overrideSprite = Resources.Load<Sprite>("GamesResources/Pairs/M") as Sprite;
        MakeButtonsInteractable(true);
    }

    private IEnumerator ActivateWinPanelDelayHalfSecond()
    {
        yield return new WaitForSeconds(0.5f);
        achievementsController.LevelCompletedAchievements(secondCountDown.text, 0, winPanel,
           "PairsScene", "Pairs", achievementsPrefab, achievementsList);
    }

    /// <summary>
    /// Turn all the remaining buttons from the list in interactable mode
    /// </summary>
    public void MakeButtonsInteractable(bool interactable)
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].interactable = interactable;
        if (interactable)
            ChangeCardsColor(true);
    }

    /// <summary>
    /// Based on selected level calls appropriate attributes for this level
    /// </summary>
    public void LevelSelector()
    {
        switch (LevelScript._selectedLevel)
        {
            case 1:
                LevelAttributes(10);
                break;
            case 2:
                LevelAttributes(12);
                break;
            case 3:
                LevelAttributes(14);
                break;
            case 4:
                LevelAttributes(16);
                break;
            case 5:
                LevelAttributes(18);
                break;
            case 6:
                LevelAttributes(20);
                break;
            case 7:
                LevelAttributes(22);
                break;
            case 8:
                LevelAttributes(24);
                break;
            case 9:
                LevelAttributes(26);
                break;
            case 10:
                LevelAttributes(28);
                break;
            case 11:
                LevelAttributes(30);
                break;
            case 12:
                LevelAttributes(32);
                break;
            case 13:
                LevelAttributes(34);
                break;
            case 14:
                LevelAttributes(36);
                break;
            case 15:
                LevelAttributes(38);
                break;
            case 16:
                LevelAttributes(40);
                break;
            case 17:
                LevelAttributes(42);
                break;
            case 18:
                LevelAttributes(44);
                break;
            case 19:
                LevelAttributes(46);
                break;
            case 20:
                LevelAttributes(48);
                break;
        }
    }

    /// <summary>
    /// Adds appropriate attributes to a specific level
    /// </summary>
    /// <param name="numberOfCards"></param>
    private void LevelAttributes(int numberOfCards)
    {
        numberOfPairs = numberOfCards;
        currentCards = new string[numberOfPairs];
        gameOverCount = numberOfPairs;
        buttons = new Button[numberOfPairs];
    }

    /// <summary>
    /// Handles counting coroutines
    /// </summary>
    private void SetTime()
    {
        float preGameTimeSpent = preGameTimeAllocation - countdownTime;
        IncreaseTime(preGameTimeSpent);
        countdownTime = gameTimeAllocation;
        shouldCountTime = true;
    }

    /// <summary>
    /// Time has passed on pair linking/spawn images
    /// </summary>
    private void Recall()
    {
        for (int i = nextCount; i < currentCards.Length; i += 2)
        {
            currentCards[i] = nameOfImages[i];
            currentCards[i + 1] = nameOfImages[i + 1];
            pairs.Add(nameOfImages[i], nameOfImages[i + 1]);
            pairs.Add(nameOfImages[i + 1], nameOfImages[i]);
        }
        PrepareGameScreen();
    }

    /// <summary>
    /// Prepares and activates the game screen
    /// </summary>
    private void PrepareGameScreen()
    {
        linkedPairsScreen.SetActive(false);
        linkedPairsScreenIpad.SetActive(false);
        gameScreen.SetActive(true);
        SetTime();
        Shuffle(currentCards);
        LoadButtonsToCardContent();
    }

    /// <summary>
    /// Called when quite/back button is pressed while on the game
    /// Referenced outside on a button
    /// </summary>
    public void MidGameQuitBtn()
    {
        MidGameQuit(midGameQuitPanel);
    }
}