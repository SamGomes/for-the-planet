using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public abstract class InteractionModule
{
    protected bool locksInteraction;
    public virtual void Init(GameObject uiPrefab, GameObject uiContainer, bool locksInteraction) {
        this.locksInteraction = locksInteraction;
    }
    public abstract void Speak(string utterance);
    public abstract void Move();


    protected void InterruptGame()
    {
        if (locksInteraction && GameGlobals.gameManager != null) 
        {
            GameGlobals.gameManager.InterruptGame();
        }
    }
    protected void ContinueGame()
    {
        if (locksInteraction && GameGlobals.gameManager != null)
        {
            GameGlobals.gameManager.ContinueGame();
        }
    }
}


public class AutisticInteractionModule : InteractionModule
{
    public override void Init(GameObject uiPrefab, GameObject uiContainer, bool locksInteraction) {
        base.Init(uiPrefab, uiContainer, locksInteraction);
    }
    public override void Speak(string utterance) {

    }
    public override void Move() {
    }

}

public class SpeechBaloonInteractionModule: InteractionModule
{
    protected GameObject speechBalloonUI;
    private List<string> currSpeeches;
    private float speechBalloonDelayPerWordInSeconds;

    MonoBehaviourFunctionalities monoBehaviourFunctionalities;

    private bool isSpeaking;

    public override void Init(GameObject uiPrefab, GameObject uiContainer, bool locksInteraction) {
        base.Init(uiPrefab, uiContainer, locksInteraction);

        this.speechBalloonUI = Object.Instantiate(uiPrefab, uiContainer.transform);
        speechBalloonUI.SetActive(false);

        this.monoBehaviourFunctionalities = GameGlobals.monoBehaviourFunctionalities;

        this.speechBalloonDelayPerWordInSeconds = 0.5f;
        currSpeeches = new List<string>();

        float updateDelay = 0.2f;
        monoBehaviourFunctionalities.StartCoroutine(FixedTimeUpdate(updateDelay));

        this.isSpeaking = false;
    }
   

    public override void Speak(string utterance) {
        currSpeeches.Add(utterance);
    }
    public override void Move() { }

    
    public bool IsSpeaking()
    {
        return this.isSpeaking;
    }

    private IEnumerator DisplaySpeechBalloonForAWhile(string message, float delay)
    {
        this.isSpeaking = true;

        this.speechBalloonUI.GetComponentInChildren<Text>().text = message;
        speechBalloonUI.SetActive(true);
        InterruptGame();
        yield return new WaitForSeconds(delay);
        if (speechBalloonUI.GetComponentInChildren<Text>().text == message) //to compensate if the balloon is already spawned
        {
            ContinueGame();
            speechBalloonUI.SetActive(false);

            this.isSpeaking = false;
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
        monoBehaviourFunctionalities.StartCoroutine(FixedTimeUpdate(updateDelay));
    }

}

public class RobotInteractionModule : InteractionModule
{
    private ThalamusConnector thalamusConnector = null;

    public override void Init(GameObject uiPrefab, GameObject uiContainer, bool locksInteraction)
    {
        base.Init(uiPrefab, uiContainer, locksInteraction);
        thalamusConnector = new ThalamusConnector(7000);
    }
    

    private void GazeAt(string target)
    {
        thalamusConnector.GazeAt(target);
    }

    public override void Speak(string utterance)
    {
        InterruptGame();
        thalamusConnector.PerformUtterance(utterance, new string[] { }, new string[] { });
        ContinueGame();
    }
    public override void Move() { }
}