using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public interface IInteractionModule
{
    void Init(Player owner);
    void Speak(string utterance);
    void Move();
}


public class AutisticInteractionModule : IInteractionModule
{
    public void Init(Player owner) { }
    public void Speak(string utterance) { }
    public void Move() { }
}

public class LegendsInteractionModule: IInteractionModule
{
    protected GameObject speechBalloonUI;
    private List<string> currSpeeches;
    private float speechBalloonDelayPerWordInSeconds;

    MonoBehaviourFunctionalities monoBehaviourFunctionalities;

    public void Init(Player owner) {

        this.speechBalloonUI = (owner.GetId() % 2 == 0) ? Object.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerUI/speechBalloonLeft"), owner.GetPlayerUI().transform) : Object.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerUI/speechBalloonRight"), owner.GetPlayerUI().transform);
        speechBalloonUI.SetActive(false);

        this.monoBehaviourFunctionalities = GameGlobals.monoBehaviourFunctionalities;


        this.speechBalloonDelayPerWordInSeconds = 0.5f;
        currSpeeches = new List<string>();

        float updateDelay = 0.2f;
        monoBehaviourFunctionalities.StartCoroutine(FixedTimeUpdate(updateDelay));
    }
   

    public void Speak(string utterance) {
        currSpeeches.Add(utterance);
    }
    public void Move() { }

    
    private IEnumerator DisplaySpeechBalloonForAWhile(string message, float delay)
    {
        this.speechBalloonUI.GetComponentInChildren<Text>().text = message;
        speechBalloonUI.SetActive(true);
        yield return new WaitForSeconds(delay);
        if (speechBalloonUI.GetComponentInChildren<Text>().text == message) //to compensate if the balloon is already spawned
        {
            speechBalloonUI.SetActive(false);
        }
    }


    private void ConsumeSpeechesOnUpdate()
    {
        if (currSpeeches.Count > 0 && !speechBalloonUI.activeSelf)
        {
            string currSpeech = currSpeeches[0];
            Regex regex = new Regex("\\w+");
            int countedWords = regex.Matches(currSpeech).Count;

            float displayingDelay = countedWords * this.speechBalloonDelayPerWordInSeconds;
            monoBehaviourFunctionalities.StartCoroutine(DisplaySpeechBalloonForAWhile(currSpeech, displayingDelay));
            currSpeeches.Remove(currSpeech);
        }
    }



    private IEnumerator FixedTimeUpdate(float updateDelay)
    {
        ConsumeSpeechesOnUpdate();
        yield return new WaitForSeconds(updateDelay);
        monoBehaviourFunctionalities.StartCoroutine(FixedTimeUpdate(updateDelay)); ;
    }

}

public class RobotInteractionModule : IInteractionModule
{
    private ThalamusConnector thalamusConnector = null;

    public void Init(Player player)
    {
        thalamusConnector = new ThalamusConnector(7000);
    }
    

    private void GazeAt(string target)
    {
        thalamusConnector.GazeAt(target);
    }

    public void Speak(string utterance)
    {
        thalamusConnector.PerformUtterance(utterance, new string[] { }, new string[] { });
    }
    public void Move() { }
}