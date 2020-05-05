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
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using UnityEngine.TestTools;

//Using the MCTS implementation originally provided by @Mikeywalsh



public class EmotionalAIPlayer: AIPlayer
{
    
    //Emotional stuff
    protected RolePlayCharacterAsset rpc;
    private List<WellFormedNames.Name> unperceivedEvents;

    protected Dictionary<GameProperties.InvestmentTarget, int> investmentIntentions;
    protected string fatimaRpcPath;

    private float lastMoneyInc;
    private float lastEnvInc;

    public EmotionalAIPlayer(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        lastEnvInc = 0.0f;
        lastMoneyInc = 0.0f;
        
        unperceivedEvents = new List<WellFormedNames.Name>();
        this.rpc = GameGlobals.FAtiMAIat.Characters.FirstOrDefault(x => x.CharacterName.ToString() == this.type.ToString());
        this.rpc.CharacterName =  (WellFormedNames.Name) ("Agent" + this.id.ToString());
        this.fatimaRpcPath = fatimaRpcPath;

        investmentIntentions = new Dictionary<GameProperties.InvestmentTarget, int>();

        //default investment intention
        investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] = 3;
        investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] = 2;
    }

    public override void Perceive(List<WellFormedNames.Name> events)
    {
        unperceivedEvents.AddRange(events);
    }
    public List<EmotionalAppraisal.DTOs.EmotionDTO> GetAllActiveEmotions()
    {
        return this.rpc.GetAllActiveEmotions().ToList();
    }

    protected string ReplaceVariablesInDialogue(string dialog, Dictionary<string, string> tags)
    {
        var tokens = Regex.Matches(dialog, @"\|.*?\|");

        var result = string.Empty;
        foreach (Match t in tokens)
        {
            string strippedT = t.Value.Replace(@"|", "");
            var valueToReplace = tags[strippedT];// rpc.GetBeliefValue(t);
            dialog = dialog.Replace(t.Value, valueToReplace);
        }
        return dialog;
    }
    public RolePlayCharacterAsset GetRpc()
    {
        return this.rpc;
    }


    public virtual void Act() { }


    protected void UpdateStep()
    {
        if (unperceivedEvents.Count > 0)
        {
            rpc.Perceive(unperceivedEvents);
            unperceivedEvents.Clear();
        }
        try
        {
            Act();
        }
        catch (Exception e)
        {
            Debug.Log("Could not act due to the following exception: "+e.ToString());
        }
        rpc.Update();
    }

    public IEnumerator EmotionalUpdateLoop(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(delay));
    }

    public override IEnumerator AutoBudgetAllocation()
    {
        base.AutoBudgetAllocation();

        int econ = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC];
        int env = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT];

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("BeforeBudgetAllocation(" + econ + "," + env + ")", this.id.ToString(), this.id.ToString()));
        Perceive(events);
        UpdateStep();
        
        econ = investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC];
        env = investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT];
        
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
//            econ = player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC];
//            env = player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
//
//            events.Add(RolePlayCharacter.EventHelper.PropertyChange(
//                "HistoryDisplay(" + econ.ToString("0.00", CultureInfo.InvariantCulture) + "," +
//                env.ToString("0.00", CultureInfo.InvariantCulture) + ")",
//                this.id.ToString(), player.GetId().ToString()));

            //aggregate
            econ += player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC];
            env += player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
        }
        //aggregate
        econ /= GameGlobals.players.Count;
        env /= GameGlobals.players.Count;
        events.Add(RolePlayCharacter.EventHelper.PropertyChange(
        "HistoryDisplay(" + econ.ToString("0.00", CultureInfo.InvariantCulture) + "," +
        env.ToString("0.00", CultureInfo.InvariantCulture) + ")",
        this.id.ToString(), "OtherPlayers"));
        
        Perceive(events);
        UpdateStep();

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

public class TableEmotionalAIPlayer : EmotionalAIPlayer
{

    public TableEmotionalAIPlayer(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name, updateDelay, fatimaRpcPath)
    {
        
    }


