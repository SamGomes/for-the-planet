using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    
    public GameObject canvas;

    private int numPlayersToAllocateBudget;
    private int numPlayersToExecuteBudget;
    private int numPlayersToDisplayHistory;

    private bool canSelectToCheckAlbumResult;
    private bool canCheckAlbumResult;
    private bool checkedAlbumResult;

    //------------ UI -----------------------------
    public GameObject playerUIPrefab;
    public GameObject investmentUIPrefab;

    public GameObject UInewRoundScreen;
    public Button UIadvanceRoundButton;
    
    public GameObject UIRollDiceForInstrumentOverlay;
    public Animator rollDiceForInstrumentOverlayAnimator;

    public GameObject dice6UI;
    public GameObject dice20UI;

    public GameObject diceArrowPrefab;
    
    public GameObject UIPrototypeArea;

    public GameObject poppupPrefab;
    public PoppupScreenFunctionalities infoPoppupNeutralRef;
    public PoppupScreenFunctionalities infoPoppupLossRef;
    public PoppupScreenFunctionalities infoPoppupWinRef;

    public PoppupScreenFunctionalities endPoppupWinRef;
    public PoppupScreenFunctionalities endPoppupLossRef;

    public Slider environmentSlider;

    private bool gameMainSceneFinished;
    private int interruptionRequests; //changed whenever an interruption occurs (either a poppup, warning, etc.)
    private bool preferredInstrumentsChoosen;

    private bool budgetAllocationResponseReceived;
    private bool historyDisplayResponseReceived;
    private bool budgetExecutionResponseReceived;

    private int currPlayerIndex;
    private int currSpeakingPlayerId;

    private float diceRollDelay;

    private int marketLimit;
    private int currNumberOfMarketDices;

    void Awake()
    {
        GameGlobals.gameManager = this;
        //mock to test
        //new StartScreenFunctionalities().InitGame();
        //GameGlobals.players = new List<Player>(3);
        //GameGlobals.gameDiceNG = new RandomDiceNG();
        //GameGlobals.currSessionId = "0";
        //GameGlobals.currGameId = 0;
        //GameGlobals.currGameRoundId = 0;
    }

    public int InterruptGame()
    {
        Debug.Log("interrupted");
        interruptionRequests++;
        return 0;
    }
    public int ContinueGame()
    {
        Debug.Log("continued");
        interruptionRequests--;
        return 0;
    }

    public void InitGame()
    {
        interruptionRequests = 0;
        InterruptGame(); //interrupt game update while loading...

        budgetAllocationResponseReceived = false;
        historyDisplayResponseReceived = false;
        budgetExecutionResponseReceived = false;
        currPlayerIndex = 0;
        
        //get player poppups (can be from any player) and set methods
        infoPoppupLossRef = new PoppupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab,canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), "Audio/albumLoss");
        infoPoppupWinRef = new PoppupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab,canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), "Audio/albumVictory");
        infoPoppupNeutralRef = new PoppupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab,canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");

        //these poppups load the end scene
        endPoppupLossRef = new PoppupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), delegate() { /*end game*/ if (this.gameMainSceneFinished) GameSceneManager.LoadEndScene(); return 0; });
        endPoppupWinRef = new PoppupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), delegate () { /*end game*/ if (this.gameMainSceneFinished) GameSceneManager.LoadEndScene(); return 0; });

        ChangeActivePlayerUI(GameGlobals.players[0], 2.0f);

        gameMainSceneFinished = false;
        preferredInstrumentsChoosen = false;

        diceRollDelay = 4.0f;

        canCheckAlbumResult = false;
        checkedAlbumResult = false;
        canSelectToCheckAlbumResult = true;
        int numPlayers = GameGlobals.players.Count;

        Player currPlayer = null;
        for (int i = 0; i < numPlayers; i++)
        {
            currPlayer = GameGlobals.players[i];
            currPlayer.ReceiveGameManager(this);
            currPlayer.UpdateMoney(0.1f);

            //Setup warnings
            currPlayer.GetWarningScreenRef().AddOnShow(InterruptGame);
            currPlayer.GetWarningScreenRef().AddOnHide(ContinueGame);
        }

        environmentSlider.value = 0.1f;

        GameGlobals.currGameRoundId = 0; //first round
        GameGlobals.commonEnvironmentInvestment = 0;

        marketLimit = Mathf.FloorToInt(GameProperties.configurableProperties.numberOfAlbumsPerGame * 4.0f / 5.0f) - 1;
        currNumberOfMarketDices = GameProperties.configurableProperties.initNumberMarketDices;

        rollDiceForInstrumentOverlayAnimator = UIRollDiceForInstrumentOverlay.GetComponent<Animator>();

        //this init is not nice
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(delegate () {
                GameGlobals.audioManager.PlayClip("Audio/snap");
            });
        }

        ContinueGame();
    }

    //warning: works only when using human players!
    private IEnumerator ChangeActivePlayerUI(Player player, float delay)
    {
        player.GetPlayerUI().transform.SetAsLastSibling();
        //yield return new WaitForSeconds(delay);
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

    void Start()
    {
        InitGame();

        numPlayersToAllocateBudget = GameGlobals.players.Count;
        numPlayersToExecuteBudget = GameGlobals.players.Count;
        numPlayersToDisplayHistory = GameGlobals.players.Count;

        GameGlobals.currGameState = GameProperties.GameState.NOT_FINISHED;

        //players talk about the initial album
        currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        //foreach (var player in GameGlobals.players)
        //{
        //    player.InformNewAlbum();
        //}

        if (GameProperties.configurableProperties.isSimulation) //start imidiately in simulation
        {
            StartGameRoundForAllPlayers();
        }
        else
        {
            UIadvanceRoundButton.onClick.AddListener(delegate () {
                UInewRoundScreen.SetActive(false);
                StartGameRoundForAllPlayers();
            });
            UIRollDiceForInstrumentOverlay.SetActive(false);
        }
        
    }

    public void StartGameRoundForAllPlayers()
    {
        int numPlayers = GameGlobals.players.Count;
        for (int i = 0; i < numPlayers; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            currPlayer.SetCurrBudget(5);
        }
        StartAlocateBudgetPhase();
    }

    public int RollInvestmentDices(Player currPlayer, GameProperties.InvestmentTarget target)
    {
        var skillSet = currPlayer.GetCurrRoundInvestment();

        int newInvestmentValue = 0;
        int numTokensForTarget = skillSet[target];

        //UI stuff
        UIRollDiceForInstrumentOverlay.transform.Find("title/Text").GetComponent<Text>().text = currPlayer.GetName() + " rolling "+ numTokensForTarget + " dice for " + target.ToString() + " ...";

        int[] rolledDiceNumbers = new int[numTokensForTarget]; //save each rolled dice number to display in the UI

        for (int i = 0; i < numTokensForTarget; i++)
        {
            int randomIncrease = GameGlobals.gameDiceNG.RollTheDice(currPlayer, target, 6, i, numTokensForTarget);
            rolledDiceNumbers[i] = randomIncrease;
            newInvestmentValue += randomIncrease;
        }
       
        string arrowText = "";
        if(target == GameProperties.InvestmentTarget.ECONOMIC)
        {
            arrowText = "+ " + newInvestmentValue * GameProperties.configurableProperties.marketingPointValue + " $";
        }
        else
        {
            arrowText = "+ " + newInvestmentValue + " 🍃";
        }
        
        StartCoroutine(PlayDiceUIs(currPlayer, newInvestmentValue, rolledDiceNumbers, 6, dice6UI, "Animations/RollDiceForInstrumentOverlay/dice6/sprites_3/endingAlternatives/", Color.yellow, arrowText, diceRollDelay));

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), currPlayer.GetId().ToString(), currPlayer.GetName().ToString(), "ROLLED_INVESTMENT_TARGET_DICES", "-", numTokensForTarget.ToString());
        return newInvestmentValue;
    }

    private IEnumerator BudgetExecutionPhase(Player currPlayer)
    {
        float playerEconomicGrowth = 0.0f;
        float playerEnvironmentContribution = 0.0f;
        yield return playerEconomicGrowth = RollInvestmentDices(currPlayer, GameProperties.InvestmentTarget.ECONOMIC);
        yield return playerEnvironmentContribution = RollInvestmentDices(currPlayer, GameProperties.InvestmentTarget.ENVIRONMENT);

        environmentSlider.value += playerEnvironmentContribution/100;
        currPlayer.UpdateMoney(currPlayer.GetMoney() + playerEnvironmentContribution/100);
    }

    private IEnumerator PlayDiceUIs(Player diceThrower, int totalDicesValue, int[] rolledDiceNumbers, int diceNum, GameObject diceImagePrefab, string diceNumberSpritesPath, Color diceArrowColor, string diceArrowText, float delayToClose)
    //the sequence number aims to void dice overlaps as it represents the order for which this dice is going to be rolled. We do not want to roll a dice two times for the same place
    {
        InterruptGame();
        UIRollDiceForInstrumentOverlay.SetActive(true);
        List<GameObject> diceUIs = new List<GameObject>();

        int numDiceRolls = rolledDiceNumbers.Length;
        for (int i = 0; i < numDiceRolls; i++)
        {
            int currDiceNumber = rolledDiceNumbers[i];
            Sprite currDiceNumberSprite = Resources.Load<Sprite>(diceNumberSpritesPath + currDiceNumber);
            if (currDiceNumberSprite == null)
            {
                Debug.Log("cannot find sprite for dice number " + currDiceNumber);
            }
            else
            {
                GameObject diceUIClone = Instantiate(diceImagePrefab, UIRollDiceForInstrumentOverlay.transform);
                diceUIs.Add(diceUIClone);
                StartCoroutine(PlayDiceUI(diceUIClone, diceThrower, numDiceRolls, i, diceNum, currDiceNumberSprite, delayToClose));
            }
        }

        while (!rollDiceForInstrumentOverlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            yield return null;
        }

        rollDiceForInstrumentOverlayAnimator.speed = 0;
        
        //get and disable arrow animation until end of dice animation
        GameObject diceArrowClone = Instantiate(diceArrowPrefab, UIRollDiceForInstrumentOverlay.transform);
        diceArrowClone.GetComponentInChildren<Image>().color = diceArrowColor;
        
        Text arrowText = diceArrowClone.GetComponentInChildren<Text>();
        arrowText.text = diceArrowText;
        arrowText.color = diceArrowColor;

        yield return new WaitForSeconds(delayToClose);

        //players see the dice result
        currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        //foreach (var player in GameGlobals.players)
        //{
        //    player.InformRollDicesValue(diceThrower, numDiceRolls * diceNum, totalDicesValue); //max value = the max dice number * number of rolls
        //}

        rollDiceForInstrumentOverlayAnimator.speed = 1;
        while (!rollDiceForInstrumentOverlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle2"))
        {
            yield return null;
        }

        //destroy arrows, dice images and finally set screen active to false
        Destroy(diceArrowClone);
        for(int i=0; i<diceUIs.Count; i++)
        {
            GameObject currDice = diceUIs[i];
            Destroy(currDice);
        }

        ContinueGame();
        UIRollDiceForInstrumentOverlay.SetActive(false);
    }

    private IEnumerator PlayDiceUI(GameObject diceUIClone, Player diceThrower, int numDicesInThrow, int sequenceNumber, int diceNum, Sprite currDiceNumberSprite, float delayToClose)
    //the sequence number aims to void dice overlaps as it represents the order for which this dice is going to be rolled. We do not want to roll a dice two times for the same place
    {
        Image diceImage = diceUIClone.GetComponentInChildren<Image>();
        Animator diceAnimator = diceImage.GetComponentInChildren<Animator>();

        float translationFactorX = Screen.width * 0.04f;
        float translationFactorY = Screen.width * 0.02f;
        diceUIClone.transform.Translate(new Vector3(Random.Range(-translationFactorX, translationFactorY), Random.Range(-translationFactorX, translationFactorY), 0));
        
        float diceRotation = sequenceNumber * (360.0f / numDicesInThrow);

        diceUIClone.transform.Rotate(new Vector3(0, 0, 1), diceRotation);
        diceImage.overrideSprite = null;
        diceAnimator.Rebind();
        diceAnimator.Play(0);
        diceAnimator.speed = Random.Range(0.8f,1.0f);

        while (!diceAnimator.GetCurrentAnimatorStateInfo(0).IsName("endState"))
        {
            yield return null;
        }
        diceImage.overrideSprite = currDiceNumberSprite;
        
    }
    
    public void UpdateGrowths()
    {
    }

    private void ResetAllPlayers()
    {
        foreach (Player player in GameGlobals.players)
        {
            player.ResetPlayer();
        }
    }

    // wait for all players to exit one phase and start other phase
    void Update () {
        
        //avoid rerun in this case because load scene is asyncronous
        if (this.gameMainSceneFinished || this.interruptionRequests > 0)
        {
            //Debug.Log("pause...");
            return;
        }

        //middle of the phases
        if (budgetAllocationResponseReceived) 
        {
            currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

            budgetAllocationResponseReceived = false;
            Player currPlayer = GameGlobals.players[currPlayerIndex];
            Player nextPlayer = ChangeToNextPlayer(currPlayer);
            //if (numPlayersToLevelUp > 0)
            //{
                //foreach (var player in GameGlobals.players)
                //{
                //    if (player == currPlayer) continue;
                //    player.InformLevelUp(currPlayer, currPlayer.GetLeveledUpInstrument());
                //}
            //}
            numPlayersToAllocateBudget--;
            if (numPlayersToAllocateBudget > 0)
            {
                nextPlayer.BudgetAllocationPhaseRequest();
            }   
        }
        if (historyDisplayResponseReceived)
        {
            currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

            historyDisplayResponseReceived = false;
            Player currPlayer = GameGlobals.players[currPlayerIndex];
            Player nextPlayer = ChangeToNextPlayer(currPlayer);
            //if (numPlayersToPlayForInstrument > 0)
            //{
                //foreach (var player in GameGlobals.players)
                //{
                //    if (player == currPlayer) continue;
                //    player.InformPlayForInstrument(nextPlayer);
                //}
            //}
            numPlayersToDisplayHistory--;
            if (numPlayersToDisplayHistory > 0)
            {
                nextPlayer.HistoryDisplayPhaseRequest();
            }
        }
        if (budgetExecutionResponseReceived)
        {
            currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

            budgetExecutionResponseReceived = false;
            Player currPlayer = GameGlobals.players[currPlayerIndex];
            StartCoroutine(BudgetExecutionPhase(currPlayer));
            Player nextPlayer = ChangeToNextPlayer(currPlayer);
            //if (numPlayersToStartLastDecisions > 0)
            //{
            //foreach (var player in GameGlobals.players)
            //{
            //    if (player == currPlayer) continue;
            //    player.InformLastDecision(nextPlayer);
            //}
            //}
            numPlayersToExecuteBudget--;
            if (numPlayersToExecuteBudget > 0)
            {
                nextPlayer.BudgetExecutionPhaseRequest();
            }
        }

        //end of first phase; trigger second phase
        if (numPlayersToAllocateBudget == 0)
        {
            //Debug.Log("running2...");
            StartDisplayHistoryPhase();
            numPlayersToAllocateBudget = GameGlobals.players.Count;
        }
        
        //end of second phase;
        if (numPlayersToDisplayHistory == 0)
        {
            StartExecuteBudgetPhase();
            numPlayersToAllocateBudget = GameGlobals.players.Count;
        }

        //end of forth phase; trigger and log album result
        if (numPlayersToExecuteBudget == 0)
        {
            UInewRoundScreen.SetActive(true);
            numPlayersToAllocateBudget = GameGlobals.players.Count;
        }

    }


    public void StartAlocateBudgetPhase()
    {
        ResetAllPlayers();
        int numPlayers = GameGlobals.players.Count;
        GameGlobals.players[0].BudgetAllocationPhaseRequest();
    }
    public void StartDisplayHistoryPhase()
    {
        ResetAllPlayers();
        int numPlayers = GameGlobals.players.Count;
        GameGlobals.players[0].HistoryDisplayPhaseRequest();
    }
    public void StartExecuteBudgetPhase()
    {
        ResetAllPlayers();
        int numPlayers = GameGlobals.players.Count;
        GameGlobals.players[0].BudgetExecutionPhaseRequest();
    }


    //------------------------------------------Responses---------------------------------------
    public void BudgetAllocationPhaseResponse(Player invoker)
    {   
        budgetAllocationResponseReceived = true;
    }
    public void HistoryDisplayPhaseResponse(Player invoker)
    {
        historyDisplayResponseReceived = true;
    }
    public void BudgetExecutionPhaseResponse(Player invoker)
    {
        budgetExecutionResponseReceived = true;
    }

    public Player ChangeToNextPlayer(Player currPlayer)
    {
        currPlayerIndex = (currPlayerIndex + 1) % GameGlobals.players.Count;
        Player nextPlayer = GameGlobals.players[currPlayerIndex];
        ChangeActivePlayerUI(nextPlayer, 2.0f);
        return nextPlayer;
    }
    

    public Player GetCurrentPlayer()
    {
        return GameGlobals.players[this.currPlayerIndex];
    }
    public int GetCurrSpeakingPlayerId()
    {
        return this.currSpeakingPlayerId;
    }

}
