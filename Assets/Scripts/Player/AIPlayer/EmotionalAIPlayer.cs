using RolePlayCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using Utilities;

public class EmotionalAIPlayer: AIPlayer
{
    //Emotional stuff
    protected RolePlayCharacterAsset rpc;
    private List<WellFormedNames.Name> unperceivedEvents;

    protected Dictionary<GameProperties.InvestmentTarget, int> investmentIntentions;
    protected string fatimaRpcPath;

    public EmotionalAIPlayer(InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
        base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        unperceivedEvents = new List<WellFormedNames.Name>();
        InitRPC(fatimaRpcPath);
        this.fatimaRpcPath = fatimaRpcPath;

        investmentIntentions = new Dictionary<GameProperties.InvestmentTarget, int>();

        //default investment intention
        investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] = 3;
        investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] = 2;

        //if (!GameGlobals.isSimulation) {
        //    playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(updateDelay));
        //}
    }

    public void InitRPC(string fatimaRpcPath)
    {
        IntegratedAuthoringTool.DTOs.CharacterSourceDTO rpcPath = GameGlobals.FAtiMAIat.GetAllCharacterSources().FirstOrDefault<IntegratedAuthoringTool.DTOs.CharacterSourceDTO>();
        if (fatimaRpcPath != null && fatimaRpcPath != "")
        {
            rpcPath = GameGlobals.FAtiMAIat.GetAllCharacterSources().Where(data => data.RelativePath == fatimaRpcPath).FirstOrDefault();
        }
        if (rpcPath == null)
        {
            warningScreenRef.DisplayPoppup("error loading fatimaRpcPath= " + fatimaRpcPath);
        }

        string rpcSource = rpcPath.Source;
        if (!GameGlobals.storedRPCs.ContainsKey(rpcSource))
        {
            rpc = RolePlayCharacterAsset.LoadFromFile(rpcSource);
            rpc.LoadAssociatedAssets();
            GameGlobals.FAtiMAIat.BindToRegistry(rpc.DynamicPropertiesRegistry);
            GameGlobals.storedRPCs[rpcSource] = rpc;
        }
        else
        {
            rpc = GameGlobals.storedRPCs[rpcSource];
        }
        rpc.ResetEmotionalState();


        rpc.CharacterName = (WellFormedNames.Name) this.name;
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
        { }


        rpc.Update();
    }

    public IEnumerator EmotionalUpdateLoop(float delay)
    {
        UpdateStep();
        yield return new WaitForSeconds(delay);
        playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(delay));
    }

    //void OnApplicationQuit()
    //{
    //    this.rpc.SaveToFile(Application.streamingAssetsPath + "/Runtimed/" + name + ".rpc");
    //}
    public override IEnumerator AutoBudgetAllocation()
    {
        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        //string econS = GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0.00", CultureInfo.InvariantCulture);
        //string envS = GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0.00", CultureInfo.InvariantCulture);
        //events.Add(RolePlayCharacter.EventHelper.PropertyChange("HistoryDisplay(" + econS + "," + envS + ")", GetName(), this.name));
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("BeforeBudgetAllocation(" + money.ToString("0.00", CultureInfo.InvariantCulture) + "," + GameGlobals.envState.ToString("0.00", CultureInfo.InvariantCulture) + ")", this.name, this.name));
        Perceive(events);
        //in simulation compute the update imediately after
        //if (GameGlobals.isSimulation)
        //{
            UpdateStep();
        //}


        base.AutoBudgetAllocation();

        int econ = investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC];
        int env = investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT];

        yield return InvestInEconomy(econ);
        yield return InvestInEnvironment(env);
        yield return EndBudgetAllocationPhase();

    }

    public override IEnumerator AutoHistoryDisplay()
    {
        yield return base.AutoHistoryDisplay();

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        foreach (Player player in GameGlobals.players)
        {
            string econ = player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0.00", CultureInfo.InvariantCulture);
            string env = player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0.00", CultureInfo.InvariantCulture);
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("HistoryDisplay(" + econ + ","+ env +")", player.GetName(), this.name));
        }
        Perceive(events);
        //in simulation compute the update imediately after
        //if (GameGlobals.isSimulation)
        //{
        UpdateStep();
        //}

        yield return null;
    }

    public override IEnumerator AutoBudgetExecution()
    {
        yield return base.AutoBudgetExecution();

        //// @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        //yield return ApplyInvestments();

        //List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        ////foreach(Player player in GameGlobals.players)
        ////{
        ////    string econ = player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture);
        ////    string env = player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture);
        ////    //goal success probability should be obtained using a method which could be overriden according to the personality of the agent
        ////    events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + econ + ","+ env +"," + env + ")", player.GetName(), this.name));
        ////}
        //events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + GameGlobals.envState + ")", GetName(), this.name));
        //Perceive(events);
        ////in simulation compute the update imediately after
        ////if (GameGlobals.isSimulation)
        ////{
        //UpdateStep();
        ////}

        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        yield return base.AutoInvestmentExecution();

        //List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        ////foreach(Player player in GameGlobals.players)
        ////{
        ////    string econ = player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture);
        ////    string env = player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture);
        ////    //goal success probability should be obtained using a method which could be overriden according to the personality of the agent
        ////    events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + econ + ","+ env +"," + env + ")", player.GetName(), this.name));
        ////}
        //events.Add(RolePlayCharacter.EventHelper.PropertyChange("DecaySimulation(" + GameGlobals.envState + ")", GetName(), this.name));
        //Perceive(events);
        ////in simulation compute the update imediately after
        ////if (GameGlobals.isSimulation)
        ////{
        //UpdateStep();
        ////}

        yield return null;
    }
}

