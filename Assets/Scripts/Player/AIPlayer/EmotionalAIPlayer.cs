using RolePlayCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using EmotionalAppraisal.DTOs;
using UnityEngine;
using UnityEngine.Networking;
using Utilities;
using Object = System.Object;

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

        //if (!GameGlobals.isSimulation) {
        //    playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(updateDelay));
        //}
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
    public RolePlayCharacterAsset getRPC()
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
            econ = player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC];
            env = player.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ENVIRONMENT];
       
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("HistoryDisplay(" + econ.ToString("0.00", CultureInfo.InvariantCulture) + "," + env.ToString("0.00", CultureInfo.InvariantCulture) + ")",
                this.id.ToString(), player.GetId().ToString()));
            
        }
        econ /= GameGlobals.players.Count;
        env /= GameGlobals.players.Count;
            
        string econStr = econ.ToString("0.00", CultureInfo.InvariantCulture);
        string envStr = env.ToString("0.00", CultureInfo.InvariantCulture);
        
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

public class CompetitiveCooperativeEmotionalAIPlayer : EmotionalAIPlayer
{
    float pDisrupt;
    Dictionary<string, float> pWeights;

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

//        pDisrupt = pWeights[strongestEmotion.EmotionType] * ((strongestEmotion.Intensity + 10.0f) / 20.0f);

//        pDisrupt = 1.0f - (rpc.Mood + 10.0f) / 20.0f; //positive rule: positive mood-> invest in env
        pDisrupt = (rpc.Mood + 10.0f) / 20.0f; //negative rule: positive mood-> invest in econ
        
        //bounded between 5 and 95%
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
        float threshold = 0.6f;

        float numDecayDice = (GameGlobals.environmentDecayBudget[0] + GameGlobals.environmentDecayBudget[1]) / 2.0f;
        float maxDecay = 6 * numDecayDice / 100.0f;
        float avgDecay = 3 * numDecayDice / 100.0f;
        float minDecay = 1 * numDecayDice / 100.0f;

        if (state - maxDecay >= threshold)
        {
            gsp = 1;
        }
        else if (state - avgDecay >= threshold)
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

        float state = this.GetMoney();
        float gsp = CalcGoalSuccessProbabilityInvestment(state, this.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC]);
        
        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
//        events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + gsp.ToString("0.00", CultureInfo.InvariantCulture) + ")", this.id.ToString(), this.id.ToString()));
        Perceive(events);
        UpdateStep();
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        yield return base.AutoInvestmentExecution();

        float state = this.GetMoney();
        float gsp = CalcGoalSuccessProbabilityDecay(state);

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
//        events.Add(RolePlayCharacter.EventHelper.PropertyChange("DecaySimulation(" + gsp.ToString("0.00", CultureInfo.InvariantCulture) + ")", this.id.ToString(), this.id.ToString()));
        Perceive(events);
        UpdateStep();
        
    }

}