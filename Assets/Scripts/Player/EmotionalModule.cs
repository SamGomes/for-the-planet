using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AssetManagerPackage;
using IntegratedAuthoringTool;
using RolePlayCharacter;
using WellFormedNames;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;


public class EmotionalModule
{
    private float speechBalloonDelayPerWordInSeconds;

    private bool isStopped;

    public int DicesValue { get; internal set; }
    public int NumDices { get; internal set; }

    List<RolePlayCharacterAsset> rpcs;

    public bool Speaks { get; internal set; }
    private Player invoker; //when no speech the object is passed so that text is displayed
    private GameObject speechBalloon;

    private readonly MonoBehaviourFunctionalities monoBehaviourFunctionalities;

    private List<string> currSpeeches;

    public EmotionalModule(Player invoker, MonoBehaviourFunctionalities monoBehaviourFunctionalities)
    {

        this.monoBehaviourFunctionalities = monoBehaviourFunctionalities;
        isStopped = false;

        rpcs = new List<RolePlayCharacterAsset>();

        foreach(IntegratedAuthoringTool.DTOs.CharacterSourceDTO rpcPath in GameGlobals.FAtiMAIat.GetAllCharacterSources())
        {
            Debug.Log(rpcPath.RelativePath);
            RolePlayCharacterAsset rpc;
            rpc = RolePlayCharacterAsset.LoadFromFile(rpcPath.Source);
            rpc.LoadAssociatedAssets();
            GameGlobals.FAtiMAIat.BindToRegistry(rpc.DynamicPropertiesRegistry);
            rpcs.Add(rpc);
        }


        //start update thread
        monoBehaviourFunctionalities.StartCoroutine(Update());

        speechBalloon = invoker.GetSpeechBaloonUI();
        speechBalloonDelayPerWordInSeconds = 0.5f;
        currSpeeches = new List<string>();
        float speechCheckDelayInSeconds = 0.1f;
        monoBehaviourFunctionalities.StartCoroutine(ConsumeSpeeches(speechCheckDelayInSeconds));
    }
    
    public void Perceive(Name[] events)
    {
        foreach(RolePlayCharacterAsset rpc in rpcs)
        {
            rpc.Perceive(events);
        }
    }

    public IEnumerator DisplaySpeechBalloonForAWhile(string message, float delay)
    {
        speechBalloon.GetComponentInChildren<Text>().text = message;
        speechBalloon.SetActive(true);
        yield return new WaitForSeconds(delay);
        if (speechBalloon.GetComponentInChildren<Text>().text == message) //to compensate if the balloon is already spawned
        {
            speechBalloon.SetActive(false);
        }
    }

    public string StripSpeechSentence(string rawMessage)
    {
        var strippedDialog = rawMessage;
        strippedDialog = strippedDialog.Replace("|dicesValue|", DicesValue.ToString());
        strippedDialog = strippedDialog.Replace("|numDices|", NumDices.ToString());
        strippedDialog = Regex.Replace(strippedDialog, @"<.*?>\s+|\s+<.*?>|\s+<.*?>\s+", "");
        return strippedDialog;
    }

    public void Decide()
    {
        //not sure of this...
        List<ActionLibrary.IAction> possibleActions = new List<ActionLibrary.IAction>();
        foreach (RolePlayCharacterAsset rpc in rpcs)
        {
            possibleActions.AddRange(rpc.Decide());
        }
        ActionLibrary.IAction chosenAction = possibleActions.FirstOrDefault();

        if (chosenAction == null)
        {
            Console.WriteLine("No action");
            //saveToFile();
            return;
        }
        else
        {
            //saveToFile();
            switch (chosenAction.Key.ToString())
            {
                case "Speak":

                    Name currentState = chosenAction.Parameters[0];
                    Name nextState = chosenAction.Parameters[1];
                    Name meaning = chosenAction.Parameters[2];
                    Name style = chosenAction.Parameters[3];

                    var possibleDialogs = GameGlobals.FAtiMAIat.GetDialogueActions(currentState, nextState, meaning, style);
                    int randomUttIndex = UnityEngine.Random.Range(0, possibleDialogs.Count());
                    var dialog = possibleDialogs[randomUttIndex].Utterance;

                    currSpeeches.Add(invoker.GetName() + ": \"" + StripSpeechSentence(dialog) + "\"");
                    break;
                default:
                    break;
            }
        }
    }

    private IEnumerator Update()
    {
        foreach (RolePlayCharacterAsset rpc in rpcs)
        {
            rpc.Update();
        }
        yield return null;
    }
    
    public IEnumerator ConsumeSpeeches(float checkSpeechDelay)
    {
        if (currSpeeches.Count > 0 && !speechBalloon.activeSelf)
        {
            string currSpeech = currSpeeches[0];
            Regex regex = new Regex("\\w+");
            int countedWords = regex.Matches(currSpeech).Count;

            float displayingDelay = countedWords * this.speechBalloonDelayPerWordInSeconds;
            monoBehaviourFunctionalities.StartCoroutine(DisplaySpeechBalloonForAWhile(currSpeech, displayingDelay));
            currSpeeches.Remove(currSpeech);
        }
        yield return new WaitForSeconds(checkSpeechDelay);
        monoBehaviourFunctionalities.StartCoroutine(ConsumeSpeeches(checkSpeechDelay));
    }
}

