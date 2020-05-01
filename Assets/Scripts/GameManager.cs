
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System;

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
    public GameObject GenerationText;
    public GameObject GenerationNumberText;
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

    public GameProperties.GamePhase currGamePhase;

    private bool gameMainSceneFinished;
    private int interruptionRequests; //changed whenever an interruption occurs (either a poppup, warning, etc.)
    
    private int currPlayerIndex;
    private float phaseEndDelay;

    private int marketLimit;
    private int currNumberOfMarketDices;

    private Text GenerationTextUI;
    private Text GenerationNumberTextUI;

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

        GameGlobals.players[0].SetName(GameGlobals.participantName + "  (YOU)");
        GameGlobals.players[0].UpdateNameUI();

        currPlayerIndex = 0;

        //get player poppups (can be from any player) and set methods
        infoPoppupLossRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoLoss"), new Color(0.9f, 0.8f, 0.8f), "Audio/albumLoss");
        infoPoppupWinRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/InfoWin"), new Color(0.9f, 0.9f, 0.8f), "Audio/albumVictory");
        infoPoppupNeutralRef = new PopupScreenFunctionalities(false, InterruptGame, ContinueGame, poppupPrefab, canvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
        

        gameMainSceneFinished = false;
        phaseEndDelay = 2.0f;
        GameGlobals.envState = 60; //Common Pool Resource
        GameGlobals.envThreshold = GameGlobals.envState / 2;
        GameGlobals.envRenew = 0.5; // At the end of the round the env renew 50%
        GameGlobals.envRenewperRound = 0;
        GameGlobals.roundBudget = 14; 
        GameGlobals.generation = 1; // First Generation
        GameGlobals.envStatePerRound = new List<int>();

        GenerationText = GameObject.Find("GenerationText");
        GenerationTextUI = GenerationText.transform.Find("genText").gameObject.GetComponent<Text>();
        GenerationTextUI.text = "Hello!\n" +"You belong to the First Generation";

        GenerationNumberText = GameObject.Find("GenerationNumber");
        GenerationNumberTextUI = GenerationNumberText.transform.Find("genNumText").gameObject.GetComponent<Text>();
        GenerationNumberTextUI.text = "Generation: "+ GameGlobals.generation.ToString();

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

            newRoundScreen.SetActive(false);
            GenerationText.SetActive(false);
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


            envDynamicSlider = new DynamicSlider(environmentSliderSceneElement.gameObject, true);
            StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));
            DontDestroyOnLoad(CommonAreaUI);

            rollDiceOverlay.SetActive(false);


            advanceRoundButton.onClick.AddListener(delegate ()
            {
                newRoundScreen.SetActive(false);
                GenerationText.SetActive(false);
                StartGameRoundForAllPlayers();
            });



            simulateEvolutionButton.onClick.AddListener(delegate()
            {
                simulateInvestmentScreen.SetActive(false);
                StartInvestmentSimulationPhase();
            });
            simulateInvestmentScreen.SetActive(false);

            //this init is not nice
            Button[] allButtons = FindObjectsOfType<Button>();
            foreach (Button button in allButtons)
            {
                button.onClick.AddListener(delegate() { GameGlobals.audioManager.PlayClip("Audio/snap"); });
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
        float economicIncrease = (float) economyResult / 100.0f;

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
        float envIncrease = (float) environmentResult / 100.0f;
//        Debug.Log("envIncrease: "+envIncrease);

        GameGlobals.envState = Mathf.Clamp01(GameGlobals.envState + envIncrease);
        if (!GameGlobals.isSimulation)
        {
            yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(
                envDynamicSlider.UpdateSliderValue(GameGlobals.envState));
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
                    Dictionary<string, string> logEntry = new Dictionary<string, string>()
                    {
                        {"currSessionId", GameGlobals.currSessionId},
                        {"currGameId", GameGlobals.currGameId.ToString()},
                        {"currRoundId", GameGlobals.currGameRoundId.ToString()},
                        {"playerId", player.GetPlayerType()},
                        {"playerType", player.GetPlayerType()},
                        {"playerCurrInvestEcon", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC].ToString()},
                        {"playerCurrInvestEnv", player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString()},
                        {"playerEconState", player.GetMoney().ToString()},
                        {"envState", GameGlobals.envState.ToString()}
                    };
                    StartCoroutine(GameGlobals.gameLogManager.WriteToLog("fortheplanetlogs", "strategies", logEntry));
                }


                TakeMoneyFromCommonPot();
                GameGlobals.currGameRoundId++;
                yield return new WaitForSeconds(10);


                if ((GameGlobals.currGameRoundId == GameProperties.configurableProperties.maxNumRounds) && 
                (GameGlobals.envState <= GameGlobals.envThreshold)) //environment exploded
                {
                    GameGlobals.currGameState = GameProperties.GameState.LOSS;
                    GameGlobals.gameSceneManager.LoadEndScene();
                }
                else if (GameGlobals.currGameRoundId == GameProperties.configurableProperties.maxNumRounds) //reached last round
                {
                    GameGlobals.currGameState = GameProperties.GameState.VICTORY;
                    GameGlobals.gameSceneManager.LoadEndScene();
                }
                else //normal round finished
                {
                    currGamePhase = GameProperties.GamePhase.BUDGET_ALLOCATION;
                    newRoundScreen.SetActive(true);
                    GameGlobals.generation += 1;
                    GenerationNumberTextUI.text = "Generation: " + GameGlobals.generation.ToString();
                    GenerationText.SetActive(false);
            }
           
        }

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
            yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, UnityEngine.Random.Range(GameGlobals.environmentDecayBudget[0],GameGlobals.environmentDecayBudget[1]+1)));

            int environmentDecay = diceManager.GetCurrDiceTotal();
            float envDecay = (float) environmentDecay / 100.0f;
