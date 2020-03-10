﻿using System;
using UnityEngine;

public class TabletPlayer : Player
{
    private TabletThalamusConnector thalamusConnector;


    public TabletPlayer(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
        : base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        thalamusConnector = new TabletThalamusConnector(int.Parse(GameGlobals.thalamusClientPort));
    }

    public void ConnectToGameMaster()
    {
        thalamusConnector.ConnectToGM(GameGlobals.tabletID, GameGlobals.participantName);
    }

    public void Dispose()
    {
        thalamusConnector.Dispose();
    }

    public override int SendBudgetAllocationPhaseResponse()
    {
        thalamusConnector.SendBudgetAllocation(GameGlobals.tabletID, currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT]);
        budgetAllocationScreenUI.SetActive(false);
        displayHistoryScreenUI.SetActive(false);
        budgetExecutionScreenUI.SetActive(false);
        investmentSimulationScreenUI.SetActive(false);
        playerActionButtonUI.gameObject.SetActive(false);
        playerMarkerUI.SetActive(false);
        playerDisablerUI.SetActive(false);
        base.SendBudgetAllocationPhaseResponse();
        return 0;
    }

}

public class RemotePlayer : Player
{
    public RemotePlayer(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
        : base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {

    }

    internal void ReceiveRemoteBudgetAllocation(int envAllocation)
    {
        unallocatedBudget = GameGlobals.roundBudget;

        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = envAllocation;
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = (GameGlobals.roundBudget - envAllocation);
        investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] += envAllocation;
        investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] += (GameGlobals.roundBudget - envAllocation);
        gameManagerRef.RemoteBudgetAllocationPhaseResponse();
    }
}