public class TableEmotionalAIPlayer : EmotionalAIPlayer
{

    public TableEmotionalAIPlayer(InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
        base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name, updateDelay, fatimaRpcPath)
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
                    //WellFormedNames.Name m = action.Parameters[2];
                    //WellFormedNames.Name m = (WellFormedNames.Name) "happy-for";// (WellFormedNames.Name) rpc.GetStrongestActiveEmotion().EmotionType;
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

                    Debug.Log(rpc.GetStrongestActiveEmotion().ToString());
                    //rpc.TellWorkingMemory("emotionTarget", rpc.GetStrongestActiveEmotion().ToString());

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

public class CompetitiveCooperativeEmotionalAIPlayer : EmotionalAIPlayer
{
    bool isDisruptive;
    float pDisrupt;
    Dictionary<string, float> pWeights;

    public CompetitiveCooperativeEmotionalAIPlayer(InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath, bool isDisruptive) :
       base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name, updateDelay, fatimaRpcPath)
    {
        this.isDisruptive = isDisruptive;
        pDisrupt = 0.5f;

        pWeights = new Dictionary<string, float>();
        pWeights["Happy-for"] = pWeights["Gloating"] = pWeights["Satisfaction"] = 
            pWeights["Relief"] = pWeights["Hope"] = pWeights["Joy"] = pWeights["Gratification"] = 
            pWeights["Gratitude"] = pWeights["Pride"] = pWeights["Admiration"] = pWeights["Love"] = -1.0f;

        pWeights["Resentment"] = pWeights["Pity"] = pWeights["Fear-confirmed"] =
           pWeights["Disappointment"] = pWeights["Fear"] = pWeights["Distress"] = pWeights["Remorse"] =
           pWeights["Anger"] = pWeights["Shame"] = pWeights["Reproach"] = pWeights["Hate"] = 1.0f;
    }

    public override string GetPlayerType()
    {
        if (isDisruptive)
        {
            return "Competitive";
        }
        else
        {
            return "Cooperative";

        }
    }

