
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;

public class GameManager : MonoBehaviour {


    //public float lastEnvDecay;

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
    
    public GameObject rollDiceOverlay;
    public GameObject diceUIPrefab;
    private DiceManager diceManager;

    public GameObject poppupPrefab;
    public PopupScreenFunctionalities infoPoppupNeutralRef;
    public PopupScreenFunctionalities infoPoppupLossRef;
    public PopupScreenFunctionalities infoPoppupWinRef;

    public GameObject CommonAreaUI;
    public Slider environmentSliderSceneElement;
    private DynamicSlider envDynamicSlider;
    public bool isAutomaticPhaseSkipEnabled;

    private bool gameMainSceneFinished;
    private int interruptionRequests; //changed whenever an interruption occurs (either a poppup, warning, etc.)
   

    private int currPlayerIndex;
    private int currSpeakingPlayerId;
    
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
        
        GameGlobals.envState = 0.1f;
        StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));
        
        DontDestroyOnLoad(CommonAreaUI);
        
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

        // Narrator (function is ienumerator)
        if(GameGlobals.isNarrated)
        {
            StartCoroutine(GameGlobals.narrator.GameStart());
        }


        ContinueGame();

        // @jbgrocha: Auto Continue for Batch Mode
        if (GameGlobals.isSimulation)
        {
            newRoundScreen.SetActive(false);
            StartGameRoundForAllPlayers();
            Debug.Log("In BatchMode : Start Next Round");
        }
        
        // ONHOLD: @jbgrocha: Send Start of New Game to AIPlayers (call AIplayer update emotional module function)

        //StartCoroutine(YieldedGameUpdateLoop());
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

        float economicIncrease = (float) economyResult / 100.0f;

        yield return StartCoroutine(currPlayer.SetEconomicResult(economicIncrease));

        // Narrator - Budget Execution Economy
        if (GameGlobals.isNarrated)
        {
            yield return GameGlobals.narrator.EconomyBudgetExecution(currPlayer, GameGlobals.currGameRoundId, economyResult);
        }

        //roll dice for GameProperties.InvestmentTarget.ENVIRONMENT       
        diceOverlayTitle = currPlayer.GetName() + " rolling " + numTokensForEnvironment + " dice for environment ...";
        yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, numTokensForEnvironment));

        int environmentResult = diceManager.GetCurrDiceTotal();

        float envIncrease = (float) environmentResult / 100.0f;

        GameGlobals.envState += envIncrease;
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));
        currPlayer.SetEnvironmentResult(envIncrease);

        if (GameGlobals.isNarrated)
        {
            // Narrator - Budget Execution Environment
            yield return GameGlobals.narrator.EnvironmentBudgetExecution(currPlayer, GameGlobals.currGameRoundId, environmentResult);
        }

        // Narrator (function is ienumerator)
        // Should Receive a Player (or results)
        // Should have 2 calls, one for environment and one for economy???
        //yield return GameGlobals.narrator.BudgetExecution(currPlayer, GameGlobals.currGameRoundId, economyResult, environmentResult);
    }

    private IEnumerator YieldedGameUpdateLoop()
    {
        //while (true)
        //{

        //avoid rerun in this case because load scene is asyncronous
        if (this.gameMainSceneFinished || this.interruptionRequests > 0)
        {
            yield return null;
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
            if (!GameGlobals.isSimulation)
            {
                yield return new WaitForSeconds(phaseEndDelay);
            }
            StartExecuteBudgetPhase();
        }

        //end of third phase
        if (numPlayersToExecuteBudget == 0)
        {
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
            numPlayersToSimulateInvestment = GameGlobals.players.Count;

            string diceOverlayTitle = "Simulating environment decay ...";
            yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, 2));

            int environmentDecay = diceManager.GetCurrDiceTotal();
            float envDecay = ((float) environmentDecay / 100.0f);

            envDecay *= GameGlobals.players.Count;

            //lastEnvDecay = envDecay;
            GameGlobals.envState -= envDecay;
            yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));

            if (GameGlobals.isNarrated)
            {
                // Narrator - Environmental Decay
                StartCoroutine(GameGlobals.narrator.EnvironmentDecaySimulation(GameGlobals.currGameRoundId, environmentDecay));
            }

            foreach (Player player in GameGlobals.players)
            {
                diceOverlayTitle = "Simulating economic decay ...";
                yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, 2));

                int economyDecay = diceManager.GetCurrDiceTotal();
                float economicDecay = ((float) economyDecay / 100.0f);
                StartCoroutine(player.SetEconomicDecay(economicDecay));

                if (GameGlobals.isNarrated)
                {
                    // Narrator - Player Economic Decay
                    StartCoroutine(GameGlobals.narrator.EconomyDecaySimulation(player, GameGlobals.currGameRoundId, economyDecay));
                }
            }

            if (!GameGlobals.isSimulation)
            {
                yield return new WaitForSeconds(phaseEndDelay);
            }
            //check for game end to stop the game instead of loading new round
            if (environmentSliderSceneElement.value <= 0.05f)
            {
                GameGlobals.currGameState = GameProperties.GameState.LOSS;
                // ONHOLD @jbgrocha: Send GAME LOSS Event to AIPlayers (call AIplayer update emotional module function)
            }
            else
            {
                if (GameGlobals.currGameRoundId > 2)
                {
                    GameGlobals.currGameState = GameProperties.GameState.VICTORY;
                    // ONHOLD @jbgrocha: Send GAME Victory Event to AIPlayers (call AIplayer update emotional module function)
                }
            }

            if (GameGlobals.currGameState != GameProperties.GameState.NOT_FINISHED)
            {
                GameGlobals.gameSceneManager.LoadEndScene();
            }

            GameGlobals.currGameRoundId++;
            newRoundScreen.SetActive(true);

            // @jbgrocha: Auto Continue for Batch Mode
            if (GameGlobals.isSimulation)
            {
                newRoundScreen.SetActive(false);
                StartGameRoundForAllPlayers();
                Debug.Log("In BatchMode : Start Next Round");
            }

        }

        //YieldedGameUpdateLoop();
        //}
    }

    public void FixedUpdate()
    {
        StartCoroutine(YieldedGameUpdateLoop());
    }

    public void StartGameRoundForAllPlayers()
    {
        int numPlayers = GameGlobals.players.Count;
        foreach (Player player in GameGlobals.players)
        {
            List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();

            //players perceive round start
            events.Add(RolePlayCharacter.EventHelper.ActionEnd("World", "State(Round,Start)", player.GetName()));
            player.Perceive(events);

            player.SetRoundBudget(GameGlobals.roundBudget);
        }

        if (GameGlobals.isNarrated)
        {
            // Narrator (function is ienumerator)
            StartCoroutine(GameGlobals.narrator.RoundStart());
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
            
            //Fatima updates
            //players see the history of each other
            List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + player.GetName() + ", Environment)", player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0.00", CultureInfo.InvariantCulture), "World"));
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + player.GetName() + ", Economic)", player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0.00", CultureInfo.InvariantCulture), "World"));
            foreach (Player otherPlayer in GameGlobals.players)
            {
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + otherPlayer.GetName() + ", Environment)", otherPlayer.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0", CultureInfo.InvariantCulture), "World"));
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("AllocatedBudgetPoints(" + otherPlayer.GetName() + ", Economic)", otherPlayer.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0", CultureInfo.InvariantCulture), "World"));
            }
            player.Perceive(events);

            if (GameGlobals.isNarrated)
            {
                // Narrator (function is ienumerator)
                StartCoroutine(GameGlobals.narrator.DisplayHistory(player, GameGlobals.currGameRoundId));
            }
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
    }


    //------------------------------------------Responses---------------------------------------
    public IEnumerator BudgetAllocationPhaseResponse(Player invoker)
    {
        currSpeakingPlayerId = Random.Range(0, GameGlobals.numberOfSpeakingPlayers);
        
        Player currPlayer = GameGlobals.players[currPlayerIndex];
        Player nextPlayer = ChangeToNextPlayer(currPlayer);

        numPlayersToAllocateBudget--;

        if (GameGlobals.isNarrated)
        {
            // Narrator (function is ienumerator)
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

        
        numPlayersToSimulateInvestment--;
        yield return null;
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
