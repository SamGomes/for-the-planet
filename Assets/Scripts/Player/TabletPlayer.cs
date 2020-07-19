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
        thalamusConnector.Disconnect(id);
        thalamusConnector.Dispose();
    }

    public override int SendBudgetAllocationPhaseResponse()
    {
        int currentInvestment = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT];
        environmentInvestmentPerRound.Add(currentInvestment);
        thalamusConnector.SendBudgetAllocation(GameGlobals.tabletID, currentInvestment);
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
        //investmentSliderUI.value = ((float)envAllocation) / 2;
        environmentInvestmentPerRound.Add(envAllocation);
        unallocatedBudget = GameGlobals.roundBudget;

        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = envAllocation;
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = (GameGlobals.roundBudget - envAllocation);
        investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] += envAllocation;
        investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] += (GameGlobals.roundBudget - envAllocation);
        gameManagerRef.RemoteBudgetAllocationPhaseResponse();
    }
}
