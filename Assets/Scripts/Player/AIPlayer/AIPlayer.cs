using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AIPlayer : Player
{
    protected EmotionalModule emotionalModule;


    public AIPlayer(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name): 
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    {
        GameObject erp = new GameObject("EmotionalRoboticPlayer");
        emotionalModule = erp.AddComponent<EmotionalModule>();
        emotionalModule.Speaks = true;
        emotionalModule.ReceiveInvoker(this); //only pass the invoker after it is initialized

        this.playerSelfDisablerUI.SetActive(true);
    }

    //simulate button clicks
    private void SimulateMouseDown(Button button)
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerDownHandler);
    }
    private void SimulateMouseUp(Button button)
    {
        var pointer = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerClickHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(button.gameObject, pointer, ExecuteEvents.pointerExitHandler);
    }
    protected IEnumerator SimulateMouseClick(Button button, float pressingTime)
    {
        SimulateMouseDown(button);
        yield return new WaitForSeconds(pressingTime);
        SimulateMouseUp(button);
    }

    public virtual IEnumerator AutoBudgetAlocation() { yield return null; }
    public virtual IEnumerator AutoHistoryDisplay() { yield return null; }
    public virtual IEnumerator AutoBudgetExecution() { yield return null; }
    public virtual IEnumerator AutoInvestmentExecution() { yield return null; }

    public override void BudgetAllocationPhaseRequest()
    {
        base.BudgetAllocationPhaseRequest();
        playerMonoBehaviourFunctionalities.StartCoroutine(AutoBudgetAlocation());
    }

    public override void HistoryDisplayPhaseRequest()
    {
        base.HistoryDisplayPhaseRequest();
        playerMonoBehaviourFunctionalities.StartCoroutine(AutoHistoryDisplay());
    }

    public override void BudgetExecutionPhaseRequest()
    {
        base.BudgetExecutionPhaseRequest();
        playerMonoBehaviourFunctionalities.StartCoroutine(AutoBudgetExecution());
    }

    public override void InvestmentSimulationRequest()
    {
        base.InvestmentSimulationRequest();
        playerMonoBehaviourFunctionalities.StartCoroutine(AutoInvestmentExecution());
    }

    public IEnumerator ClickEconomyInvestmentButton()
    {
        yield return new WaitForSeconds(1.0f);
        yield return SimulateMouseClick(this.spendTokenInEconomicGrowthButtonUI, 0.5f);
    }

    public IEnumerator ClickEconomyInvestmentButton(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            yield return ClickEconomyInvestmentButton();
        }
    }

    public IEnumerator InvestInEconomy(int quantity)
    {
        // TODO:
        // validate if there is enough budget for the quantity
        // output a debug message if there isn't
        int clicks = quantity - currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC];
        yield return ClickEconomyInvestmentButton(clicks);
    }

    public IEnumerator InvestAllInEconomy()
    {
        int quantity = roundBudget;
        yield return InvestInEconomy(quantity);
    }

    public IEnumerator ClickEnvironmentInvestmentButton()
    {
        yield return new WaitForSeconds(1.0f);
        yield return SimulateMouseClick(this.spendTokenInEnvironmentButtonUI, 0.5f);
    }

    public IEnumerator ClickEnvironmentInvestmentButton(int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            yield return ClickEnvironmentInvestmentButton();
        }
    }

    public IEnumerator InvestInEnvironment(int quantity)
    {
        // TODO:
        // validate if there is enough budget for the quantity
        // output a debug message if there isn't
        int clicks = quantity - currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT];
        yield return ClickEnvironmentInvestmentButton(clicks);
    }

    public IEnumerator InvestAllInEvironment()
    {
        int quantity = roundBudget;
        yield return InvestInEnvironment(quantity);
    }

    public IEnumerator EndBudgetAllocationPhase()
    {
        yield return new WaitForSeconds(3.0f);
        yield return SimulateMouseClick(this.playerActionButtonUI, 0.5f);
    }

    public IEnumerator ApplyInvestments()
    {
        yield return new WaitForSeconds(3.0f);
        yield return SimulateMouseClick(this.executeBudgetButton, 0.5f);
    }

}




public class AIPlayerCooperative : AIPlayer
{
    public AIPlayerCooperative(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation() {
        yield return InvestAllInEvironment();
        yield return EndBudgetAllocationPhase();
    }
    public override IEnumerator AutoHistoryDisplay() {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
    }
    public override IEnumerator AutoBudgetExecution()
    {
        yield return ApplyInvestments();
    }
    public override IEnumerator AutoInvestmentExecution() {
        yield return null;
    }

}

public class AIPlayerDefector : AIPlayer
{
    public AIPlayerDefector(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation()
    {
        yield return InvestAllInEconomy();
        yield return EndBudgetAllocationPhase();
    }
    public override IEnumerator AutoHistoryDisplay()
    {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
    }
    public override IEnumerator AutoBudgetExecution()
    {
        yield return ApplyInvestments();
    }
    public override IEnumerator AutoInvestmentExecution()
    {
        yield return null;
    }

}