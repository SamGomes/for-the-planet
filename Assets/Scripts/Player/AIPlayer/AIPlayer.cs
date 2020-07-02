using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AIPlayer : Player
{

    protected InteractionModule interactionModule;

    public AIPlayer(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name): 
        base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        this.interactionModule = interactionModule;
        if (!GameGlobals.isSimulation)
        {
            this.playerSelfDisablerUI.SetActive(true);
        }
        GameObject speechBaloonPrefab = (this.GetId() % 2 == 0) ? Resources.Load<GameObject>("Prefabs/PlayerUI/speechBalloonLeft") : Resources.Load<GameObject>("Prefabs/PlayerUI/speechBalloonRight");
        interactionModule.Init(speechBaloonPrefab, playerUI, false);
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


    public IEnumerator ClickEconomyInvestmentButton()
    {
        if (!GameGlobals.isSimulation)
        {
            yield return new WaitForSeconds(1.0f);
            yield return SimulateMouseClick(this.spendTokenInEconomicGrowthButtonUI, 0.5f);
        }
        else
        {
            SpendTokenInEconomicGrowth();
        }

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
        int quantity = GameGlobals.roundBudget;
        yield return InvestInEconomy(quantity);
    }

    public IEnumerator ClickEnvironmentInvestmentButton()
    {
        if (!GameGlobals.isSimulation)
        {
            yield return new WaitForSeconds(0.1f);
            yield return SimulateMouseClick(this.spendTokenInEnvironmentButtonUI, 0.5f);
        }
        else
        {
            SpendTokenInEnvironment();
        }
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
        int quantity = GameGlobals.roundBudget;
        yield return InvestInEnvironment(quantity);
    }

    public IEnumerator EndBudgetAllocationPhase()
    {
        if (!GameGlobals.isSimulation)
        {
            yield return new WaitForSeconds(1.0f);
            yield return SimulateMouseClick(this.playerActionButtonUI, 0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.0f);
            SendBudgetAllocationPhaseResponse();

        }

    }

    public IEnumerator ApplyInvestments()
    {
        if (!GameGlobals.isSimulation)
        {
            yield return new WaitForSeconds(3.0f);
            yield return SimulateMouseClick(this.executeBudgetButton, 0.5f);
        }
        else
        {
            yield return new WaitForSeconds(0.0f);
            SendBudgetExecutionPhaseResponse();
        }

    }
    
    
    public virtual IEnumerator AutoBudgetAllocation() { yield return null; }
    public virtual IEnumerator AutoHistoryDisplay() { yield return null; }
    public virtual IEnumerator AutoBudgetExecution() { yield return ApplyInvestments(); }
    public virtual IEnumerator AutoInvestmentExecution() { yield return null; }

    public override void BudgetAllocationPhaseRequest()
    {
        base.BudgetAllocationPhaseRequest();
        playerMonoBehaviourFunctionalities.StartCoroutine(AutoBudgetAllocation());
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




public class AIPlayerCooperator : AIPlayer
{
    public AIPlayerCooperator(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAllocation()
    {
        yield return InvestAllInEvironment();
        yield return EndBudgetAllocationPhase();
    }

}

public class AIPlayerDefector : AIPlayer
{
    public AIPlayerDefector(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAllocation()
    {
        yield return InvestAllInEconomy();
        yield return EndBudgetAllocationPhase();
    }
}

public class AIPlayerBalancedCooperator : AIPlayer
{
    public AIPlayerBalancedCooperator(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAllocation()
    {
        float meanInv = GameGlobals.roundBudget / 2.0f;
        int environmentInvestment = Mathf.CeilToInt(meanInv);
        int economyInvestment = Mathf.FloorToInt(meanInv);

        yield return InvestInEconomy(economyInvestment);
        yield return InvestInEnvironment(environmentInvestment);

        yield return EndBudgetAllocationPhase();
    }

}

public class AIPlayerBalancedDefector : AIPlayer
{
    public AIPlayerBalancedDefector(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAllocation()
    {
        float meanInv = GameGlobals.roundBudget / 2.0f;
        int environmentInvestment = Mathf.FloorToInt(meanInv);
        int economyInvestment = Mathf.CeilToInt(meanInv);
        
        yield return InvestInEconomy(economyInvestment);
        yield return InvestInEnvironment(environmentInvestment);
        yield return EndBudgetAllocationPhase();
    }
}

public class AIPlayerRandom : AIPlayer
{
    public AIPlayerRandom(string type, InteractionModule interactionModule, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(type, interactionModule, playerCanvas, warningScreenRef, UIAvatar, id, name)
    { }

    public override IEnumerator AutoBudgetAllocation()
    {
        int economyInvestment = UnityEngine.Random.Range(0, GameGlobals.roundBudget + 1);
        int environmentInvestment = GameGlobals.roundBudget - economyInvestment;

        yield return InvestInEconomy(economyInvestment);
        yield return InvestInEnvironment(environmentInvestment);

        yield return EndBudgetAllocationPhase();
    }

}