    public override void Act()
    {
        EmotionalAppraisal.IActiveEmotion strongestEmotion = this.rpc.GetStrongestActiveEmotion();

        if (strongestEmotion == null) {
            return;
        }


        Dictionary<string, string> eventLogEntry = new Dictionary<string, string>();
        eventLogEntry["currSessionId"] = GameGlobals.currSessionId.ToString();
        eventLogEntry["currGameId"] = GameGlobals.currGameId.ToString();
        eventLogEntry["currGameCondition"] = GameGlobals.currGameCondition.ToString();
        eventLogEntry["currGameRoundId"] = GameGlobals.currGameRoundId.ToString();
        eventLogEntry["currGamePhase"] = gameManagerRef.GetCurrGamePhase().ToString();
        eventLogEntry["playerId"] = this.id.ToString();
        eventLogEntry["playerType"] = this.GetPlayerType();
        eventLogEntry["emotionType"] = strongestEmotion.EmotionType;
        eventLogEntry["intensity"] = strongestEmotion.Intensity.ToString();
        //eventLogEntry["causeEventName"] = strongestEmotion.GetCause(rpc.).EventName;
        playerMonoBehaviourFunctionalities.StartCoroutine(GameGlobals.gameLogManager.WriteToLog("fortheplanetlogs", "feltEmotionsLog", eventLogEntry));

        interactionModule.Speak("I'm feeling " + strongestEmotion.EmotionType);

        pDisrupt = pWeights[strongestEmotion.EmotionType] * strongestEmotion.Intensity / 10.0f;

        if(pDisrupt > 0.95f)
        {
            pDisrupt = 0.95f;
        }
        if (pDisrupt < 0.05f)
        {
            pDisrupt = 0.05f;
        }

        int econ = 0;
        for (int i = 0; i < (GameGlobals.roundBudget); i++){
            if (UnityEngine.Random.Range(0.0f, 1.0f) < pDisrupt)
            {
                econ++;
            }
        }
        int env = GameGlobals.roundBudget - econ;
    
        investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] = env;
        investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] = econ;

        pDisrupt = 0.5f;
    }


    protected float CalcGoalSuccessProbabilityInvestment(float state, int numDice)
    {
        float gsp = 0.0f;
        float threshold = 0.6f; //property

        float numDecayDice = GameGlobals.environmentDecayBudget; //property
        float maxInvest = 6 * numDice / 100.0f;
        float avgInvest = 3 * numDice / 100.0f;
        float minInvest = 1 * numDice / 100.0f;

        if (state + minInvest >= threshold)
        {
            gsp = 1;
        }
        else if (state + avgInvest >= threshold)
        {
            gsp = 0.75f;
        }
        else if (state + maxInvest >= threshold)
        {
            gsp = 0.25f;
        }
        return gsp;
    }

    protected float CalcGoalSuccessProbabilityDecay(float state)
    {
        float gsp = 0.0f;
        float threshold = 0.6f; //property

        float numDecayDice = GameGlobals.environmentDecayBudget; //property
        float maxDecay = 6 * numDecayDice / 100.0f;
        float avgDecay = 3 * numDecayDice / 100.0f;
        float minDecay = 1 * numDecayDice / 100.0f;

        if (state - maxDecay >= threshold)
        {
            gsp = 1;
        }
        else if (state - avgDecay >= threshold )
        {
            gsp = 0.75f;
        }
        else if (state - minDecay >= threshold)
        {
            gsp = 0.25f;
        }
        return gsp;
    }

    public override IEnumerator AutoBudgetExecution()
    {
        yield return base.AutoBudgetExecution();

        float state = 0.0f;
        float gsp = 0.0f;
        if (isDisruptive)
        {
            state = this.GetMoney();
            gsp = CalcGoalSuccessProbabilityInvestment(state, this.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC]);
        }
        else
        {
            state = GameGlobals.envState;
            gsp = CalcGoalSuccessProbabilityInvestment(state, this.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT]);
        }
        
        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + gsp + ")", GetName(), this.name));
        Perceive(events);
        UpdateStep();
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        yield return base.AutoInvestmentExecution();

        float state = 0.0f;
        if (isDisruptive)
        {
            state = this.GetMoney();
        }
        else
        {
            state = GameGlobals.envState;

        }
        float gsp = CalcGoalSuccessProbabilityDecay(state);

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("DecaySimulation(" + gsp + ")", GetName(), this.name));
        Perceive(events);
        UpdateStep();
        
    }


}