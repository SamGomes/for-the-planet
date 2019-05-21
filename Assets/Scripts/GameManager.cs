
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

    void OnApplicationQuit()
    {
        foreach (Player player in GameGlobals.players)
        {
            player.rpc.SaveToFile(Application.streamingAssetsPath+ "/Runtimed/"+ player.GetName()+".rpc");
        }
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

        StartCoroutine(envDynamicSlider.UpdateSliderValue(0.1f));
        
        GameGlobals.commonEnvironmentInvestment = 0;
        DontDestroyOnLoad(CommonAreaUI);

        marketLimit = Mathf.FloorToInt(GameProperties.configurableProperties.numberOfAlbumsPerGame * 4.0f / 5.0f) - 1;
        currNumberOfMarketDices = GameProperties.configurableProperties.initNumberMarketDices;
        
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

        // @jbgrocha: Auto Continue for Batch Mode
        if (GameGlobals.autoPlay)
        {
            newRoundScreen.SetActive(false);
            StartGameRoundForAllPlayers();
            Debug.Log("In BatchMode : Start Next Round");
        }

        // ONHOLD: @jbgrocha: Send Start of New Game to AIPlayers (call AIplayer update emotional module function)
    }


    private IEnumerator BudgetExecutionPhase(Player currPlayer)
    {
        var currInvestment = currPlayer.GetCurrRoundInvestment();
        int numTokensForEconomy = currInvestment[GameProperties.InvestmentTarget.ECONOMIC];
        int numTokensForEnvironment = currInvestment[GameProperties.InvestmentTarget.ENVIRONMENT];

        //roll dice for GameProperties.InvestmentTarget.ECONOMIC
        string diceOverlayTitle = currPlayer.GetName() + " rolling " + numTokensForEconomy + " dice for economic growth ...";
        yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, numTokensForEconomy));
        yield return StartCoroutine(currPlayer.SetMoney(currPlayer.GetMoney() + ((float)diceManager.GetCurrDiceTotal() / 100.0f)));

        //roll dice for GameProperties.InvestmentTarget.ENVIRONMENT       
        diceOverlayTitle = currPlayer.GetName() + " rolling " + numTokensForEnvironment + " dice for environment ...";
        yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, numTokensForEnvironment));
        yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(environmentSliderSceneElement.value + ((float)diceManager.GetCurrDiceTotal() / 100.0f)));
    }

    private IEnumerator YieldedGameUpdateLoop()
    {
        //avoid rerun in this case because load scene is asyncronous
        if (this.gameMainSceneFinished || this.interruptionRequests > 0)
        {
            //Debug.Log("pause...");
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
            if (!GameGlobals.autoPlay)
            {
                yield return new WaitForSeconds(phaseEndDelay);
            }
            StartExecuteBudgetPhase();
        }

        //end of third phase
        if (numPlayersToExecuteBudget == 0)
        {
            //Fatima updates
            foreach (Player player in GameGlobals.players)
            {
                List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();

                //players see the results of dice roll and think
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("Increase(Environment)", System.Math.Round(envDynamicSlider.GetSliderValue(), 2).ToString(), "World"));
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("Increase(" + player.GetName() + ", Economy)", System.Math.Round(envDynamicSlider.GetSliderValue(), 2).ToString(), "World"));

                events.Add(RolePlayCharacter.EventHelper.PropertyChange("State(Environment)", System.Math.Round(envDynamicSlider.GetSliderValue(), 2).ToString(), "World"));
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("State(" + player.GetName() + ",Economy)", System.Math.Round(player.GetMoney(), 2).ToString(), "World"));

                player.rpc.Perceive(events);
            }

            simulateInvestmentScreen.SetActive(true); //StartInvestmentSimulationPhase(); is called in this screen
            numPlayersToExecuteBudget = GameGlobals.players.Count;

            if(GameGlobals.autoPlay)
            {
                simulateInvestmentScreen.SetActive(false);
                StartInvestmentSimulationPhase();
            }
            
        }

        //end of forth phase
        if (numPlayersToSimulateInvestment == 0)
        {
            numPlayersToSimulateInvestment = GameGlobals.players.Count;

            string diceOverlayTitle = "Simulating environment growth ...";
            yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, 2));

            float envDecay = ((float)diceManager.GetCurrDiceTotal() / 100.0f);
            yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(environmentSliderSceneElement.value - envDecay));

            //Fatima updates
            foreach (Player player in GameGlobals.players)
            {
                diceOverlayTitle = "Simulating economic growth ...";
                yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, 2));

                float economicDecay = ((float)diceManager.GetCurrDiceTotal() / 100.0f);
                yield return StartCoroutine(player.SetMoney(player.GetMoney() - economicDecay));

                List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();

                //players see the results of dice roll and think
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("SimulationDecay(Environment)", envDecay.ToString(), "World"));
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("SimulationDecay(" + player.GetName() + ", Economy)", economicDecay.ToString(), "World"));

                events.Add(RolePlayCharacter.EventHelper.PropertyChange("State(Environment)", System.Math.Round(envDynamicSlider.GetSliderValue(), 2).ToString(), "World"));
                events.Add(RolePlayCharacter.EventHelper.PropertyChange("State(" + player.GetName() + ",Economy)", System.Math.Round(player.GetMoney(), 2).ToString(), "World"));
            
                foreach (Player otherPlayer in GameGlobals.players)
                {
                    if (otherPlayer == player)
                    {
                        continue;
                    }
                    events.Add(RolePlayCharacter.EventHelper.PropertyChange("State(" + otherPlayer.GetName() + ",Economy)", System.Math.Round(player.GetMoney(), 2).ToString(), "World"));
                }
                player.rpc.Perceive(events);

                var decideResult = player.rpc.Decide().ToList<ActionLibrary.IAction>();
                var emot = player.rpc.GetStrongestActiveEmotion();
                string printedEmot = (emot != null)? "{ EmotionType: "+emot.EmotionType+"; Valence: "+emot.Valence+"; Intensity: "+emot.Intensity + " }": "null";
                StartCoroutine(GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), player.GetId().ToString(), player.GetName(), "FATIMA_EMOTION_CHECK", "-", printedEmot));
                StartCoroutine(GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), player.GetId().ToString(), player.GetName(), "FATIMA_DECIDE_CALLED", "-", decideResult.ToArray().ToString()));
            }


            if (!GameGlobals.autoPlay)
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
            if (GameGlobals.autoPlay)
            {
                newRoundScreen.SetActive(false);
                StartGameRoundForAllPlayers();
                Debug.Log("In BatchMode : Start Next Round");
            }

        }

    }

    // allows to wait for all players to exit one phase before starting other phase
    void Update () {
        StartCoroutine(YieldedGameUpdateLoop());
    }


    public void StartGameRoundForAllPlayers()
    {
        int numPlayers = GameGlobals.players.Count;
        //Fatima updates
        foreach (Player player in GameGlobals.players)
        {
            List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();

            //players see the results of dice roll and think
            events.Add(RolePlayCharacter.EventHelper.ActionEnd("World", "Start(Round)", player.GetName()));
            player.rpc.Perceive(events);



            player.SetRoundBudget(5);
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
