using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public interface IInteractionModule
{
    void Init(GameObject uiPrefab, GameObject uiContainer);
    void Speak(string utterance);
    void Move();
}


public class AutisticInteractionModule : IInteractionModule
{
    public void Init(GameObject uiPrefab, GameObject uiContainer) { }
    public void Speak(string utterance) { }
    public void Move() { }
}

public class LegendsInteractionModule: IInteractionModule
{
    protected GameObject speechBalloonUI;
    private List<string> currSpeeches;
    private float speechBalloonDelayPerWordInSeconds;

    MonoBehaviourFunctionalities monoBehaviourFunctionalities;

    public void Init(GameObject uiPrefab, GameObject uiContainer) {

        this.speechBalloonUI = Object.Instantiate(uiPrefab, uiContainer.transform);
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

    public void Init(GameObject uiPrefab, GameObject uiContainer)
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