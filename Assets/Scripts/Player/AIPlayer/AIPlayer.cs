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
        emotionalModule = new EmotionalModule(this, playerMonoBehaviourFunctionalities);
        emotionalModule.Speaks = true;

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




public class AIPlayerCooperator : AIPlayer
{
    public AIPlayerCooperator(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation() {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Start of Budget Allocation
        yield return InvestAllInEvironment();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before ending Budget Allocation
        yield return EndBudgetAllocationPhase();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - End of Budget Allocation
    }
    public override IEnumerator AutoHistoryDisplay() {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
        // @jbgrocha: Fatima Speech Act (emotional engine call) - History Display
    }
    public override IEnumerator AutoBudgetExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        yield return ApplyInvestments();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }
    public override IEnumerator AutoInvestmentExecution() {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Decay
        yield return null;
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Decay
    }

}

public class AIPlayerDefector : AIPlayer
{
    public AIPlayerDefector(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Start of Budget Allocation
        yield return InvestAllInEconomy();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before ending Budget Allocation
        yield return EndBudgetAllocationPhase();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - End of Budget Allocation
    }
    public override IEnumerator AutoHistoryDisplay()
    {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
        // @jbgrocha: Fatima Speech Act (emotional engine call) - History Display
    }
    public override IEnumerator AutoBudgetExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        yield return ApplyInvestments();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }
    public override IEnumerator AutoInvestmentExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Decay
        yield return null;
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Decay
    }

}

public class AIPlayerBalancedCooperator: AIPlayer
{
    public AIPlayerBalancedCooperator(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation()
    {

        int environmentInvestment = roundBudget / 2 + roundBudget % 2;
        int economyInvestment = roundBudget / 2;

        // @jbgrocha: Fatima Speech Act (emotional engine call) - Start of Budget Allocation
        yield return InvestInEconomy(economyInvestment);

        yield return InvestInEnvironment(environmentInvestment);

        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before ending Budget Allocation
        yield return EndBudgetAllocationPhase();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - End of Budget Allocation
    }
    public override IEnumerator AutoHistoryDisplay()
    {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
        // @jbgrocha: Fatima Speech Act (emotional engine call) - History Display
    }
    public override IEnumerator AutoBudgetExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        yield return ApplyInvestments();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }
    public override IEnumerator AutoInvestmentExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Decay
        yield return null;
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Decay
    }

}

public class AIPlayerBalancedDefector : AIPlayer
{
    public AIPlayerBalancedDefector(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAlocation()
    {

        int environmentInvestment = roundBudget / 2;
        int economyInvestment = roundBudget / 2 + roundBudget % 2;

        // @jbgrocha: Fatima Speech Act (emotional engine call) - Start of Budget Allocation
        yield return InvestInEconomy(economyInvestment);

        yield return InvestInEnvironment(environmentInvestment);

        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before ending Budget Allocation
        yield return EndBudgetAllocationPhase();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - End of Budget Allocation
    }
    public override IEnumerator AutoHistoryDisplay()
    {
        yield return new WaitForSeconds(3.0f);
        yield return emotionalModule.DisplaySpeechBalloonForAWhile("Mock Warning! This is not a fatima call!", 2.0f);
        // @jbgrocha: Fatima Speech Act (emotional engine call) - History Display
    }
    public override IEnumerator AutoBudgetExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Budget Dice Rolls
        yield return ApplyInvestments();
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Budget Dice Rolls
    }
    public override IEnumerator AutoInvestmentExecution()
    {
        // @jbgrocha: Fatima Speech Act (emotional engine call) - Before Decay
        yield return null;
        // @jbgrocha: Fatima Speech Act (emotional engine call) - After Decay
    }

}