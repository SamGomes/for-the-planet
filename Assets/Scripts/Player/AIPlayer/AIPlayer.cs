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
}




public class AIPlayerCooperative : AIPlayer
{
    public AIPlayerCooperative(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation() {
        yield return new WaitForSeconds(3.0f);
        yield return SimulateMouseClick(this.spendTokenInEconomicGrowthButtonUI, 0.5f);
        yield return new WaitForSeconds(3.0f);
        yield return SimulateMouseClick(this.playerActionButtonUI, 0.5f);
    }
    public override IEnumerator AutoHistoryDisplay() {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
    }
    public override IEnumerator AutoBudgetExecution()
    {
        yield return new WaitForSeconds(3.0f);
        yield return SimulateMouseClick(this.executeBudgetButton, 0.5f);
    }
    public override IEnumerator AutoInvestmentExecution() {
        yield return null;
    }
}