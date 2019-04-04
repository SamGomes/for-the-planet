using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    
    public GameObject canvas;

    private int numPlayersToAllocateBudget;
    private int numPlayersToExecuteBudget;
    private int numPlayersToDisplayHistory;
    private int numPlayersToSimulateInvestment;

    //------------ UI -----------------------------
    public GameObject playerUIPrefab;
    public GameObject investmentUIPrefab;

    public GameObject UInewRoundScreen;
    public Button UIadvanceRoundButton;
    
    private GameObject rollDiceForInstrumentOverlayDicesUI;
    private Animator rollDiceForInstrumentOverlayAnimator;
    public GameObject UIRollDiceForInstrumentOverlay;

    public GameObject dice6UI;
    public GameObject dice20UI;

    public GameObject diceArrowPrefab;
    
    public GameObject UIPrototypeArea;

    public GameObject poppupPrefab;
    public PopupScreenFunctionalities infoPoppupNeutralRef;
    public PopupScreenFunctionalities infoPoppupLossRef;
    public PopupScreenFunctionalities infoPoppupWinRef;

    public PopupScreenFunctionalities endPoppupWinRef;
    public PopupScreenFunctionalities endPoppupLossRef;

    public Slider environmentSlider;
    public Button commonSkipPhaseButton;
    public bool isAutomaticPhaseSkipEnabled;

    private bool gameMainSceneFinished;
    private int interruptionRequests; //changed whenever an interruption occurs (either a poppup, warning, etc.)

    private bool budgetAllocationResponseReceived;
    private bool historyDisplayResponseReceived;
    private bool budgetExecutionResponseReceived;
    private bool investmentSimulationResponseReceived;

    private int currPlayerIndex;
    private int currSpeakingPlayerId;

    private float diceRollDelay;

    private int marketLimit;
    private int currNumberOfMarketDices;

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

    public void InitMainGameplay()
    {

        interruptionRequests = 0;
        InterruptGame(); //interrupt game update while loading...

        budgetAllocationResponseReceived = false;
        historyDisplayResponseReceived = false;
        budgetExecutionResponseReceived = false;
        investmentSimulationResponseReceived = false;

        numPlayersToAllocateBudget = GameGlobals.players.Count;
        numPlayersToExecuteBudget = GameGlobals.players.Count;
        numPlayersToDisplayHistory = GameGlobals.players.Count;
        numPlayersToSimulateInvestment = GameGlobals.players.Count;

        currPlayerIndex = 0;
        
        //get player poppups (can be from any player) and set methods
        infoPoppupLossRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab,canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), "Audio/albumLoss");
        infoPoppupWinRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab,canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), "Audio/albumVictory");
        infoPoppupNeutralRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab,canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");

        //these poppups load the end scene
        endPoppupLossRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), delegate() { /*end game*/ if (this.gameMainSceneFinished) GameSceneManager.LoadEndScene(); return 0; });
        endPoppupWinRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), delegate () { /*end game*/ if (this.gameMainSceneFinished) GameSceneManager.LoadEndScene(); return 0; });

        ChangeActivePlayerUI(GameGlobals.players[0], 2.0f);

        gameMainSceneFinished = false;
        diceRollDelay = 4.0f;
        
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

        StartCoroutine(AuxiliaryMethods.UpdateSliderValue(environmentSlider, 0.1f));

        GameGlobals.currGameRoundId = 0; //first round
        GameGlobals.commonEnvironmentInvestment = 0;

        marketLimit = Mathf.FloorToInt(GameProperties.configurableProperties.numberOfAlbumsPerGame * 4.0f / 5.0f) - 1;
        currNumberOfMarketDices = GameProperties.configurableProperties.initNumberMarketDices;

        rollDiceForInstrumentOverlayAnimator = UIRollDiceForInstrumentOverlay.GetComponent<Animator>();
        rollDiceForInstrumentOverlayDicesUI = UIRollDiceForInstrumentOverlay.transform.Find("diceUIs").gameObject;

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
        GameGlobals.gameManager = this;
        InitMainGameplay();
        GameGlobals.currGameState = GameProperties.GameState.NOT_FINISHED;

        //players talk about the initial album
        currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        
        UIadvanceRoundButton.onClick.AddListener(delegate () {
            UInewRoundScreen.SetActive(false);
            StartGameRoundForAllPlayers();
        });
        UIRollDiceForInstrumentOverlay.SetActive(false);
    }

    public void StartGameRoundForAllPlayers()
    {
        int numPlayers = GameGlobals.players.Count;
        for (int i = 0; i < numPlayers; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            currPlayer.ResetPlayerUI();
            currPlayer.SetCurrBudget(5);
        }
        StartAlocateBudgetPhase();
    }


    public IEnumerator RollInvestmentDices(Player currPlayer, GameProperties.InvestmentTarget target)
    {
        var currInvestment = currPlayer.GetCurrRoundInvestment();

        int newInvestmentValue = 0;
        int numTokensForTarget = currInvestment[target];
        if (numTokensForTarget == 0) //there are no dices to roll so return imediately
        {
            yield break;
        }

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
            arrowText = "+ " + newInvestmentValue + " Economic Growth";
        }
        else
        {
            arrowText = "+ " + newInvestmentValue + " Environment growth";
        }

        yield return StartCoroutine(PlayDiceUIs(currPlayer, newInvestmentValue, rolledDiceNumbers, 6, dice6UI, "Animations/RollDiceForInstrumentOverlay/dice6/sprites_3/endingAlternatives/", Color.yellow, arrowText, diceRollDelay));

        //update game metrics after animation
        if (target == GameProperties.InvestmentTarget.ECONOMIC)
        {
            currPlayer.UpdateMoney(currPlayer.GetMoney() + ((float)newInvestmentValue / 100.0f));
        }
        else
        {
            StartCoroutine(AuxiliaryMethods.UpdateSliderValue(environmentSlider, environmentSlider.value + (float) newInvestmentValue / 100.0f));
        }
       
        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), currPlayer.GetId().ToString(), currPlayer.GetName().ToString(), "ROLLED_INVESTMENT_TARGET_DICES", "-", numTokensForTarget.ToString());
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
                GameObject diceUIClone = Instantiate(diceImagePrefab, rollDiceForInstrumentOverlayDicesUI.transform);
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
        GameObject diceArrowClone = UIRollDiceForInstrumentOverlay.transform.Find("oneUpArrow").gameObject;
        diceArrowClone.SetActive(true);
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
        //Destroy(diceArrowClone);
        diceArrowClone.SetActive(false);
        for (int i=0; i<diceUIs.Count; i++)
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
    
    private IEnumerator BudgetExecutionPhase(Player currPlayer)
    {
        yield return RollInvestmentDices(currPlayer, GameProperties.InvestmentTarget.ECONOMIC);
        yield return RollInvestmentDices(currPlayer, GameProperties.InvestmentTarget.ENVIRONMENT);
    }

    private IEnumerator InvestmentSimulationPhase()
    {
        //simulate evolution
        AuxiliaryMethods.UpdateSliderValue(environmentSlider, environmentSlider.value - 1.0f * Random.Range(0.05f, 0.2f));
        foreach (Player player in GameGlobals.players)
        {
            player.UpdateMoney(Random.Range(0.05f, 0.2f));
        }
        yield return null;
    }
    

    private void ResetAllPlayerUIs()
    {
        foreach (Player player in GameGlobals.players)
        {
            player.ResetPlayerUI();
        }
    }

    IEnumerator YieldedGameUpdateLoop()
    {
        //avoid rerun in this case because load scene is asyncronous
        if (this.gameMainSceneFinished || this.interruptionRequests > 0)
        {
            //Debug.Log("pause...");
            yield return null;
        }

        //middle of the phases
        if (budgetAllocationResponseReceived)
        {
            currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

            budgetAllocationResponseReceived = false;
            Player currPlayer = GameGlobals.players[currPlayerIndex];
            Player nextPlayer = ChangeToNextPlayer(currPlayer);

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
            //Player currPlayer = GameGlobals.players[currPlayerIndex];
            //Player nextPlayer = ChangeToNextPlayer(currPlayer);
            //if (numPlayersToPlayForInstrument > 0)
            //{
            //foreach (var player in GameGlobals.players)
            //{
            //    if (player == currPlayer) continue;
            //    player.InformPlayForInstrument(nextPlayer);
            //}
            //}
            //numPlayersToDisplayHistory--;

        }
        if (budgetExecutionResponseReceived)
        {
            currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

            budgetExecutionResponseReceived = false;
            Player currPlayer = GameGlobals.players[currPlayerIndex];
            yield return StartCoroutine(BudgetExecutionPhase(currPlayer));
            Player nextPlayer = ChangeToNextPlayer(currPlayer);
            
            numPlayersToExecuteBudget--;
            if (numPlayersToExecuteBudget > 0)
            {
                nextPlayer.BudgetExecutionPhaseRequest();
            }
            
        }
        if (investmentSimulationResponseReceived)
        {
            //currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

            //investmentSimulationResponseReceived = false;
            //Player currPlayer = GameGlobals.players[currPlayerIndex];
            yield return StartCoroutine(InvestmentSimulationPhase());
            //Player nextPlayer = ChangeToNextPlayer(currPlayer);

            //numPlayersToSimulateInvestment--;
            //if (numPlayersToSimulateInvestment > 0)
            //{
            //    nextPlayer.InvestmentSimulationRequest();
            //}
            
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
            numPlayersToDisplayHistory = GameGlobals.players.Count;
        }

        //end of third phase
        if (numPlayersToExecuteBudget == 0)
        {
            StartInvestmentSimulationPhase();
            numPlayersToExecuteBudget = GameGlobals.players.Count;
        }

        //end of forth phase
        if (numPlayersToSimulateInvestment == 0)
        {
            UInewRoundScreen.SetActive(true);
            numPlayersToSimulateInvestment = GameGlobals.players.Count;
        }

    }

    // allows to wait for all players to exit one phase before starting other phase
    void Update () {
        StartCoroutine(YieldedGameUpdateLoop());
    }

    public void StartAlocateBudgetPhase()
    {
        ResetAllPlayerUIs();
        GameGlobals.players[0].BudgetAllocationPhaseRequest();

        commonSkipPhaseButton.onClick.RemoveAllListeners();
        commonSkipPhaseButton.onClick.AddListener(delegate () {
            numPlayersToAllocateBudget = 0;
        });
    }
    public void StartDisplayHistoryPhase()
    {
        ResetAllPlayerUIs();
        GameGlobals.players[0].HistoryDisplayPhaseRequest();

        //this phase displays the history of all players
        foreach (Player player in GameGlobals.players)
        {
            player.HistoryDisplayPhaseRequest();
            player.GetPlayerDisablerUI().SetActive(false);
            player.GetPlayerMarkerUI().SetActive(false);
        }

        commonSkipPhaseButton.onClick.RemoveAllListeners();
        commonSkipPhaseButton.onClick.AddListener(delegate () {
            numPlayersToDisplayHistory = 0;
        });
    }
    public void StartExecuteBudgetPhase()
    {
        ResetAllPlayerUIs();
        GameGlobals.players[0].BudgetExecutionPhaseRequest();

        commonSkipPhaseButton.onClick.RemoveAllListeners();
        commonSkipPhaseButton.onClick.AddListener(delegate () {
            numPlayersToExecuteBudget = 0;
        });
    }
    public void StartInvestmentSimulationPhase()
    {
        ResetAllPlayerUIs();

        //this phase simulates the evolution of all players
        foreach (Player player in GameGlobals.players)
        {
            player.InvestmentSimulationRequest();
            player.GetPlayerDisablerUI().SetActive(false);
            player.GetPlayerMarkerUI().SetActive(false);
        }

        commonSkipPhaseButton.onClick.RemoveAllListeners();
        commonSkipPhaseButton.onClick.AddListener(delegate () {
            numPlayersToSimulateInvestment = 0;
        });
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
    public void InvestmentSimulationPhaseResponse(Player invoker)
    {
        investmentSimulationResponseReceived = true;
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
