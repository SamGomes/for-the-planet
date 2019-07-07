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


    public EmotionalAIPlayer(InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
        base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        unperceivedEvents = new List<WellFormedNames.Name>();
        InitRPC(fatimaRpcPath);

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
        rpc = RolePlayCharacterAsset.LoadFromFile(rpcPath.Source);
        rpc.LoadAssociatedAssets();
        GameGlobals.FAtiMAIat.BindToRegistry(rpc.DynamicPropertiesRegistry);

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

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        //foreach(Player player in GameGlobals.players)
        //{
        //    string econ = player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture);
        //    string env = player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture);
        //    //goal success probability should be obtained using a method which could be overriden according to the personality of the agent
        //    events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + econ + ","+ env +"," + env + ")", player.GetName(), this.name));
        //}
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + GameGlobals.envState + ")", GetName(), this.name));
        Perceive(events);
        //in simulation compute the update imediately after
        //if (GameGlobals.isSimulation)
        //{
        UpdateStep();
        //}

        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        yield return base.AutoInvestmentExecution();

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        //foreach(Player player in GameGlobals.players)
        //{
        //    string econ = player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture);
        //    string env = player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture);
        //    //goal success probability should be obtained using a method which could be overriden according to the personality of the agent
        //    events.Add(RolePlayCharacter.EventHelper.PropertyChange("BudgetExecution(" + econ + ","+ env +"," + env + ")", player.GetName(), this.name));
        //}
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("DecaySimulation(" + GameGlobals.envState + ")", GetName(), this.name));
        Perceive(events);
        //in simulation compute the update imediately after
        //if (GameGlobals.isSimulation)
        //{
        UpdateStep();
        //}

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

public class DisruptiveConstructiveEmotionalAIPlayer : EmotionalAIPlayer
{
    float pDisrupt;
    Dictionary<string, float> pDeltas;

    public DisruptiveConstructiveEmotionalAIPlayer(InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay, string fatimaRpcPath) :
       base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name, updateDelay, fatimaRpcPath)
    {
        pDisrupt = 0.5f;

        pDeltas = new Dictionary<string, float>();
        pDeltas["Happy-for"] = pDeltas["Gloating"] = pDeltas["Satisfaction"] = 
            pDeltas["Relief"] = pDeltas["Hope"] = pDeltas["Joy"] = pDeltas["Gratification"] = 
            pDeltas["Gratitude"] = pDeltas["Pride"] = pDeltas["Admiration"] = pDeltas["Love"] = -0.1f;

        pDeltas["Resentment"] = pDeltas["Pity"] = pDeltas["Fear-confirmed"] =
           pDeltas["Disappointment"] = pDeltas["Fear"] = pDeltas["Distress"] = pDeltas["Remorse"] =
           pDeltas["Anger"] = pDeltas["Shame"] = pDeltas["Reproach"] = pDeltas["Hate"] = 0.1f;
    }

    public override void Act()
    {
        List<EmotionalAppraisal.DTOs.EmotionDTO> emotionList = this.GetAllActiveEmotions();
        emotionList = emotionList.Distinct().ToList();
        string emotionListStr = "";
        foreach (EmotionalAppraisal.DTOs.EmotionDTO dto in emotionList)
        {
            pDisrupt += pDeltas[dto.Type];
            emotionListStr += dto.Type + ",";
        }
        Debug.Log("Emotion List: " + emotionListStr);

        if(pDisrupt> 0.9f)
        {
            pDisrupt = 0.9f;
        }
        if (pDisrupt < 0.1f)
        {
            pDisrupt = 0.1f;
        }

        int econ = 0;
        for (int i = 0; i < 5; i++){
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
}