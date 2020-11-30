﻿
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System;

public class GameManager : MonoBehaviour
{

    public GameObject canvas;

    private int numPlayersToAllocateBudget;
    private int numPlayersToExecuteBudget;
    private int numPlayersToDisplayHistory;
    private int numPlayersToSimulateInvestment;

    //------------ UI -----------------------------
    public GameObject playerUIPrefab;
    public GameObject investmentUIPrefab;

    //--------Tutorial-----------------------
    public GameObject tutorialScreens;
    public GameObject tutorialScreen;
    public GameObject tutorialScreen2;
    public GameObject tutorialScreen3;
    public GameObject tutorialScreen4;
    public GameObject tutorialScreen5;
    public GameObject tutorialScreen6;
    public GameObject tutorialScreen7;
    public Button advanceTutorialButton;

    public GameObject GenerationText;
    public GameObject FirstGenerationText;
    public GameObject GenerationNumberText;
    public GameObject simulateInvestmentScreen;
    public Button simulateEvolutionButton;
    public GameObject waitingForPlayers;
    public GameObject BetweenRoundScreen;
    public GameObject IntroductionScreen;
    public GameObject InformationScreen;

    public GameObject StartGameButtonObject;
    public Button NextInstructionsButton;
    public Button TenthGenButton;

    public GameObject ImpactCP;
    public Text ImpactCPtext;

    //Transition between rounds
    public GameObject UIroundSum;
    public Text roundSumTex;
    public GameObject NewGenerationScreen;
    public Text NewGenerationText;
    public Image InformationGen;

    public GameObject victoryOverlayUI;
    public GameObject lossOverlayUI;
    public Boolean GenerationResult;

    public Button passRoundButton;
    public Button newRoundButton;
    public Button NewGenerationButton;
    public Button PassInformationButton;

    public GameObject rollDiceOverlay;
    public GameObject diceUIPrefab;
    private DiceManager diceManager;

    public GameObject poppupPrefab;
    public PopupScreenFunctionalities infoPoppupNeutralRef;
    public PopupScreenFunctionalities infoPoppupLossRef;
    public PopupScreenFunctionalities infoPoppupWinRef;

    public GameObject poppupScreen;

    public GameObject CommonAreaUI;
    public Slider environmentSliderSceneElement;
    private DynamicSlider envDynamicSlider;

    public GameProperties.GamePhase currGamePhase;

    private bool gameMainSceneFinished;
    private int interruptionRequests; //changed whenever an interruption occurs (either a poppup, warning, etc.)

    private int currPlayerIndex;
    private float phaseEndDelay;

    public int roundTaken = 0;
    public Boolean stopbugButton = false;


    private int marketLimit;
    private int currNumberOfMarketDices;

    private Text GenerationTextUI;
    private Text GenerationNumberTextUI;

    private Text FirstGenerationTextUI;

    private GameObject GenerationEnvHistory;
    private Image GenPhotoUI;
    private Image EnvPhotoUI;

    private Sprite firstGenPhoto;

    public int InterruptGame()
    {
        interruptionRequests++;
        return 0;
    }
    public int ContinueGame()
    {
        interruptionRequests--;
        return 0;
    }

    IEnumerator WaitingForAIPlayers()
    {

        GenerationText.SetActive(false);
        GenerationEnvHistory.SetActive(false);
        waitingForPlayers.SetActive(true);
        yield return new WaitForSeconds(UnityEngine.Random.Range(5, 11));
        waitingForPlayers.SetActive(false);
        StartGameRoundForAllPlayers();

    }

