using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoboticPlayer : AIPlayer
{

    private ThalamusConnector thalamusConnector = null;

    public RoboticPlayer(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(playerUIPrefab, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    {
        this.playerSelfDisablerUI.SetActive(true);
        this.InitThalamusConnectorOnPort(7000, name);
    }

    public void InitThalamusConnectorOnPort(int port, string name)
    {
        thalamusConnector = new ThalamusConnector(port);
        this.name = name;
    }

    public void PerformUtterance(string text, string[] tags, string[] values)
    {
        thalamusConnector.PerformUtterance(text, tags, values);
    }

    public void FlushRobotUtterance(string text)
    {
       PerformUtterance(text, new string[] { }, new string[] { });
    }

    public void GazeAt(string target)
    {
        thalamusConnector.GazeAt(target);
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

        FlushRobotUtterance("OlÁ!!!!");

        yield return EndBudgetAllocationPhase();
    }

}
