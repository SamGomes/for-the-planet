using RolePlayCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Utilities;
using MCTS.Core;
//Using the MCTS implementation originally provided by @Mikeywalsh

public class EmotionalAIPlayer: AIPlayer
{
    
    //Emotional stuff
    protected RolePlayCharacterAsset rpc;
    protected Dictionary<GameProperties.InvestmentTarget, int> investmentIntentions;
    
    public EmotionalAIPlayer(string playerType, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
        base(playerType, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        rpc = GameGlobals.FAtiMAIat.Characters.FirstOrDefault(x => x.CharacterName.ToString() == this.playerType.ToString()+"_"+id);
        rpc.CharacterName =  (WellFormedNames.Name) ("Agent" + this.id.ToString());

        investmentIntentions = new Dictionary<GameProperties.InvestmentTarget, int>();

        //default investment intention
        investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] = 3;
        investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] = 2;
    }
    
    public virtual Dictionary<GameProperties.InvestmentTarget, int> ComputeIntentions(){  return null; }
    
    public RolePlayCharacterAsset GetRpc()
    {
        return this.rpc;
    }

    public override IEnumerator AutoBudgetAllocation()
    {
        base.AutoBudgetAllocation();

        investmentIntentions = ComputeIntentions();
        int econ = investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC];
        int env = investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT];
        
        WellFormedNames.Name evnt = RolePlayCharacter.EventHelper.PropertyChange("BeforeBudgetAllocation(" + econ + "," + env + ")", this.id.ToString(), this.id.ToString());
        rpc.Perceive(evnt);
        rpc.Update();

        yield return InvestInEconomy(econ);
        yield return InvestInEnvironment(env);
        yield return EndBudgetAllocationPhase();
    }

    public override IEnumerator AutoHistoryDisplay()
    {
        yield return base.AutoHistoryDisplay();

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        float econ = 0;
        float env = 0;
        foreach (Player player in GameGlobals.players)
        {
            if (player == this)
            {
                continue;
            }
            //aggregate
            econ += player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC];
            env += player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
        }
        //aggregate
        econ /= (GameGlobals.players.Count - 1);
        env /= (GameGlobals.players.Count - 1);
        WellFormedNames.Name evnt = RolePlayCharacter.EventHelper.PropertyChange(
        "HistoryDisplay(" + econ.ToString("0.00", CultureInfo.InvariantCulture) + "," +
        env.ToString("0.00", CultureInfo.InvariantCulture) + ")",
        this.id.ToString(), "OtherPlayers");
        rpc.Perceive(evnt);
        rpc.Update();

        yield return null;
    }

    public override IEnumerator AutoBudgetExecution()
    {
        yield return base.AutoBudgetExecution();
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        yield return base.AutoInvestmentExecution();
    }
}



//concrete actions
public class FTPMove: Move
{
    private int investmentEnv;

    public FTPMove(int investmentEnv)
    {
        this.investmentEnv = investmentEnv;
    }
    
    /// Equality override which returns true if this instance is equal to a passed in object
    public override bool Equals(object obj)
    {
        if (obj is FTPMove)
        {
            FTPMove other = (FTPMove) obj;
            return other.investmentEnv == investmentEnv;
        }
        return false;
    }

    /// Returns a unique hash code for this move instance
    public int GetInvestmentEnv()
    {
        return this.investmentEnv;
    }


    /// Returns a unique hash code for this move instance
    public override int GetHashCode()
    {
        return investmentEnv;
    }

    /// Returns a string representation of this move instance
    public override string ToString()
    {
        return "investment Env: " + investmentEnv + ";\n"
               + "investment Econ: " + (GameGlobals.roundBudget - investmentEnv);
    }
}



//concrete states
public class FTPBoard : Board
{
    protected float env;
    protected List<float> econs;
    protected Func<Player, int> actionPredictorCallback;

    protected int currGameRound;
    protected int maxGameRound;
    
        
        
    public override float GetScore(int player)
    {
        if(winner == 0)
        {
            return DRAW_SCORE;
        }
        if(winner == player)
        {
            return WIN_SCORE;
        }

        float bestEcon = -1.0f;
        foreach (Player innerPlayer in GameGlobals.players)
        {
            int id = innerPlayer.GetId();
            float currEcon = econs[id];
            if (currEcon > bestEcon)
            {
                bestEcon = currEcon;
            }
        }
        
        return (econs[player - 1] - bestEcon)* 0.08f;       
    }

        
    public FTPBoard()
    {
        this.env = 0;
        this.econs = new List<float>();
        
        CurrentPlayer = 0;

        this.actionPredictorCallback = null;
        this.maxGameRound = 0;
        this.currGameRound = 0;
        possibleMoves = GeneratePossibleMoves();

    }
    public FTPBoard(FTPBoard board)
    {
        CurrentPlayer = board.CurrentPlayer;
        winner = board.Winner;
        possibleMoves = GeneratePossibleMoves();
        
        this.maxGameRound = board.maxGameRound;
        this.currGameRound = board.currGameRound;
        this.actionPredictorCallback = board.actionPredictorCallback;
        this.env = board.env;
        this.econs = new List<float>(board.econs);
    }
    public FTPBoard(int id, int maxGameRound, Func<Player, int> actionPredictorCallback, float env, List<float> econs): this()
    {
        this.CurrentPlayer = id;
        this.maxGameRound = maxGameRound;
        this.currGameRound = 0;
        this.actionPredictorCallback = actionPredictorCallback;
        this.env = env;
        this.econs = econs;
    }
    
