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

    public EmotionalAIPlayer(IInteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay) :
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

        //rpc.CharacterName = (WellFormedNames.Name) this.name;
    }

    public override void Perceive(List<WellFormedNames.Name> events)
    {
        unperceivedEvents.AddRange(events);
    }
    public override EmotionalAppraisal.IActiveEmotion GetMyStrongestEmotion()
    {
        return this.rpc.GetStrongestActiveEmotion();
    }

    private string ReplaceVariablesInDialogue(string dialog)
    {
        var tokens = Regex.Split(dialog, @"|");

        var result = string.Empty;
        bool process = false;
        foreach (var t in tokens)
        {
            if (process)
            {
                var beliefValue = "aaaa";// rpc.GetBeliefValue(t);
                result += beliefValue;
                process = false;
            }
            else if (t == string.Empty)
            {
                process = true;
                continue;
            }
            else
            {
                result += t;
            }
        }
        return result;
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
                        
                    WellFormedNames.Name s = action.Parameters[3];
                    var dialogs = GameGlobals.FAtiMAIat.GetDialogueActions(cs, ns, m, s);
                    if(dialogs.Count <= 0)
                    {
                        break;
                    }
                    var dialog = dialogs.Shuffle().FirstOrDefault();
                    interactionModule.Speak(ReplaceVariablesInDialogue(dialog.Utterance));
                    break;
                case "Invest":
                    investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC] =  int.Parse(action.Parameters[0].ToString());
                    investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT] =  int.Parse(action.Parameters[1].ToString());
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
    public override IEnumerator AutoBudgetAlocation()
    {
        yield return InvestInEconomy(investmentIntentions[GameProperties.InvestmentTarget.ECONOMIC]);
        yield return InvestInEnvironment(investmentIntentions[GameProperties.InvestmentTarget.ENVIRONMENT]);
        yield return EndBudgetAllocationPhase();
    }

    public override IEnumerator AutoBudgetExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        yield return ApplyInvestments();

        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        foreach(Player player in GameGlobals.players)
        {
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("InvestmentResults("+ player.GetName() + ", Environment)", player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture), "World"));
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("InvestmentResults(" + player.GetName() + ", Economy)", player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture), "World"));
        }
       Perceive(events);
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }

    public override IEnumerator AutoInvestmentExecution()
    {
        base.AutoInvestmentExecution();
        List<WellFormedNames.Name> events = new List<WellFormedNames.Name>();
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("SimulationDecay(World, Environment)", gameManagerRef.lastEnvDecay.ToString("0.00", CultureInfo.InvariantCulture), "World"));
        events.Add(RolePlayCharacter.EventHelper.PropertyChange("SimulationDecay(" + name + ", Economy)", lastEconomicDecay.ToString("0.00", CultureInfo.InvariantCulture), "World"));
        Perceive(events);

        yield return null;
    }
}
