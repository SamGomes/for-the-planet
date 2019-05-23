using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmotionalRoboticPlayer : RoboticPlayer
{
    public EmotionalRoboticPlayer(GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    {
        this.playerSelfDisablerUI.SetActive(true);
    }

    // Investment
    public override IEnumerator AutoBudgetAlocation() {
        // Execute Emotional Engine Decision for Investment

        // Get Environment Investment from emotional decision
        int environmentInvestment = roundBudget / 2;

        // Get Economy Investment from emotional decision
        int economyInvestment = roundBudget / 2 + roundBudget % 2;

        // @jbgrocha: Fatima Speech Act (emotional engine call) - Start of Budget Allocation
        yield return InvestInEconomy(economyInvestment);

        yield return InvestInEnvironment(environmentInvestment);

        yield return EndBudgetAllocationPhase();
    }

}
