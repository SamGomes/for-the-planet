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

    private RolePlayCharacterAsset rpc;
    private bool isStopped;

    public int DicesValue { get; internal set; }
    public int NumDices { get; internal set; }


    public bool Speaks { get; internal set; }
    private Player invoker; //when no speech the object is passed so that text is displayed
    private GameObject speechBalloon;

    private readonly MonoBehaviourFunctionalities monoBehaviourFunctionalities;

    private List<string> currSpeeches;

    public EmotionalModule(MonoBehaviourFunctionalities monoBehaviourFunctionalities)
    {

        this.monoBehaviourFunctionalities = monoBehaviourFunctionalities;
        isStopped = false;
        
        foreach(IntegratedAuthoringTool.DTOs.CharacterSourceDTO rpcPath in GameGlobals.FAtiMAIat.GetAllCharacterSources())
        {
            rpc = RolePlayCharacterAsset.LoadFromFile(rpcPath.RelativePath);
            rpc.LoadAssociatedAssets();
            GameGlobals.FAtiMAIat.BindToRegistry(rpc.DynamicPropertiesRegistry);
        }


        //start update thread
        monoBehaviourFunctionalities.StartCoroutine(UpdateCoroutine());

        speechBalloonDelayPerWordInSeconds = 0.5f;
        currSpeeches = new List<string>();

        float speechCheckDelayInSeconds = 0.1f;
        monoBehaviourFunctionalities.StartCoroutine(ConsumeSpeeches(speechCheckDelayInSeconds));
    }

    public void ReceiveInvoker(Player invoker)
    {
        this.invoker = invoker;
        speechBalloon = invoker.GetSpeechBaloonUI();
    }

    public void Perceive(Name[] events)
    {
        rpc.Perceive(events);
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
        IEnumerable<ActionLibrary.IAction> possibleActions = rpc.Decide();
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

    private IEnumerator UpdateCoroutine()
    {
        rpc.Update();
        yield return new WaitForSeconds(0.1f);
    }

    void OnDestroy()
    {
        if (!isStopped)
        {
            monoBehaviourFunctionalities.StopCoroutine(UpdateCoroutine());
            isStopped = true;
        }
    }

    void OnApplicationQuit()
    {
        if (!isStopped)
        {
            monoBehaviourFunctionalities.StopCoroutine(UpdateCoroutine());
            isStopped = true;
        }
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

