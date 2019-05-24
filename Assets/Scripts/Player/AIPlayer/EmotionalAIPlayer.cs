using RolePlayCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
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
    List<ActionLibrary.IAction> whatICanDo;

    public EmotionalAIPlayer(IInteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        unperceivedEvents = new List<WellFormedNames.Name>();
        whatICanDo = new List<ActionLibrary.IAction>();
        InitRPC();

        playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(0.2f));
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
    public override List<ActionLibrary.IAction> GetWhatICanDo()
    {
        return this.whatICanDo;
    }
    public override EmotionalAppraisal.IActiveEmotion GetMyStrongestEmotion()
    {
        return this.rpc.GetStrongestActiveEmotion();
    }


    public IEnumerator EmotionalUpdateLoop(float delay)
    {
        if (unperceivedEvents.Count > 0)
        {
            rpc.Perceive(unperceivedEvents);
            unperceivedEvents.Clear();
        }
        whatICanDo = rpc.Decide().ToList<ActionLibrary.IAction>();
        rpc.Update();
        yield return new WaitForSeconds(delay);
        EmotionalUpdateLoop(delay);
    }

    //void OnApplicationQuit()
    //{
    //    this.rpc.SaveToFile(Application.streamingAssetsPath + "/Runtimed/" + name + ".rpc");
    //}

   

}