    //Waiting for popScreen
    IEnumerator WaitingForAIPlayersToPlay()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0, 2));

    }

    void Start()
    {
        currGamePhase = GameProperties.GamePhase.BUDGET_ALLOCATION;

        //assign some game globals
        GameGlobals.gameManager = this;
        GameGlobals.currGameState = GameProperties.GameState.NOT_FINISHED;

        diceManager = new DiceManager(rollDiceOverlay, diceUIPrefab, GameGlobals.monoBehaviourFunctionalities, GameGlobals.diceLogic);

        // init main scene elements
        interruptionRequests = 0;
        InterruptGame(); //interrupt game update while loading...

        numPlayersToAllocateBudget = GameGlobals.players.Count;
        numPlayersToExecuteBudget = GameGlobals.players.Count;
        numPlayersToDisplayHistory = GameGlobals.players.Count;
        numPlayersToSimulateInvestment = GameGlobals.players.Count;

        currPlayerIndex = 0;

        //get player poppups (can be from any player) and set methods
        infoPoppupLossRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), "Audio/albumLoss");
        infoPoppupWinRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), "Audio/albumVictory");
        infoPoppupNeutralRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");

        gameMainSceneFinished = false;
        phaseEndDelay = 2.0f;
        GameGlobals.envState = 60; //Common Pool Resource
        GameGlobals.StartingEnvState = GameGlobals.envState;
        GameGlobals.envThreshold = GameGlobals.envState / 2;
        //GameGlobals.envRenew = 0.5; // At the end of the round the env renew 50%
        //GameGlobals.envRenewperRound = 0;
        GameGlobals.roundBudget = 20;
        GameGlobals.fairRefPoint = Convert.ToInt32((GameGlobals.envState / 2) / 4.5);
        GameGlobals.maxSelfish = GameGlobals.fairRefPoint * 2;
        GameGlobals.envStatePerRound = new List<int>();
        GameGlobals.waitingForPaux = true;

        //Study Conditions
        GameGlobals.firstGeneration = false;
        GameGlobals.ruinedPlanet = false;
        GameGlobals.informationOfGeneration = true;
        GameGlobals.conditionTag = "InterGenGoodPlanetInfo";


        GameGlobals.callMongoLogServer = gameObject.AddComponent<CallMongoLogServer>();

        if (GameGlobals.firstGeneration)
        {
            GameGlobals.generation = 1;
        }
        else
        {
            GameGlobals.generation = 4;
        }


        //Init Gen UI

        tutorialScreens = GameObject.Find("TutorialScreens");
        tutorialScreen = GameObject.Find("TutorialScreen");
        tutorialScreen2 = GameObject.Find("TutorialScreen2");
        tutorialScreen3 = GameObject.Find("TutorialScreen3");
        tutorialScreen4 = GameObject.Find("TutorialScreen4");
        tutorialScreen5 = GameObject.Find("TutorialScreen5");
        tutorialScreen6 = GameObject.Find("TutorialScreen6");
        tutorialScreen7 = GameObject.Find("TutorialScreen7");
        int tutorialAux = 1;
        advanceTutorialButton = tutorialScreens.transform.Find("advanceTutorialButton").gameObject.GetComponent<Button>();
        GenerationText = GameObject.Find("TenthGenerationText");
        TenthGenButton = GenerationText.transform.Find("TenthGenButton").gameObject.GetComponent<Button>();
        GenerationTextUI = GenerationText.transform.Find("genText").gameObject.GetComponent<Text>();

        FirstGenerationText = GameObject.Find("FirstGenerationText");
        FirstGenerationTextUI = FirstGenerationText.transform.Find("Text").gameObject.GetComponent<Text>();

        GenerationEnvHistory = GameObject.Find("EnvHistory");
        EnvPhotoUI = GenerationEnvHistory.GetComponent<Image>();

        NewGenerationScreen = GameObject.Find("NewGenerationScreen");
        NewGenerationText = GameObject.Find("NewGenerationText").transform.Find("Text").gameObject.GetComponent<Text>();
        NewGenerationButton = NewGenerationScreen.transform.Find("NewGenerationButton").gameObject.GetComponent<Button>();


        passRoundButton = GameObject.Find("passRoundButton").GetComponent<Button>();
        passRoundButton.gameObject.SetActive(false);

        newRoundButton = GameObject.Find("newRoundButton").GetComponent<Button>();

        waitingForPlayers = GameObject.Find("WaitingForPlayers");

        poppupScreen = GameObject.Find("PoppupScreen");

        BetweenRoundScreen = GameObject.Find("BetweenRoundScreen");
        IntroductionScreen = GameObject.Find("IntroductionScreen");
        StartGameButtonObject = GameObject.Find("StartGameButtonObject");
        NextInstructionsButton = StartGameButtonObject.transform.Find("NextInstructionsButton").gameObject.GetComponent<Button>();

        InformationScreen = GameObject.Find("InformationScreen");
        PassInformationButton = InformationScreen.transform.Find("PassInformationButton").gameObject.GetComponent<Button>();
        InformationGen = InformationScreen.transform.Find("InformationGen").gameObject.GetComponent<Image>();



        if (GameGlobals.firstGeneration)
        {
            GenerationText.SetActive(false);
            GenerationEnvHistory.SetActive(false);
            StartGameButtonObject.SetActive(true);
            FirstGenerationTextUI.text = "Hello!\n" + "You are the first generation in this planet.\n" + "The game will start with average resources " + GameGlobals.envState.ToString() + ".\n";
        }
        else
        {
            FirstGenerationText.SetActive(false);
            GenerationEnvHistory.SetActive(false);
            StartGameButtonObject.SetActive(false);
            EnvPhotoUI.sprite = Resources.Load<Sprite>("Textures/Generation/Information1");
            GenerationTextUI.text = "Hello!\n" + "You are an intermediate generation in this planet.\n" + "The previous Generations preserved the planet until this generation.\n You can see how the previous generations played this game.";// + "Take from Common-Pool but if you take too much there will be no next generation.";
        }


        GenerationNumberText = GameObject.Find("GenerationNumber");
        GenerationNumberTextUI = GenerationNumberText.transform.Find("genNumText").gameObject.GetComponent<Text>();
        GenPhotoUI = GenerationNumberText.transform.Find("GenPhoto").gameObject.GetComponent<Image>();
        if (GameGlobals.firstGeneration){ GenerationNumberTextUI.text = "First Generation";
            firstGenPhoto = Resources.Load<Sprite>("Textures/Generation/1stgen_icon");
        }
        else { GenerationNumberTextUI.text = "Intermediate Generation";
            firstGenPhoto = Resources.Load<Sprite>("Textures/Generation/2ndgen_icon");
        }      
        
        GenPhotoUI.sprite = firstGenPhoto;

        UIroundSum = GameObject.Find("roundSumUI");
        roundSumTex = UIroundSum.transform.Find("roundSumTex").gameObject.GetComponent<Text>();

        ImpactCP = GameObject.Find("ImpactCP");
        ImpactCPtext = ImpactCP.transform.Find("Text").gameObject.GetComponent<Text>();

        int numPlayers = GameGlobals.players.Count;

        // @jbgrocha: Auto Continue for Batch Mode
        Player currPlayer = null;
        if (GameGlobals.isSimulation)
        {
            for (int i = 0; i < numPlayers; i++)
            {
                currPlayer = GameGlobals.players[i];
                currPlayer.ReceiveGameManager(this);
                StartCoroutine(currPlayer.SetMoney(0.1f));
            }

            tutorialScreens.SetActive(false);
            GenerationText.SetActive(false);
            GenerationEnvHistory.SetActive(false);
            waitingForPlayers.SetActive(false);
            UIroundSum.SetActive(false);
            poppupScreen.SetActive(false);
            BetweenRoundScreen.SetActive(false);
            NewGenerationScreen.SetActive(false);
            InformationScreen.SetActive(false);
            StartGameRoundForAllPlayers();
        }
        else
        {

            for (int i = 0; i < numPlayers; i++)
            {
                currPlayer = GameGlobals.players[i];
                currPlayer.ReceiveGameManager(this);
                StartCoroutine(currPlayer.SetMoney(0.1f));




                //Setup warnings
                currPlayer.GetWarningScreenRef().AddOnShow(InterruptGame);
                currPlayer.GetWarningScreenRef().AddOnHide(ContinueGame);
            }
            //Set Players faces
            GameGlobals.players[0].SetFace(Resources.Load<Sprite>("Textures/Generation/player_icon"));
            GameGlobals.players[1].SetFace(Resources.Load<Sprite>("Textures/Generation/player_icon_red"));
            GameGlobals.players[2].SetFace(Resources.Load<Sprite>("Textures/Generation/player_icon_blue"));

            envDynamicSlider = new DynamicSlider(environmentSliderSceneElement.gameObject, true, true);
            StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState, true));
            DontDestroyOnLoad(CommonAreaUI);

            rollDiceOverlay.SetActive(false);
            poppupScreen.SetActive(false);
            BetweenRoundScreen.SetActive(false);

            //start tutoriala
            tutorialScreens.SetActive(true);
            tutorialScreen.SetActive(true);
            tutorialScreen2.SetActive(false);
            tutorialScreen3.SetActive(false);
            tutorialScreen4.SetActive(false);
            tutorialScreen5.SetActive(false);
            tutorialScreen6.SetActive(false);
            tutorialScreen7.SetActive(false);
            waitingForPlayers.SetActive(false);
            UIroundSum.SetActive(false);
            NewGenerationScreen.SetActive(false);
            InformationScreen.SetActive(false);


            NextInstructionsButton.onClick.AddListener(delegate ()
            {
                IntroductionScreen.SetActive(false);
                StartCoroutine(WaitingForAIPlayers());
            });

            TenthGenButton.onClick.AddListener(delegate ()
            {
                GenerationText.SetActive(false);
                GenerationEnvHistory.SetActive(true);
                StartGameButtonObject.SetActive(true);
            });

            advanceTutorialButton.onClick.AddListener(delegate ()
            {
                if (tutorialAux == 1)
                {
                    tutorialScreen.SetActive(false);
                    tutorialScreen2.SetActive(true);
                    UIroundSum.SetActive(false);
                    waitingForPlayers.SetActive(false);
                    if (GameGlobals.firstGeneration) { GenerationEnvHistory.SetActive(false); }
                    else { GenerationEnvHistory.SetActive(true); }
                    ImpactCP.SetActive(false);
                    tutorialAux = 2;
                }
                else if (tutorialAux == 2)
                {
                    tutorialScreen2.SetActive(false);
                    tutorialScreen3.SetActive(true);
                    tutorialAux = 3;
                }
                else if (tutorialAux == 3)
                {
                    tutorialScreen3.SetActive(false);
                    tutorialScreen4.SetActive(true);
                    tutorialAux = 4;
                }
                else if (tutorialAux == 4)
                {
                    tutorialScreen4.SetActive(false);
                    tutorialScreen5.SetActive(true);
                    tutorialAux = 5;
                }
                else if (tutorialAux == 5)
                {
                    tutorialScreen5.SetActive(false);
                    tutorialScreen6.SetActive(true);
                    tutorialAux = 6;
                }
                else if (tutorialAux == 6)
                {
                    tutorialScreen6.SetActive(false);
                    tutorialScreen7.SetActive(true);
                    tutorialAux = 7;
                    advanceTutorialButton.GetComponentInChildren<Text>().text = "Finish Tutorial";
                }
                else if (tutorialAux == 7)
                {
                    tutorialScreen7.SetActive(false);
                    tutorialScreens.SetActive(false);
                    GenerationEnvHistory.SetActive(false);
                    tutorialAux = 8;
                }
            });

            newRoundButton.onClick.AddListener(delegate ()
            {
                BetweenRoundScreen.SetActive(false);
                if (GameGlobals.firstGeneration &&  GameGlobals.generation == 9)
                {
                    GameGlobals.currGameState = GameProperties.GameState.VICTORY;
                    GameGlobals.gameSceneManager.LoadEndScene();
                }
                else if (!GameGlobals.firstGeneration && GameGlobals.generation == 12)
                {
                    GameGlobals.currGameState = GameProperties.GameState.VICTORY;
                    GameGlobals.gameSceneManager.LoadEndScene();
                }
                else
                {
                    ResetCommonPot();
                }
            });

            NewGenerationButton.onClick.AddListener(delegate ()
            {
                NewGenerationScreen.SetActive(false);
                
                if(GameGlobals.informationOfGeneration == false) { 
                    if (GameGlobals.ruinedPlanet && GameGlobals.generation == 4)
                    {
                        GameGlobals.currGameRoundId++;
                        GameGlobals.envStatePerRound.Add(0);
                        foreach (Player p in GameGlobals.players)
                        {
                            p.environmentInvestmentPerRound.Add(0);
                        }
                        ResetCommonPot();
                    }
                    else
                    {
                        StartGameRoundForAllPlayers();
                    }
                }
                else
                {
                    ChangeGraphInformation();
                    InformationScreen.SetActive(true);
                }
            });

            PassInformationButton.onClick.AddListener(delegate ()
            {
                InformationScreen.SetActive(false);
                if (GameGlobals.ruinedPlanet && GameGlobals.generation == 4)
                {
                    GameGlobals.currGameRoundId++;
                    GameGlobals.envStatePerRound.Add(0);
                    foreach (Player p in GameGlobals.players)
                    {
                        p.environmentInvestmentPerRound.Add(0);
                    }
                    ResetCommonPot();
                }
                else
                {
                    StartGameRoundForAllPlayers();
                }
            });

            simulateEvolutionButton.onClick.AddListener(delegate ()
            {
                simulateInvestmentScreen.SetActive(false);
                StartInvestmentSimulationPhase();
            });
            simulateInvestmentScreen.SetActive(false);

            //this init is not nice
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button button in allButtons)
            {
                button.onClick.AddListener(delegate () { GameGlobals.audioManager.PlayClip("Audio/snap"); });
            }

            if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
            {
                StartCoroutine(GameGlobals.narrator.GameStart());
            }

            ContinueGame();
        }
    }


    private IEnumerator BudgetExecutionPhase(Player currPlayer)
    {
        var currInvestment = currPlayer.GetCurrRoundInvestment();
        int numTokensForEconomy = currInvestment[GameProperties.InvestmentTarget.ECONOMIC];
        int numTokensForEnvironment = currInvestment[GameProperties.InvestmentTarget.ENVIRONMENT];

        //roll dice for GameProperties.InvestmentTarget.ECONOMIC
        string diceOverlayTitle = currPlayer.GetName() + " rolling " + numTokensForEconomy + " dice for economic growth ...";
        yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, numTokensForEconomy));

        int economyResult = diceManager.GetCurrDiceTotal();
        float economicIncrease = (float)economyResult / 100.0f;

        yield return StartCoroutine(currPlayer.SetEconomicResult(economicIncrease));
        //        Debug.Log("economicIncrease: "+economicIncrease);

        if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
        {
            yield return GameGlobals.narrator.EconomyBudgetExecution(currPlayer, GameGlobals.currGameRoundId, economyResult);
        }


        //roll dice for GameProperties.InvestmentTarget.ENVIRONMENT       
        diceOverlayTitle = currPlayer.GetName() + " rolling " + numTokensForEnvironment + " dice for environment ...";
        yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, numTokensForEnvironment));

        int environmentResult = diceManager.GetCurrDiceTotal();
        float envIncrease = (float)environmentResult / 100.0f;
        //        Debug.Log("envIncrease: "+envIncrease);

        GameGlobals.envState = Mathf.Clamp01(GameGlobals.envState + envIncrease);
        if (!GameGlobals.isSimulation)
        {
            yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(
                envDynamicSlider.UpdateSliderValue(GameGlobals.envState, true));
        }

        currPlayer.SetEnvironmentResult(envIncrease);

        if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
        {
            yield return GameGlobals.narrator.EnvironmentBudgetExecution(currPlayer, GameGlobals.currGameRoundId,
                environmentResult);
        }

        // Narrator (function is ienumerator)
        // Should Receive a Player (or results)
        // Should have 2 calls, one for environment and one for economy???
        //yield return GameGlobals.narrator.BudgetExecution(currPlayer, GameGlobals.currGameRoundId, economyResult, environmentResult);
    }

    private IEnumerator YieldedGameUpdateLoop()
    {
        //avoid rerun in this case because load scene is asyncronous
        if (this.gameMainSceneFinished || this.interruptionRequests > 0)
        {
            yield return null;
        }


        //end of first phase; trigger second phase
        if (numPlayersToAllocateBudget == 0)
        {
            currGamePhase = GameProperties.GamePhase.HISTORY_DISPLAY;
            StartCoroutine(WaitingForAIPlayersToPlay());
            poppupScreen.SetActive(false);

            StartDisplayHistoryPhase();
            numPlayersToAllocateBudget = GameGlobals.players.Count;
        }

        //end of second phase;
        if (numPlayersToDisplayHistory == 0)
        {
            numPlayersToDisplayHistory = GameGlobals.players.Count;

            numPlayersToDisplayHistory = GameGlobals.players.Count;

            foreach (Player player in GameGlobals.players)
            {
                if (player.GetPlayerType() == "HUMAN") {
                    Dictionary<string, string> logEntry = new Dictionary<string, string>()
                    {
                        {"currSessionId", GameGlobals.currSessionId},
                        {"currGameId", GameGlobals.conditionTag.ToString()},
                        //{"currRoundId", GameGlobals.currGameRoundId.ToString()},
                        {"generation", GameGlobals.generation.ToString()},
                        {"playerName", GameGlobals.workerId },
                        {"playerType", player.GetPlayerType()},
                        //{"playerCurrInvestEcon", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC].ToString()},
                        {"playerTookFromCP", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString()},
                        {"playerGain", player.GetGains().ToString()},
                        {"nCollaboration", player.GetNCollaboration().ToString()},
                        {"envState", Convert.ToInt32(GameGlobals.envState).ToString()}
                    };
                    GameGlobals.callMongoLogServer.SentLog(logEntry);
                }
                else
                {
                    Dictionary<string, string> logEntry = new Dictionary<string, string>()
                    {
                        {"currSessionId", GameGlobals.currSessionId},
                        {"currGameId", GameGlobals.conditionTag.ToString()},
                        //{"currRoundId", GameGlobals.currGameRoundId.ToString()},
                        {"generation", GameGlobals.generation.ToString()},
                        {"playerName", player.GetName()},
                        {"playerType", player.GetPlayerType()},
                        //{"playerCurrInvestEcon", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC].ToString()},
                        {"playerTookFromCP", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString()},
                        {"playerGain", player.GetGains().ToString()},
                        {"nCollaboration", player.GetNCollaboration().ToString()},
                        {"envState", Convert.ToInt32(GameGlobals.envState).ToString()}
                    };
                    GameGlobals.callMongoLogServer.SentLog(logEntry);
                }

            }


            //TakeMoneyFromCommonPot();
            TakeFromCommonPot();
            GameGlobals.currGameRoundId++;
            yield return new WaitForSeconds(1);
            passRoundButton.gameObject.SetActive(true);
            passRoundButton.onClick.RemoveAllListeners();
            passRoundButton.onClick.AddListener(delegate ()
            {
                //RenewCommonPool(true);
                if (this.stopbugButton)
                { //DO NOTHING}
                    }
                /*else if (GameGlobals.currGameRoundId == GameProperties.configurableProperties.maxNumRounds && GameGlobals.envState <= GameGlobals.envThreshold) //environment exploded
                    {
                    this.stopbugButton = true;
                    GameGlobals.currGameState = GameProperties.GameState.LOSS;
                    GameGlobals.gameSceneManager.LoadEndScene();
                    UIroundSum.SetActive(false);
                }
                else if (GameGlobals.currGameRoundId == GameProperties.configurableProperties.maxNumRounds) //reached last round
                    {
                    this.stopbugButton = true;
                    GameGlobals.currGameState = GameProperties.GameState.VICTORY;
                    GameGlobals.gameSceneManager.LoadEndScene();
                    UIroundSum.SetActive(false);
                }*/
                //normal round finished
                else {
                    victoryOverlayUI.SetActive(false);
                    lossOverlayUI.SetActive(false);
                    //RenewCommonPool(true);
                    currGamePhase = GameProperties.GamePhase.BUDGET_ALLOCATION;
                    if (GenerationResult) { victoryOverlayUI.SetActive(true); }
                    else { lossOverlayUI.SetActive(true); }
                    BetweenRoundScreen.SetActive(true);
                    UIroundSum.SetActive(false);
                    GenerationEnvHistory.SetActive(false);
                    GenerationText.SetActive(false);
                }

                passRoundButton.gameObject.SetActive(false);
            });


        }
        //-----------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------//
        //-----------------------------------------------------------------------------------------------//
        //end of third phase
        if (numPlayersToExecuteBudget == 0)
        {
            currGamePhase = GameProperties.GamePhase.INVESTMENT_SIMULATION;

            simulateInvestmentScreen.SetActive(true); //StartInvestmentSimulationPhase(); is called in this screen
            numPlayersToExecuteBudget = GameGlobals.players.Count;

            if (GameGlobals.isSimulation)
            {
                simulateInvestmentScreen.SetActive(false);
                StartInvestmentSimulationPhase();
            }
        }

        //end of forth phase
        if (numPlayersToSimulateInvestment == 0)
        {
            currGamePhase = GameProperties.GamePhase.BUDGET_ALLOCATION;
            numPlayersToSimulateInvestment = GameGlobals.players.Count;

            string diceOverlayTitle = "Simulating environment decay ...";
            yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, UnityEngine.Random.Range(GameGlobals.environmentDecayBudget[0], GameGlobals.environmentDecayBudget[1] + 1)));

            int environmentDecay = diceManager.GetCurrDiceTotal();
            float envDecay = (float)environmentDecay / 100.0f;
            //            Debug.Log("envDecay: "+envDecay);

            GameGlobals.envState = Mathf.Clamp01(GameGlobals.envState - envDecay);
            if (!GameGlobals.isSimulation)
            {
                yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState, true));
            }

            if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
            {
                StartCoroutine(GameGlobals.narrator.EnvironmentDecaySimulation(GameGlobals.currGameRoundId, environmentDecay));
            }

            foreach (Player player in GameGlobals.players)
            {
                diceOverlayTitle = "Simulating economic decay ...";
                yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, UnityEngine.Random.Range(GameGlobals.playerDecayBudget[0], GameGlobals.playerDecayBudget[1] + 1)));

                int economyDecay = diceManager.GetCurrDiceTotal();
                float economicDecay = (float)economyDecay / 100.0f;
                StartCoroutine(player.SetEconomicDecay(economicDecay));
                //                Debug.Log("economicDecay: "+economicDecay);

                if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
                {
                    StartCoroutine(GameGlobals.narrator.EconomyDecaySimulation(player, GameGlobals.currGameRoundId, economyDecay));
                }
            }

            if (!GameGlobals.isSimulation)
            {
                yield return new WaitForSeconds(phaseEndDelay);
            }

            //check for game end to stop the game instead of loading new round
            if (GameGlobals.envState <= 0.001f)
            {
                GameGlobals.currGameState = GameProperties.GameState.LOSS;
                //ONHOLD @jbgrocha: Send GAME LOSS Event to AIPlayers (call AIplayer update emotional module function)
            }
            else
            {
                if (GameGlobals.currGameRoundId > GameProperties.configurableProperties.maxNumRounds - 1)
                {
                    GameGlobals.currGameState = GameProperties.GameState.VICTORY;
                    // ONHOLD @jbgrocha: Send GAME Victory Event to AIPlayers (call AIplayer update emotional module function)
                }
                Debug.Log("[Game: " + GameGlobals.currGameId + "; Round: " + (GameGlobals.currGameRoundId + 1) + " of " + GameProperties.configurableProperties.maxNumRounds + "]");
            }


            foreach (Player player in GameGlobals.players)
            {
                Dictionary<string, string> logEntry = new Dictionary<string, string>()
                {
                    {"sessionId", GameGlobals.currSessionId},
                    {"gameId", GameGlobals.currGameId.ToString()},
                    {"roundId", GameGlobals.currGameRoundId.ToString()},
                    {"condition", GameProperties.currSessionParameterization.id},
                    {"gameState", GameGlobals.currGameState.ToString()},


                    {"envState", GameGlobals.envState.ToString()},

                    {"playerId", player.GetId().ToString()},
                    {"playerType", player.GetPlayerType()},

                    {"playerInvestEcon", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC].ToString()},
                    {"playerInvestEnv", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString()},

                    {"playerInvHistEcon", player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString()},
                    {"playerInvHistEnv", player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString()},

                    {"playerEconState", player.GetMoney().ToString()}
                };

                StartCoroutine(GameGlobals.gameLogManager.WriteToLog("fortheplanetlogs", "gameresultslog", logEntry));
            }

            if (GameGlobals.currGameState != GameProperties.GameState.NOT_FINISHED)
            {
                GameGlobals.gameSceneManager.LoadEndScene();
            }
            else
            {
                GameGlobals.currGameRoundId++;
                if (GameGlobals.isSimulation)
                {
                    StartGameRoundForAllPlayers();
                }
                else
                {//rever acho que nao necessita do if
                    tutorialScreens.SetActive(true);
                    tutorialScreen.SetActive(false);
                    tutorialScreen2.SetActive(false);
                    tutorialScreen3.SetActive(false);
                    tutorialScreen4.SetActive(false);
                    tutorialScreen5.SetActive(false);
                    tutorialScreen6.SetActive(false);
                    tutorialScreen7.SetActive(false);

                }
            }
        }
    }

    // Run update or fixed update if is or not simulation mode    
    public void Update()
    {
        if (GameGlobals.isSimulation)
        {
            StartCoroutine(YieldedGameUpdateLoop());

        }
    }
    public void FixedUpdate()
    {
        if (!GameGlobals.isSimulation)
        {
            StartCoroutine(YieldedGameUpdateLoop());

        }
    }

    public void StartGameRoundForAllPlayers()
    {
        int numPlayers = GameGlobals.players.Count;
        /*foreach (Player player in GameGlobals.players)
        {
            List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();

            //players perceive round start
            events.Add(RolePlayCharacter.EventHelper.ActionEnd("World", "State(Round,Start)", player.GetName()));
            player.Perceive(events);
        }*/

        if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
        {
            StartCoroutine(GameGlobals.narrator.RoundStart());
        }
        StartAlocateBudgetPhase();

    }



    //------------------------------------------Requests---------------------------------------
    public void StartAlocateBudgetPhase()
    {
        if (!GameGlobals.isSimulation)
        {
            foreach (Player player in GameGlobals.players)
            {
                player.ResetPlayerUI();
            }
            ChangeActivePlayerUI(GameGlobals.players[0]);
        }
        GameGlobals.players[0].BudgetAllocationPhaseRequest();
    }
    public void StartDisplayHistoryPhase()
    {
        //this phase displays the history of all players
        foreach (Player player in GameGlobals.players)
        {
            if (!GameGlobals.isSimulation)
            {
                player.ResetPlayerUI();
            }
            player.HistoryDisplayPhaseRequest();

            //Fatima updates
            //players see the history of each other
            /*List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + player.GetName() + ", Environment)", player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0.00", CultureInfo.InvariantCulture), "World"));
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + player.GetName() + ", Economic)", player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0.00", CultureInfo.InvariantCulture), "World"));
            foreach (Player otherPlayer in GameGlobals.players)
            {
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + otherPlayer.GetName() + ", Environment)", otherPlayer.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0", CultureInfo.InvariantCulture), "World"));
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + otherPlayer.GetName() + ", Economic)", otherPlayer.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0", CultureInfo.InvariantCulture), "World"));
            }
            player.Perceive(events);*/

            if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
            {
                StartCoroutine(GameGlobals.narrator.DisplayHistory(player, GameGlobals.currGameRoundId));
            }
        }
    }
    public void StartExecuteBudgetPhase()
    {
        if (!GameGlobals.isSimulation)
        {
            foreach (Player player in GameGlobals.players)
            {
                player.ResetPlayerUI();
            }
            ChangeActivePlayerUI(GameGlobals.players[0]);
        }
        GameGlobals.players[0].BudgetExecutionPhaseRequest();
    }

    public void StartInvestmentSimulationPhase()
    {
        //this phase simulates the evolution of all players
        foreach (Player player in GameGlobals.players)
        {
            if (!GameGlobals.isSimulation)
            {
                player.ResetPlayerUI();
            }
            player.InvestmentSimulationRequest();
        }
    }


    //------------------------------------------Responses---------------------------------------
    public IEnumerator BudgetAllocationPhaseResponse(Player invoker)
    {
        /*
         * 
        var watch = System.Diagnostics.Stopwatch.StartNew();
        // the code that you want to measure comes here
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        */

        Player currPlayer = GameGlobals.players[currPlayerIndex];
        Player nextPlayer = ChangeToNextPlayer(currPlayer);
        poppupScreen.SetActive(true);

        numPlayersToAllocateBudget--;

        if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
        {
            StartCoroutine(GameGlobals.narrator.BudgetAllocation(currPlayer, GameGlobals.currGameRoundId));
        }

        if (numPlayersToAllocateBudget > 0)
        {
            nextPlayer.BudgetAllocationPhaseRequest();
        }
        yield return null;
    }
    public IEnumerator HistoryDisplayPhaseResponse(Player invoker)
    {
        numPlayersToDisplayHistory--;
        yield return null;
    }
    public IEnumerator BudgetExecutionPhaseResponse(Player invoker)
    {
        Player currPlayer = GameGlobals.players[currPlayerIndex];
        yield return StartCoroutine(BudgetExecutionPhase(currPlayer));
        Player nextPlayer = ChangeToNextPlayer(currPlayer);

        numPlayersToExecuteBudget--;
        if (numPlayersToExecuteBudget > 0)
        {
            nextPlayer.BudgetExecutionPhaseRequest();
        }
    }
    public IEnumerator InvestmentSimulationPhaseResponse(Player invoker)
    {
        numPlayersToSimulateInvestment--;
        yield return null;
    }


    public Player ChangeToNextPlayer(Player currPlayer)
    {
        currPlayerIndex = (currPlayerIndex + 1) % GameGlobals.players.Count;
        Player nextPlayer = GameGlobals.players[currPlayerIndex];
        if (!GameGlobals.isSimulation)
        {
            ChangeActivePlayerUI(nextPlayer);
        }
        return nextPlayer;
    }


    public GameProperties.GamePhase GetCurrGamePhase()
    {
        return this.currGamePhase;
    }

    private IEnumerator ChangeActivePlayerUI(Player player)
    {
        player.GetPlayerUI().transform.SetAsLastSibling();
        int numPlayers = GameGlobals.players.Count;
        for (int i = 0; i < numPlayers; i++)
        {
            if (GameGlobals.players[i] == player)
            {
                player.GetPlayerMarkerUI().SetActive(true);
                player.GetPlayerDisablerUI().SetActive(true);
                continue;
            }
            Player currPlayer = GameGlobals.players[i];
            currPlayer.GetPlayerMarkerUI().SetActive(false);
            currPlayer.GetPlayerDisablerUI().SetActive(false);
        }
        return null;
    }


    private void ChangePhotoinGenPhoto(int genNumber)
    {
    switch (genNumber){
        case 1:
             GenPhotoUI.sprite = firstGenPhoto;
                break;
        case 4:
             GenPhotoUI.sprite = Resources.Load<Sprite>("Textures/Generation/2ndgen_icon");
             break;
        case 7:
             GenPhotoUI.sprite = Resources.Load<Sprite>("Textures/Generation/3rdgen_icon");
             break;
        case 9:
             if (GameGlobals.firstGeneration) { GenPhotoUI.sprite = Resources.Load<Sprite>("Textures/Generation/5thgen_icon"); }
             else { GenPhotoUI.sprite = Resources.Load<Sprite>("Textures/Generation/4thgen_icon"); }         
             break;
        case 12:
             GenPhotoUI.sprite = Resources.Load<Sprite>("Textures/Generation/5thgen_icon");
             break;
            }
    }


    private void TakeFromCommonPot()
    {
        foreach (Player p in GameGlobals.players)
        {
            GameGlobals.envState -= p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
            this.roundTaken += p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
        }
        UIroundSum.SetActive(true);
        roundSumTex.text = "Total value to take from the Common-Pool:  " + this.roundTaken.ToString();

        GameGlobals.envStatePerRound.Add(Convert.ToInt32(GameGlobals.envState));

        StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState, true));
        ImpactCP.SetActive(true);

        if (GameGlobals.envState >= GameGlobals.envThreshold) {
            GenerationResult = true;
            ImpactCPtext.GetComponent<Text>().text = "Your group took a total of " + this.roundTaken.ToString() + ", which is less or equal than the threshold of 30 units.\nThe Common-Pool is refilled to 60 units.";
            if (GameGlobals.firstGeneration && GameGlobals.generation == 9) { 
                newRoundButton.GetComponentInChildren<Text>().text = "End Game"; 
            }
            else if (!GameGlobals.firstGeneration && GameGlobals.generation == 12)
            {
                newRoundButton.GetComponentInChildren<Text>().text = "End Game";
            }
        }
        else if(GameGlobals.envState < GameGlobals.envThreshold)
        {
            GenerationResult = false;
            ImpactCPtext.GetComponent<Text>().text = "Your group took a total of " + this.roundTaken.ToString() + ", which is more than the threshold of 30 units.\nThe Common-Pool is permanently destroyed for future generations.";
            if (GameGlobals.firstGeneration && GameGlobals.generation == 9){
                newRoundButton.GetComponentInChildren<Text>().text = "End Game";
            }
            else if (!GameGlobals.firstGeneration && GameGlobals.generation == 12){
                newRoundButton.GetComponentInChildren<Text>().text = "End Game";
            }
        }

        this.roundTaken = 0;
        GameGlobals.diffCP = 0;
        GameGlobals.impactOnCP = "";

    }

    private void ResetCommonPot()
    {
        GameGlobals.generation = NewGeneration();
        GenerationNumberTextUI.text = "Intermediate Generation";
        ChangePhotoinGenPhoto(GameGlobals.generation);

        this.roundTaken = 0;
        GameGlobals.diffCP = 0;
        GameGlobals.impactOnCP = "";

        if(GameGlobals.informationOfGeneration == false) { 
            if (GameGlobals.ruinedPlanet && GameGlobals.generation == 4) {
                GameGlobals.envState = 0;
                NewGenerationText.text = "Hello!\n" + "You are an intermediate generation in this planet.\n" + "Unfortunately the previous Generations destroyed the planet," +
                    "so you will not obtain any bonus in this round.\n";
                NewGenerationButton.GetComponentInChildren<Text>().text = "End Generation";
            } 
            else {
                GameGlobals.envState = 60;
                NewGenerationText.text = "Hello!\n" + "You are an intermediate generation in this planet.\n" + "The previous Generations preserved the planet until this generation.\n";
                NewGenerationButton.GetComponentInChildren<Text>().text = "Start Round";
            }
            InformationScreen.SetActive(false);
        }
        else
        {
            if (GameGlobals.ruinedPlanet && GameGlobals.generation == 4)
            {
                GameGlobals.envState = 0;
                NewGenerationText.text = "Hello!\n" + "You are an intermediate generation in this planet.\n" + "Unfortunately the previous Generations destroyed the planet," +
                    "so you will not obtain any bonus in this round.\n You can see how the previous generations played this game.";
                NewGenerationButton.GetComponentInChildren<Text>().text = "Next";
            }
            else
            {
                GameGlobals.envState = 60;
                NewGenerationText.text = "Hello!\n" + "You are an intermediate generation in this planet.\n" + "The previous Generations preserved the planet until this generation.\n You can see how the previous generations played this game.";
                NewGenerationButton.GetComponentInChildren<Text>().text = "Next";
            }
        }


        StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState, false));

        NewGenerationScreen.SetActive(true);

    }

    private int NewGeneration()
    {
        switch (GameGlobals.generation)
        {
            case 1:
                return 4;
            case 4:
                return 7;
            case 7:
                return 9;
            case 9:
                return 12;
            case 12:
                return 99;
        }
        return 0;
    }
    
    private void ChangeGraphInformation()
    {
        switch (GameGlobals.generation)
        {
            case 4:
                if(GameGlobals.ruinedPlanet == false) { 
                    InformationGen.sprite = Resources.Load<Sprite>("Textures/Generation/Information1");
                }
                else
                {
                    InformationGen.sprite = Resources.Load<Sprite>("Textures/Generation/Information2");
                    PassInformationButton.GetComponentInChildren<Text>().text = "End Generation";
                }
                return;
            case 7:
                InformationGen.sprite = Resources.Load<Sprite>("Textures/Generation/Information3");
                PassInformationButton.GetComponentInChildren<Text>().text = "Start Round" ;
                return;
            case 9:
                InformationGen.sprite = Resources.Load<Sprite>("Textures/Generation/Information4");
                return;
            case 12:
                InformationGen.sprite = Resources.Load<Sprite>("Textures/Generation/Information5");
                return;
        }
    }



    // OLD IMPLEMENTATION com 10 rondas
    /*


        //Take from common-pool what players choose
        private void TakeMoneyFromCommonPot()
        {
            //List<int> votes = new List<int>();
            foreach (Player p in GameGlobals.players)
            {
                GameGlobals.envState -= p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
                this.roundTaken += p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
                //GameGlobals.diffCP -= p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];

                //votes.Add(p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT]);
            }
            //renew envState
            //GameGlobals.diffCP += GameGlobals.envRenewperRound;
            //StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState,false));

            UIroundSum.SetActive(true);
            if(this.roundTaken > 0) {
                roundSumTex.text = "Total value to take from the Common-Pool:  " + this.roundTaken.ToString();/* + "\n" 
                + "Impact on the Common-Pool: " + "+" + Convert.ToInt32(GameGlobals.diffCP).ToString();
            }
            else{
                roundSumTex.text = "Total value to take from the Common-Pool:  " + this.roundTaken.ToString();
            }

            //end generation
            GameGlobals.generation += 1;

            /* Player take medianVote
            int[] sortedVotes = votes.ToArray();
            Array.Sort(sortedVotes);
            int len = sortedVotes.Length;
            float medianVote;
            if (len % 2 == 0)
            {
                int a = sortedVotes[len / 2 - 1];
                int b = sortedVotes[len / 2];
                medianVote = (a + b) / 2;
            }
            else
            {
                medianVote = sortedVotes[len / 2];
            }

            foreach (Player p in GameGlobals.players)
            {
                p.environmentMedianInvestmentPerRound.Add(Convert.ToInt32(medianVote));
                float newMoney = p.GetMoney() + medianVote;
                StartCoroutine(p.SetMoney(newMoney));
            }
            GameGlobals.envState -= (3 * medianVote);
            GameGlobals.envStatePerRound.Add(GameGlobals.envState);
            StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));

        }
        private void RenewCommonPool(Boolean arrows)
        {
            GameGlobals.envRenewperRound = (float)(GameGlobals.envState * GameGlobals.envRenew);
            GameGlobals.diffCP += Convert.ToInt32(GameGlobals.envRenewperRound);
            GameGlobals.envState += GameGlobals.diffCP;

            if (GameGlobals.envState <= 0)
            {
                GameGlobals.envStatePerRound.Add(0);
            }
            else
            {
                GameGlobals.envStatePerRound.Add(Convert.ToInt32(GameGlobals.envState));
            }

            StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState, arrows));
            ImpactCP.SetActive(true);
            ImpactCPtext.GetComponent<Text>().text = "Taken-From the Common-Pool: " + this.roundTaken.ToString() + "\n" +
            "Common-Pool Renewed: " + GameGlobals.diffCP.ToString() + "\n" +
            "Impact on the Common-Pool: " + GameGlobals.impactOnCP;


            this.roundTaken = 0;
            GameGlobals.diffCP = 0;
            GameGlobals.impactOnCP = "";
            GameGlobals.fairRefPoint = Convert.ToInt32(GameGlobals.envState / 9);
            if (GameGlobals.fairRefPoint >= 13) { GameGlobals.fairRefPoint = 13; }
            GameGlobals.maxSelfish = GameGlobals.fairRefPoint * 2;
            if (GameGlobals.maxSelfish > 14) { GameGlobals.maxSelfish = 14; }
        }

       }
    */
}