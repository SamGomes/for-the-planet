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
    private RolePlayCharacterAsset rpc;
    List<WellFormedNames.Name> unperceivedEvents;

    private Dictionary<GameProperties.InvestmentTarget, int> investmentIntentions;

    public EmotionalAIPlayer(InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay) :
        base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        unperceivedEvents = new List<WellFormedNames.Name>();
        InitRPC();

        investmentIntentions = new Dictionary<GameProperties.InvestmentTarget, int>();

        //default investment intention
        investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] = 3;
        investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] = 2;


        playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(updateDelay));
    }

    public void InitRPC()
    {
        IntegratedAuthoringTool.DTOs.CharacterSourceDTO rpcPath = GameGlobals.FAtiMAIat.GetAllCharacterSources().FirstOrDefault<IntegratedAuthoringTool.DTOs.CharacterSourceDTO>();
        Debug.Log(rpcPath.RelativePath);
        rpc = RolePlayCharacterAsset.LoadFromFile(rpcPath.Source);
        rpc.LoadAssociatedAssets();
        GameGlobals.FAtiMAIat.BindToRegistry(rpc.DynamicPropertiesRegistry);

        rpc.CharacterName = (WellFormedNames.Name) this.name;
    }

    public override void Perceive(List<WellFormedNames.Name> events)
    {
        unperceivedEvents.AddRange(events);
    }
    public override EmotionalAppraisal.IActiveEmotion GetMyStrongestEmotion()
    {
        return this.rpc.GetStrongestActiveEmotion();
    }

    private string ReplaceVariablesInDialogue(string dialog, Dictionary<string, string> tags)
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


    public void Act(List<ActionLibrary.IAction> actionList)
    {
        foreach(ActionLibrary.IAction action in actionList)
        {
            switch (action.Key.ToString())
            {
                case "Speak":
                    WellFormedNames.Name cs = action.Parameters[0];
                    WellFormedNames.Name ns = action.Parameters[1];
                    //WellFormedNames.Name m = action.Parameters[2];
                    //WellFormedNames.Name m = (WellFormedNames.Name) "happy-for";// (WellFormedNames.Name) rpc.GetStrongestActiveEmotion().EmotionType;
                    WellFormedNames.Name m = (WellFormedNames.Name) "-";
                    if (rpc.GetStrongestActiveEmotion()!= null)
                    {
                        m = (WellFormedNames.Name) rpc.GetStrongestActiveEmotion().EmotionType;
                    }
                        
                    WellFormedNames.Name s = (WellFormedNames.Name) this.name; //ESTA MAL
                    var dialogs = GameGlobals.FAtiMAIat.GetDialogueActions(cs, ns, m, s);
                    if(dialogs.Count <= 0)
                    {
                        break;
                    }
                    var dialog = dialogs.Shuffle().FirstOrDefault();
                    
                    interactionModule.Speak(ReplaceVariablesInDialogue(dialog.Utterance, new Dictionary<string, string>() { { "target", action.Target.ToString() }}));

                    Debug.Log(rpc.GetStrongestActiveEmotion().ToString());
                    //rpc.TellWorkingMemory("emotionTarget", rpc.GetStrongestActiveEmotion().ToString());

                    WellFormedNames.Name speakEvent = RolePlayCharacter.EventHelper.ActionEnd(this.name,"Speak("+cs+","+ns+"," + m + ","+ s + ")", this.name);
                    Perceive(new List<WellFormedNames.Name>() { speakEvent });

                    break;
                case "Invest":
                    investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] =  int.Parse(action.Parameters[0].ToString());
                    investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] =  int.Parse(action.Parameters[1].ToString());
                    break;
            }

        }
    }


    public IEnumerator EmotionalUpdateLoop(float delay)
    {
        string print = "Player: " + this.name + " feels ";
        EmotionalAppraisal.IEmotion strongestEmotion = rpc.GetStrongestActiveEmotion();
        if (strongestEmotion != null) {
            print += strongestEmotion.EmotionType;
        } else {
            print += "nothing";
        }
        Debug.Log(print);

        if (unperceivedEvents.Count > 0)
        {
            rpc.Perceive(unperceivedEvents);
            unperceivedEvents.Clear();
        }

        try
        {
            List<ActionLibrary.IAction> actionList = rpc.Decide().ToList<ActionLibrary.IAction>();
            Act(actionList);
        }
        catch(Exception e)
        {
            int y = 2;
        }


        rpc.Update();
        yield return new WaitForSeconds(delay);
        playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(delay));
    }

    //void OnApplicationQuit()
    //{
    //    this.rpc.SaveToFile(Application.streamingAssetsPath + "/Runtimed/" + name + ".rpc");
    //}
    public override IEnumerator AutoBudgetAllocation()
    {
        base.AutoBudgetAllocation();

        int econ = investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC];
        int env = investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT];

        yield return InvestInEconomy(econ);
        yield return InvestInEnvironment(env);
        yield return EndBudgetAllocationPhase();

        //List<WellFormedNames.Name> events = new List<WellFormedNames.Name>(){ RolePlayCharacter.EventHelper.PropertyChange("AllocateBudget(" + econ.ToString("0.00", CultureInfo.InvariantCulture) + "," + env.ToString("0.00", CultureInfo.InvariantCulture) + ")", this.name, this.name)  };
        //Perceive(events);
    }

    public override IEnumerator AutoHistoryDisplay()
    {
        base.AutoHistoryDisplay();


        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        foreach (Player player in GameGlobals.players)
        {
            string econ = player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC].ToString("0.00", CultureInfo.InvariantCulture);
            string env = player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString("0.00", CultureInfo.InvariantCulture);
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("InvestmentHistory(" + econ + ","+ env +")", player.GetName(), this.name));
        }
        Perceive(events);

        yield return null;
    }

    public override IEnumerator AutoBudgetExecution()
    {
        base.AutoBudgetExecution();


        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        yield return ApplyInvestments();

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        foreach(Player player in GameGlobals.players)
        {
            string econ = player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture);
            string env = player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture);
            //goal success probability should be obtained using a method which could be overriden according to the personality of the agent
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("InvestmentResults(" + econ + ","+ env +"," + env + ")", player.GetName(), this.name));
        }
        Perceive(events);
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        base.AutoInvestmentExecution();

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        foreach (Player player in GameGlobals.players)
        {
            string econ = player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture);
            string env = player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture);
            //goal success probability should be obtained using a method which could be overriden according to the personality of the agent
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("Decay(" + econ + "," + env + "," + env + ")", player.GetName(), this.name));
        }
        Perceive(events);

        yield return null;
    }
}
