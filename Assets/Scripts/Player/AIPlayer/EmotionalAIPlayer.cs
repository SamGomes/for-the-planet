using RolePlayCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

class EmotionalAIPlayer: AIPlayer
{
    //Emotional stuff
    private RolePlayCharacterAsset rpc;
    private List<string> currSpeeches;
    private float speechBalloonDelayPerWordInSeconds;
    List<WellFormedNames.Name> unperceivedEvents;
    List<ActionLibrary.IAction> whatICanDo;

    public EmotionalAIPlayer(GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    {
        unperceivedEvents = new List<WellFormedNames.Name>();
        whatICanDo = new List<ActionLibrary.IAction>();
        InitRPC();

        playerMonoBehaviourFunctionalities.StartCoroutine(EmotionalUpdateLoop(0.2f));
    }

    public void InitRPC()
    {
        //rpc.Update();
        currSpeeches = new List<string>();

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
    public EmotionalAppraisal.IActiveEmotion GetMyStrongestEmotion()
    {
        return this.rpc.GetStrongestActiveEmotion();
    }


    public IEnumerator EmotionalUpdateLoop(float delay)
    {
        ConsumeSpeechesOnUpdate();
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

    //public string StripSpeechSentence(string rawMessage)
    //{
    //    var strippedDialog = rawMessage;
    //    strippedDialog = strippedDialog.Replace("|dicesValue|", DicesValue.ToString());
    //    strippedDialog = strippedDialog.Replace("|numDices|", NumDices.ToString());
    //    strippedDialog = Regex.Replace(strippedDialog, @"<.*?>\s+|\s+<.*?>|\s+<.*?>\s+", "");
    //    return strippedDialog;
    //}
    public void ConsumeSpeechesOnUpdate()
    {
        if (currSpeeches.Count > 0 && !speechBalloonUI.activeSelf)
        {
            string currSpeech = currSpeeches[0];
            Regex regex = new Regex("\\w+");
            int countedWords = regex.Matches(currSpeech).Count;

            float displayingDelay = countedWords * this.speechBalloonDelayPerWordInSeconds;
            playerMonoBehaviourFunctionalities.StartCoroutine(DisplaySpeechBalloonForAWhile(currSpeech, displayingDelay));
            currSpeeches.Remove(currSpeech);
        }
    }


}