//            Debug.Log("envDecay: "+envDecay);
            
            GameGlobals.envState = Mathf.Clamp01(GameGlobals.envState - envDecay);
            if (!GameGlobals.isSimulation)
            {
                yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));
            }

            if (!GameGlobals.isSimulation && GameGlobals.isNarrated)
            {
                StartCoroutine(GameGlobals.narrator.EnvironmentDecaySimulation(GameGlobals.currGameRoundId, environmentDecay));
            }

            foreach (Player player in GameGlobals.players)
            {
                diceOverlayTitle = "Simulating economic decay ...";
                yield return StartCoroutine(diceManager.RollTheDice(diceOverlayTitle, UnityEngine.Random.Range(GameGlobals.playerDecayBudget[0],GameGlobals.playerDecayBudget[1]+1)));

                int economyDecay = diceManager.GetCurrDiceTotal();
                float economicDecay = (float) economyDecay / 100.0f;
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
                Debug.Log("[Game: "+GameGlobals.currGameId+"; Round: " + (GameGlobals.currGameRoundId+1) +" of "+GameProperties.configurableProperties.maxNumRounds+"]");
            }

            
            foreach(Player player in GameGlobals.players)
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
                {
                    GenerationText.SetActive(true);
                    newRoundScreen.SetActive(true);
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
        Player currPlayer = GameGlobals.players[currPlayerIndex];
        Player nextPlayer = ChangeToNextPlayer(currPlayer);

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

    private void TakeMoneyFromCommonPot()
    {
        //List<int> votes = new List<int>();
        foreach (Player p in GameGlobals.players)
        {
            GameGlobals.envState -= p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
            
            //votes.Add(p.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT]);
        }
        //renew envState
        GameGlobals.envRenewperRound = (float)(GameGlobals.envState * GameGlobals.envRenew);
        GameGlobals.envState += GameGlobals.envRenewperRound;
        GameGlobals.envStatePerRound.Add(Convert.ToInt32(GameGlobals.envState));
        StartCoroutine(envDynamicSlider.UpdateSliderValue(GameGlobals.envState));
        
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
        */
    }

}