    public override void Act()
    {
        List<ActionLibrary.IAction> actionList = rpc.Decide().ToList<ActionLibrary.IAction>();
        foreach (ActionLibrary.IAction action in actionList)
        {
            switch (action.Key.ToString())
            {
                case "Speak":
                    WellFormedNames.Name cs = action.Parameters[0];
                    WellFormedNames.Name ns = action.Parameters[1];
                    WellFormedNames.Name m = (WellFormedNames.Name)"-";
                    if (rpc.GetStrongestActiveEmotion() != null)
                    {
                        m = (WellFormedNames.Name)rpc.GetStrongestActiveEmotion().EmotionType;
                    }

                    WellFormedNames.Name s = (WellFormedNames.Name)this.name; //ESTA MAL
                    var dialogs = GameGlobals.FAtiMAIat.GetDialogueActions(cs, ns, m, s);
                    if (dialogs.Count <= 0)
                    {
                        break;
                    }
                    var dialog = dialogs.Shuffle().FirstOrDefault();

                    interactionModule.Speak(ReplaceVariablesInDialogue(dialog.Utterance, new Dictionary<string, string>() { { "target", action.Target.ToString() } }));

                    WellFormedNames.Name speakEvent = RolePlayCharacter.EventHelper.ActionEnd(this.name, "Speak(" + cs + "," + ns + "," + m + "," + s + ")", this.name);
                    Perceive(new List<WellFormedNames.Name>() { speakEvent });

                    break;
                case "Invest":
                    investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] = int.Parse(action.Parameters[0].ToString());
                    investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] = int.Parse(action.Parameters[1].ToString());
                    break;
            }

        }
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
    public FTPBoard(int maxGameRound, Func<Player, int> actionPredictorCallback, float env, List<float> econs): this()
    {
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
        foreach(Player player in GameGlobals.players)
        {
            float estGainEcon = 0.0f;
            float estDecayEcon = (GameGlobals.playerDecayBudget[0] + GameGlobals.playerDecayBudget[1]) / 2.0f;

            if (player.GetId() == CurrentPlayer)
            {
                estEnvDice += myInvestmentEnv;
                estGainEcon =((GameGlobals.roundBudget - myInvestmentEnv)*3.5f)/ 100.0f;
            }
            else
            { 
                int opponentEnvDice = actionPredictorCallback(player);
                estEnvDice += opponentEnvDice;
                estGainEcon =((GameGlobals.roundBudget - opponentEnvDice)*3.5f)/ 100.0f;

            }
            estEcons.Add(estGainEcon - estDecayEcon);
        }

        float estGainEnv = (estEnvDice * 3.5f) / 100.0f;
        float estDecayEnv = (GameGlobals.environmentDecayBudget[0] + GameGlobals.environmentDecayBudget[1]) / 2.0f;
        estDecayEnv = (estDecayEnv*3.5f) / 100.0f;
        
        float estEnv = estGainEnv - estDecayEnv;
        
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
        if (env < 0.05)
        {
            this.winner = 0;
        }
        else if (currGameRound >= maxGameRound)
        {
            float bestEcon = -1.0f;
            int bestPlayerId = 0;
            int winnerCount = 0;
            for (int i = 0; i < GameGlobals.players.Count; i++)
            {
                float currEcon = econs[i];
                if (bestEcon >= currEcon)
                {
                    bestEcon = currEcon;
                    bestPlayerId = i;
                    winnerCount++;
                }
            }

            if (winnerCount > 1)
            {
                this.winner = 0;
            }
            else
            {
                this.winner = bestPlayerId + 1;
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
    float pDisrupt;
    Dictionary<string, float> pWeights;
    
    
    // big brain
    private static TreeSearch<Node> mcts;

    
    public CompetitiveCooperativeEmotionalAIPlayer(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
       base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name, updateDelay, fatimaRpcPath)
    {
        pDisrupt = 0.5f;

        pWeights = new Dictionary<string, float>();
        pWeights["Happy-for"] = pWeights["Gloating"] = pWeights["Satisfaction"] = 
            pWeights["Relief"] = pWeights["Hope"] = pWeights["Joy"] = pWeights["Gratification"] = 
            pWeights["Gratitude"] = pWeights["Pride"] = pWeights["Admiration"] = pWeights["Love"] = -1.0f;

        pWeights["Resentment"] = pWeights["Pity"] = pWeights["Fear-confirmed"] =
           pWeights["Disappointment"] = pWeights["Fear"] = pWeights["Distress"] = pWeights["Remorse"] =
           pWeights["Anger"] = pWeights["Shame"] = pWeights["Reproach"] = pWeights["Hate"] = 1.0f;
    }

    public override void Act()
    {
        EmotionalAppraisal.IActiveEmotion strongestEmotion = this.rpc.GetStrongestActiveEmotion();
        if (strongestEmotion == null) {
            return;
        }

        string rpcArr = "[";
        int j = 0;
        
        interactionModule.Speak("I'm feeling " + strongestEmotion.EmotionType);
        
        float pMood = (rpc.Mood + 10.0f) / 20.0f; //negative rule: positive mood-> invest in econ

        List<float> moneyValues = new List<float>();
        foreach (Player player in GameGlobals.players)
        {
            moneyValues.Add(player.GetMoney());
        }

        int numMctsSteps = 1 + (int)((1.0f + pMood/-20.0f)/2.0f * 9);
        FTPBoard currentState = new FTPBoard(20, PredictAction, GameGlobals.envState, moneyValues);
        investmentIntentions = Analyse(numMctsSteps, currentState); 
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
        int env = 0;
        for (int i = 0; i < (GameGlobals.roundBudget); i++){
            if (UnityEngine.Random.Range(0.0f, 1.0f) < player.GetCoopPerc())
            {
                env++;
            }
        }
        int econ = GameGlobals.roundBudget - env;
        return env;
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