using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceManager
{
    private IDiceLogic diceLogic;
    private GameObject diceUIPrefab;
    private GameObject rollDiceOverlay;
    private GameObject rollDiceOverlayContainer;
    private Animator rollDiceOverlayAnimator;

    private MonoBehaviourFunctionalities monobehaviorFunctionalities;

    private int[] currDiceResults;

    public DiceManager(GameObject rollDiceOverlay, GameObject diceUIPrefab, IDiceLogic diceLogic)
    {
        this.diceUIPrefab = diceUIPrefab;
        this.diceLogic = diceLogic;

        this.monobehaviorFunctionalities = new MonoBehaviourFunctionalities();

        this.rollDiceOverlay = rollDiceOverlay;
        this.rollDiceOverlayContainer = rollDiceOverlay.transform.Find("diceUIs").gameObject;
        this.rollDiceOverlayAnimator = rollDiceOverlay.GetComponent<Animator>();
        this.rollDiceOverlay.SetActive(false);
    }

    public IEnumerator RollTheDice(string rollOverlayTitle, int numRolls)
    {
        //var currInvestment = currPlayer.GetCurrRoundInvestment();

        //int newInvestmentValue = 0;
        //int numTokensForTarget = currInvestment[target];
        //if (numTokensForTarget == 0) //there are no dices to roll so return imediately
        //{
        //    yield break;
        //}

        //rollDiceOverlay.transform.Find("title/Text").GetComponent<Text>().text = currPlayer.GetName() + " rolling " + numTokensForTarget + " dice for " + target.ToString() + " ...";
        rollDiceOverlay.transform.Find("title/Text").GetComponent<Text>().text = rollOverlayTitle;
        List<int> currDiceResults = new List<int>(); //save each rolled dice number to display in the UI

        for (int i = 0; i < numRolls; i++)
        {
            int randomResult = diceLogic.RollTheDice(6, numRolls);
            currDiceResults.Add(randomResult);
            //newInvestmentValue += randomIncrease;
            monobehaviorFunctionalities.StartCoroutine(PlayDiceUI(randomResult, i, numRolls, 2.0f, diceUIPrefab, "Animations/RollDiceOverlay/dice6/sprites_3/endingAlternatives/"));
        }

        //update game metrics after dice rolls
        //if (target == GameProperties.InvestmentTarget.ECONOMIC)
        //{
        //    yield return StartCoroutine(currPlayer.SetMoney(currPlayer.GetMoney() + ((float)newInvestmentValue / 100.0f)));
        //}
        //else
        //{
        //    yield return StartCoroutine(envDynamicSlider.UpdateSliderValue(environmentSliderSceneElement.value + (float)newInvestmentValue / 100.0f));
        //}

        //GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), currPlayer.GetId().ToString(), currPlayer.GetName().ToString(), "ROLLED_INVESTMENT_TARGET_DICES", "-", numTokensForTarget.ToString());


        yield return currDiceResults.Count == numRolls; //wait until all dice are rolled
        currDiceResults.Clear();
    }


    //public IEnumerator YieldedRollDices(int numRolls, string rollOverlayTitle)
    //{
    //}

    private IEnumerator PlayDiceUI(int currThrowResult, int sequenceNumber, int numDicesInThrow, float displayDelay, GameObject diceImagePrefab, string diceNumberSpritesPath)
    //the sequence number aims to void dice overlaps as it represents the order for which this dice is going to be rolled. We do not want to roll a dice two times for the same place
    {

        if (sequenceNumber < 1)
        {
            this.rollDiceOverlay.SetActive(true);
        }


        GameObject diceUIClone = new GameObject();
        Sprite currDiceNumberSprite = Resources.Load<Sprite>(diceNumberSpritesPath + currThrowResult);
        if (currDiceNumberSprite == null)
        {
            Debug.Log("cannot find sprite for dice number " + currThrowResult);
            yield return null;
        }
        else
        {
            diceUIClone = Object.Instantiate(diceImagePrefab, rollDiceOverlayContainer.transform);

            //play dice UI
            Image diceImage = diceUIClone.GetComponentInChildren<Image>();
            Animator diceAnimator = diceImage.GetComponentInChildren<Animator>();

            float translationFactorX = Screen.width * 0.04f;
            float translationFactorY = Screen.width * 0.02f;
            diceUIClone.transform.Translate(new Vector3(Random.Range(-translationFactorX, translationFactorY), Random.Range(-translationFactorX, translationFactorY), 0));

            float diceRotation = sequenceNumber * (360.0f / numDicesInThrow);

            diceUIClone.transform.Rotate(new Vector3(0, 0, 1), diceRotation);
            diceImage.overrideSprite = null;
            diceAnimator.Rebind();
            diceAnimator.Play(0);
            diceAnimator.speed = Random.Range(0.8f, 1.0f);

            while (!diceAnimator.GetCurrentAnimatorStateInfo(0).IsName("endState"))
            {
                yield return null;
            }
            diceImage.overrideSprite = currDiceNumberSprite;
        }


        while (!rollDiceOverlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
        {
            yield return null;
        }

        rollDiceOverlayAnimator.speed = 0;
        yield return new WaitForSeconds(displayDelay);

        rollDiceOverlayAnimator.speed = 1;
        while (!rollDiceOverlayAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle2"))
        {
            yield return null;
        }
        
        Object.Destroy(diceUIClone);
        if (sequenceNumber > numDicesInThrow)
        {
            this.rollDiceOverlay.SetActive(false);
        }

    }


}
