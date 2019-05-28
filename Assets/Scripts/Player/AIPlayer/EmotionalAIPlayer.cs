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

public class EmotionalAIPlayer: AIPlayer
{
    //Emotional stuff
    private RolePlayCharacterAsset rpc;
    List<WellFormedNames.Name> unperceivedEvents;
    List<ActionLibrary.IAction> actionList;

    private Dictionary<GameProperties.InvestmentTarget, int> investmentIntentions;

    public EmotionalAIPlayer(IInteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name, float updateDelay) :
        base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        unperceivedEvents = new List<WellFormedNames.Name>();
        actionList = new List<ActionLibrary.IAction>();
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
    }

    public override void Perceive(List<WellFormedNames.Name> events)
    {
        unperceivedEvents.AddRange(events);
    }
    public override List<ActionLibrary.IAction> GetActionList()
    {
        return this.actionList;
    }
    public override EmotionalAppraisal.IActiveEmotion GetMyStrongestEmotion()
    {
        return this.rpc.GetStrongestActiveEmotion();
    }


    public void Act()
    {
        foreach(ActionLibrary.IAction action in actionList)
        {
            switch (action.Key.ToString())
            {
                case "Speak":
                    interactionModule.Speak(action.Parameters[1].ToString());
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
        if (unperceivedEvents.Count > 0)
        {
            rpc.Perceive(unperceivedEvents);
            unperceivedEvents.Clear();
        }
        List<ActionLibrary.IAction> newActionList = rpc.Decide().ToList<ActionLibrary.IAction>();

        bool listsAreEqual = true;

        if(actionList.Count != newActionList.Count)
        {
            listsAreEqual = false;
        }
        else
        {
            foreach (ActionLibrary.IAction action1 in newActionList)
            {
                foreach (ActionLibrary.IAction action2 in actionList)
                {
                    if(action1.Name != action2.Name)
                    {
                        listsAreEqual = false;
                    }
                }
            }
        }
        
        

        // Update Actions and Act only if It is a different actionList
        if (!listsAreEqual)
        {
            actionList = newActionList;
            Act();
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
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("InvestmentResult("+ player.GetName() + ", Environment)", player.lastEnvironmentResult.ToString("0.00", CultureInfo.InvariantCulture), "World"));
            events.Add(RolePlayCharacter.EventHelper.PropertyChange("InvestmentResult(" + player.GetName() + ", Economy)", player.lastEconomicResult.ToString("0.00", CultureInfo.InvariantCulture), "World"));
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
