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

    public GameObject newRoundScreen;
    public Button advanceRoundButton;
    public GameObject simulateInvestmentScreen;
    public Button simulateEvolutionButton;
    
    private GameObject rollDiceOverlayDicesContainer;
    private Animator rollDiceOverlayAnimator;
    public GameObject rollDiceOverlay;

    public GameObject dice6UI;
    public GameObject dice20UI;

    public GameObject diceArrowPrefab;

    public GameObject prototypeArea;

    public GameObject poppupPrefab;
    public PopupScreenFunctionalities infoPoppupNeutralRef;
    public PopupScreenFunctionalities infoPoppupLossRef;
    public PopupScreenFunctionalities infoPoppupWinRef;

    public PopupScreenFunctionalities endPoppupWinRef;
    public PopupScreenFunctionalities endPoppupLossRef;

    public Slider environmentSliderSceneElement;
    private DynamicSlider envDynamicSlider;
    public bool isAutomaticPhaseSkipEnabled;

    private bool gameMainSceneFinished;
    private int interruptionRequests; //changed whenever an interruption occurs (either a poppup, warning, etc.)
   

    private int currPlayerIndex;
    private int currSpeakingPlayerId;

    private float diceRollDelay;
    private float phaseEndDelay;

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


    void Start()
    {


        //assign some game globals
        GameGlobals.gameManager = this;
        GameGlobals.currGameState = GameProperties.GameState.NOT_FINISHED;

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

        //these poppups load the end scene
        endPoppupLossRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), delegate () { /*end game*/ if (this.gameMainSceneFinished) GameSceneManager.LoadEndScene(); return 0; });
        endPoppupWinRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), delegate () { /*end game*/ if (this.gameMainSceneFinished) GameSceneManager.LoadEndScene(); return 0; });

        gameMainSceneFinished = false;
        diceRollDelay = 4.0f;
        phaseEndDelay = 2.0f;

        int numPlayers = GameGlobals.players.Count;
        Player currPlayer = null;
        for (int i = 0; i < numPlayers; i++)
        {
            currPlayer = GameGlobals.players[i];
            currPlayer.ReceiveGameManager(this);
            StartCoroutine(currPlayer.SetMoney(0.1f));

            //Setup warnings
            currPlayer.GetWarningScreenRef().AddOnShow(InterruptGame);
            currPlayer.GetWarningScreenRef().AddOnHide(ContinueGame);
        }

        envDynamicSlider = new DynamicSlider(environmentSliderSceneElement.gameObject);

        StartCoroutine(envDynamicSlider.UpdateSliderValue(0.1f));

        GameGlobals.currGameRoundId = 0; //first round
        GameGlobals.commonEnvironmentInvestment = 0;

        marketLimit = Mathf.FloorToInt(GameProperties.configurableProperties.numberOfAlbumsPerGame * 4.0f / 5.0f) - 1;
        currNumberOfMarketDices = GameProperties.configurableProperties.initNumberMarketDices;

        rollDiceOverlayAnimator = rollDiceOverlay.GetComponent<Animator>();
        rollDiceOverlayDicesContainer = rollDiceOverlay.transform.Find("diceUIs").gameObject;
        rollDiceOverlay.SetActive(false);

        advanceRoundButton.onClick.AddListener(delegate () {
            newRoundScreen.SetActive(false);
            StartGameRoundForAllPlayers();
        });
        
        simulateEvolutionButton.onClick.AddListener(delegate () {
            simulateInvestmentScreen.SetActive(false);
            StartInvestmentSimulationPhase();
        });
        simulateInvestmentScreen.SetActive(false);

        //this init is not nice
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(delegate () {
                GameGlobals.audioManager.PlayClip("Audio/snap");
            });
        }

        ContinueGame();

        //players talk about the initial album
        //currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
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

        rollDiceOverlay.transform.Find("title/Text").GetComponent<Text>().text = currPlayer.GetName() + " rolling "+ numTokensForTarget + " dice for " + target.ToString() + " ...";
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

        yield return StartCoroutine(PlayDiceUIs(currPlayer, newInvestmentValue, rolledDiceNumbers, 6, dice6UI, "Animations/RollDiceOverlay/dice6/sprites_3/endingAlternatives/", Color.yellow, arrowText, diceRollDelay));

        //update game metrics after dice rolls
        if (target == GameProperties.InvestmentTarget.ECONOMIC)
        {
            yield return StartCoroutine(currPlayer.SetMoney(currPlayer.GetMoney() + ((float)newInvestmentValue / 100.0f)));
        }
        else
        {
            yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(environmentSliderSceneElement.value + (float)newInvestmentValue / 100.0f));
        }
       
        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), currPlayer.GetId().ToString(), currPlayer.GetName().ToString(), "ROLLED_INVESTMENT_TARGET_DICES", "-", numTokensForTarget.ToString());
    }

    private IEnumerator PlayDiceUIs(Player diceThrower, int totalDicesValue, int[] rolledDiceNumbers, int diceNum, GameObject diceImagePrefab, string diceNumberSpritesPath, Color diceArrowColor, string diceArrowText, float delayToClose)
    //the sequence number aims to void dice overlaps as it represents the order for which this dice is going to be rolled. We do not want to roll a dice two times for the same place
    {
        InterruptGame();
        rollDiceOverlay.SetActive(true);
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
                GameObject diceUIClone = Instantiate(diceImagePrefab, rollDiceOverlayDicesContainer.transform);
                diceUIs.Add(diceUIClone);
                StartCoroutine(PlayDiceUI(diceUIClone, diceThrower, numDiceRolls, i, diceNum, currDiceNumberSprite, delayToClose));
            }
        }

        while (!rollDiceOverlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            yield return null;
        }

        rollDiceOverlayAnimator.speed = 0;
        
        //get and disable arrow animation until end of dice animation
        GameObject diceArrowClone = rollDiceOverlay.transform.Find("oneUpArrow").gameObject;
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

        rollDiceOverlayAnimator.speed = 1;
        while (!rollDiceOverlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle2"))
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
        rollDiceOverlay.SetActive(false);
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

    private IEnumerator YieldedGameUpdateLoop()
    {
        //avoid rerun in this case because load scene is asyncronous
        if (this.gameMainSceneFinished || this.interruptionRequests > 0)
        {
            //Debug.Log("pause...");
            yield return null;
        }

        if (GameGlobals.currGameState != GameProperties.GameState.NOT_FINISHED)
        {
            GameSceneManager.LoadEndScene();
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
            numPlayersToDisplayHistory = GameGlobals.players.Count;
            yield return new WaitForSeconds(phaseEndDelay);
            StartExecuteBudgetPhase();
        }

        //end of third phase
        if (numPlayersToExecuteBudget == 0)
        {
            simulateInvestmentScreen.SetActive(true); //StartInvestmentSimulationPhase(); is called in this screen
            numPlayersToExecuteBudget = GameGlobals.players.Count;
        }

        //end of forth phase
        if (numPlayersToSimulateInvestment == 0)
        {
            numPlayersToSimulateInvestment = GameGlobals.players.Count;
            yield return new WaitForSeconds(phaseEndDelay);
            newRoundScreen.SetActive(true);
        }

    }

    // allows to wait for all players to exit one phase before starting other phase
    void Update () {
        StartCoroutine(YieldedGameUpdateLoop());
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

    public void StartAlocateBudgetPhase()
    {
        foreach (Player player in GameGlobals.players)
        {
            player.ResetPlayerUI();
        }
        ChangeActivePlayerUI(GameGlobals.players[0]);
        GameGlobals.players[0].BudgetAllocationPhaseRequest();
    }
    public void StartDisplayHistoryPhase()
    {
        //this phase displays the history of all players
        foreach (Player player in GameGlobals.players)
        {
            player.ResetPlayerUI();
            player.HistoryDisplayPhaseRequest();
        }
    }
    public void StartExecuteBudgetPhase()
    {
        foreach (Player player in GameGlobals.players)
        {
            player.ResetPlayerUI();
        }

        ChangeActivePlayerUI(GameGlobals.players[0]);
        GameGlobals.players[0].BudgetExecutionPhaseRequest();
    }

    public void StartInvestmentSimulationPhase()
    {

        //this phase simulates the evolution of all players
        foreach (Player player in GameGlobals.players)
        {
            player.ResetPlayerUI();
            player.InvestmentSimulationRequest();
        }

        //check for game end
        if (environmentSliderSceneElement.value <= 0.0f)
        {
            GameGlobals.currGameState = GameProperties.GameState.LOSS;
        }
        else
        {
            if (GameGlobals.currGameRoundId > 2)
            {
                GameGlobals.currGameState = GameProperties.GameState.VICTORY;
            }
        }
    }


    //------------------------------------------Responses---------------------------------------
    public IEnumerator BudgetAllocationPhaseResponse(Player invoker)
    {
        currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        
        Player currPlayer = GameGlobals.players[currPlayerIndex];
        Player nextPlayer = ChangeToNextPlayer(currPlayer);

        numPlayersToAllocateBudget--;
        if (numPlayersToAllocateBudget > 0)
        {
            nextPlayer.BudgetAllocationPhaseRequest();
        }
        yield return null;
    }
    public IEnumerator HistoryDisplayPhaseResponse(Player invoker)
    {
        //currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        
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
        numPlayersToDisplayHistory--;
        yield return null;
    }
    public IEnumerator BudgetExecutionPhaseResponse(Player invoker)
    {
        currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        
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
        //currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);

        //investmentSimulationResponseReceived = false;
        //Player currPlayer = GameGlobals.players[currPlayerIndex];
        //Player nextPlayer = ChangeToNextPlayer(currPlayer);

        //if (numPlayersToSimulateInvestment > 0)
        //{
        //    nextPlayer.InvestmentSimulationRequest();
        //}

        //simulate evolution
        //yield return StartCoroutine(RollInvestmentDices(GameProperties.InvestmentTarget.ENVIRONMENT));//Random.Range(0.2f, 0.4f));
        yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(0.1f));//Random.Range(0.2f, 0.4f));
        foreach (Player player in GameGlobals.players)
        {
            yield return StartCoroutine(player.SetMoney(0.1f));//s Random.Range(0.2f, 0.4f));
        }
        numPlayersToSimulateInvestment--;
    }

    public Player ChangeToNextPlayer(Player currPlayer)
    {
        currPlayerIndex = (currPlayerIndex + 1) % GameGlobals.players.Count;
        Player nextPlayer = GameGlobals.players[currPlayerIndex];
        ChangeActivePlayerUI(nextPlayer);
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
}