    /// Performs a move on this board state for the current player and returns the updated state.
    public override Board MakeMove(Move move)
    {
        DetermineWinner(move);
        
        FTPMove moveFTP = (FTPMove) move;
        int myInvestmentEnv = moveFTP.GetInvestmentEnv();

        List<float> estEcons = new List<float>();

        int estEnvDice = 0;
        
        float estDecayEcon = (GameGlobals.playerDecayBudget[0] + GameGlobals.playerDecayBudget[1]) / 2.0f;
        estDecayEcon = (estDecayEcon * 3.5f) / 100.0f;
        foreach(Player player in GameGlobals.players)
        {
            float estGainEcon = 0.0f;
            if (player.GetId() == CurrentPlayer)
            {
                estEnvDice += myInvestmentEnv;
                estGainEcon = ((GameGlobals.roundBudget - myInvestmentEnv)*3.5f)/ 100.0f;
            }
            else
            { 
                int opponentEnvDice = actionPredictorCallback(player);
                estEnvDice += opponentEnvDice;
                estGainEcon = ((GameGlobals.roundBudget - opponentEnvDice)*3.5f)/ 100.0f;
            }
            float econToPop = econs[0];
            econs.RemoveAt(0);
            estEcons.Add(Mathf.Clamp01(econToPop + (estGainEcon - estDecayEcon)));
        }

        float estGainEnv = (estEnvDice * 3.5f) / 100.0f;
        float estDecayEnv = (GameGlobals.environmentDecayBudget[0] + GameGlobals.environmentDecayBudget[1]) / 2.0f;
        estDecayEnv = (estDecayEnv * 3.5f) / 100.0f;
        
        float estEnv = Mathf.Clamp01(env + (estGainEnv - estDecayEnv));
        
        econs = estEcons;
        env = estEnv;
        this.currGameRound = this.currGameRound + 1;

        return base.MakeMove(move);
    }

    /// Gets a list of possible moves that can follow from this board state
    public List<Move> GeneratePossibleMoves()
    {
        List<Move> moves = new List<Move>();
        for (int i = 0; i < (GameGlobals.roundBudget + 1); i++)
        {
            moves.Add(new FTPMove(i));
        }
        
        return moves;
    }
    
    /// Gets a list of possible moves that can follow from this board state
    public override List<Move> PossibleMoves()
    {
        return possibleMoves;
    }

    /// Performs a deep copy of the current board state and returns the copy
    public override Board Duplicate()
    {
        return new FTPBoard(this);
    }

    /// Gives a rich text string representation of this board <para/>
    /// The output string will have color tags that make the board easier to read
    public override string ToRichString()
    {
        return "";
    }

    /// Returns amount of players playing on this board <para/>
    /// Can't have static polymorphism and a workaround would be less efficient for execution speed <para/>
    /// Compromise is to have every instance contain the player count
    protected override int PlayerCount()
    {
        return GameGlobals.players.Count;
    }

    /// Determines if there is a winner or not for this board state and updates the winner integer accordingly
    protected override void DetermineWinner()
    {
        this.winner = -1;
        if (env < 0.001)
        {
            this.winner = 10;
        }
        else if (currGameRound >= maxGameRound)
        {
            float bestEcon = -1.0f;
            List<int> bestPlayers = new List<int>();
            for (int i = 0; i < GameGlobals.players.Count; i++)
            {
                float currEcon = econs[i];
                if (currEcon > bestEcon)
                {
                    bestEcon = currEcon;
                    bestPlayers.Add(i);
                }else if ((currEcon - bestEcon) < 0.001)
                {
                    bestPlayers.Add(i);
                }
            }

            if (bestPlayers.Count() > 1)
            {
                this.winner = 10;
                if (bestPlayers.Contains(CurrentPlayer))
                {
                    this.winner = 0;
                }
            }
            else
            {
                this.winner = bestPlayers[0] + 1;
            }
        }
    }

    /// A more efficient method of determining if there is a winner <para/>
    /// Saves time by using knowledge of the last move to remove unnessessary computation
    protected override void DetermineWinner(Move move)
    {
        DetermineWinner();
    }
}


public class CompetitiveCooperativeEmotionalAIPlayer : EmotionalAIPlayer
{
    // big brain
    private static TreeSearch<Node> mcts;
    
    public CompetitiveCooperativeEmotionalAIPlayer(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
       base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name, updateDelay, fatimaRpcPath)
    {
    }

    public override Dictionary<GameProperties.InvestmentTarget, int> ComputeIntentions()
    {
        EmotionalAppraisal.IActiveEmotion strongestEmotion = this.rpc.GetStrongestActiveEmotion();
        if (strongestEmotion == null) {
            return investmentIntentions;
        }

        string rpcArr = "[";
        int j = 0;
        
        interactionModule.Speak("I'm feeling " + strongestEmotion.EmotionType);
        
        List<float> moneyValues = new List<float>();
        foreach (Player player in GameGlobals.players)
        {
            moneyValues.Add(player.GetMoney());
        }

        float rpcMood = rpc.Mood;
        int numMctsSteps = 0;
        if(rpcMood >= -10.0f && rpcMood < -3.0f)
        { 
            numMctsSteps = 3;
        }
        else if (rpcMood >= -3.0f && rpcMood <= 3.0f)
        {
            numMctsSteps = 2;
        }
        else if (rpcMood > 3.0f && rpcMood <= 10.0f) 
        {
            numMctsSteps = 1;
        }
        
        FTPBoard currentState = new FTPBoard(this.id + 1, GameProperties.configurableProperties.maxNumRounds, PredictAction, GameGlobals.envState, moneyValues);
        return Analyse(numMctsSteps, currentState); 
    }

    public Dictionary<GameProperties.InvestmentTarget, int> Analyse(int numMctsSteps, FTPBoard currentState)
    {
        // big brain time: call MCTS
        mcts = new TreeSearch<Node>(currentState); //only makes 1 sim per node and expands all

        for (int i = 0; i < (Math.Pow(6, numMctsSteps)); i++)
        {
            mcts.Step();
        }

        Node bestNodeChoice = mcts.BestNodeChoice(mcts.Root);
        int env = ((FTPMove) bestNodeChoice.GameBoard.LastMoveMade).GetInvestmentEnv();
        Dictionary<GameProperties.InvestmentTarget, int> inv = new Dictionary<GameProperties.InvestmentTarget, int>();
        inv[GameProperties.InvestmentTarget.ENVIRONMENT] = env; 
        inv[GameProperties.InvestmentTarget.ECONOMIC] = GameGlobals.roundBudget - env;
        return inv;
    }
    
    public int PredictAction(Player player)
    {
        float coopValue = (player.GetCoopPerc() * GameGlobals.roundBudget);
        return UnityEngine.Random.Range(Mathf.FloorToInt(coopValue), Mathf.CeilToInt(coopValue));
    }
}


public class AIMCTSPlayer : AIPlayer
{
    // big brain
    private static TreeSearch<Node> mcts;
    private int depth;
    
    public AIMCTSPlayer(int depth, string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        this.depth = depth;
    }

    public override IEnumerator AutoBudgetAllocation()
    {
        List<float> moneyValues = new List<float>();
        foreach (Player player in GameGlobals.players)
        {
            moneyValues.Add(player.GetMoney());
        }

        FTPBoard currentState = new FTPBoard(this.id + 1, GameProperties.configurableProperties.maxNumRounds, PredictAction, GameGlobals.envState, moneyValues);
        currRoundInvestment = Analyse(depth, currentState);
        
        yield return InvestInEconomy(currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC]);
        yield return InvestInEnvironment(currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT]);
        yield return EndBudgetAllocationPhase();
    }

    public Dictionary<GameProperties.InvestmentTarget, int> Analyse(int numMctsSteps, FTPBoard currentState)
    {
        // big brain time: call MCTS
        mcts = new TreeSearch<Node>(currentState); //only makes 1 sim per node and expands all

        for (int i = 0; i < (Math.Pow(6, numMctsSteps)); i++)
        {
            mcts.Step();
        }

        Node bestNodeChoice = mcts.BestNodeChoice(mcts.Root);
        int env = ((FTPMove) bestNodeChoice.GameBoard.LastMoveMade).GetInvestmentEnv();
        Dictionary<GameProperties.InvestmentTarget, int> inv = new Dictionary<GameProperties.InvestmentTarget, int>();
        inv[GameProperties.InvestmentTarget.ENVIRONMENT] = env; 
        inv[GameProperties.InvestmentTarget.ECONOMIC] = GameGlobals.roundBudget - env;
        return inv;
    }
    
    
    public int PredictAction(Player player)
    {
        float coopValue = (player.GetCoopPerc() * GameGlobals.roundBudget);
        return UnityEngine.Random.Range(Mathf.FloorToInt(coopValue), Mathf.CeilToInt(coopValue));
    }
}