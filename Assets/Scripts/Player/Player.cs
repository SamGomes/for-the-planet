using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public abstract class Player
{
    protected GameProperties.PlayerType type;

    protected int id;
    protected GameManager gameManagerRef;
    protected string name;
    protected int currBudget;
    protected float economicIndex;

    protected Dictionary<GameProperties.InvestmentTarget, int> currRoundInvestment;
    protected List<Dictionary<GameProperties.InvestmentTarget, int>> investmentHistory;
  
    public Player(int id, string name)
    {
        this.gameManagerRef = GameGlobals.gameManager;
        this.id = id;
        this.name = name;
        this.economicIndex = 0.0f;
    }

    public void ReceiveGameManager(GameManager gameManagerRef) {
        this.gameManagerRef = gameManagerRef;
    }

    public virtual void RegisterMeOnPlayersLog()
    {
        GameGlobals.gameLogManager.WritePlayerToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), this.id.ToString(), this.name, "-");
    }

    public abstract void ResetPlayer(params object[] args);

    public abstract void BudgetAllocation();
    public abstract void HistoryDisplay();
    public abstract void BudgetExecution();

    //public abstract void InformChoosePreferredInstrument(Player nextPlayer);
    //public abstract void InformBudgetAllocation(Player invoker, GameProperties.Instrument leveledUpInstrument);
    //public abstract void InformPlayForInstrument(Player nextPlayer);
    //public abstract void InformLastDecision(Player nextPlayer);
    //public abstract void InformRollDicesValue(Player invoker, int maxValue, int obtainedValue);
    //public abstract void InformAlbumResult(int albumValue, int marketValue);
    //public abstract void InformGameResult(GameProperties.GameState state);
    //public abstract void InformNewAlbum();
    
    public int GetId()
    {
        return this.id;
    }
    public string GetName()
    {
        return this.name;
    }
    public GameProperties.PlayerType GetPlayerType()
    {
        return this.type;
    }
    public float GetEconomicIndex()
    {
        return this.economicIndex;
    }
    public Dictionary<GameProperties.InvestmentTarget, int> GetCurrRoundInvestment()
    {
        return this.currRoundInvestment;
    }
    public List<Dictionary<GameProperties.InvestmentTarget, int>> GetInvestmentsHistory()
    {
        return this.investmentHistory;
    }

    public void BudgetAllocationPhaseRequest()
    {
        currRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        BudgetAllocation();
    }
    public void HistoryDisplayPhaseRequest()
    {
        HistoryDisplay();
    }
    public void BudgetExecutionPhaseRequest()
    {
        BudgetExecution();
    }

    public virtual int SendBudgetAllocationPhaseResponse()
    {
        gameManagerRef.BudgetAllocationPhaseResponse(this);
        return 0;
    }
    public virtual int SendHistoryDisplayPhaseResponse()
    {
        gameManagerRef.HistoryDisplayPhaseResponse(this);
        return 0;
    }
    public virtual int SendBudgetExecutionPhaseResponse()
    {
        gameManagerRef.BudgetExecutionPhaseResponse(this);
        return 0;
    }

    public virtual int AddInvestment(GameProperties.InvestmentTarget target)
    {
        currBudget--;
        currRoundInvestment[target]++;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"ADDED_INVESTMENT", target.ToString() , "-");
        return 0;
    }

    public void TakeAllMoney()
    {
        this.economicIndex = 0;
    }
    public void UpdateEconomicIndex(float economicGrowth)
    {
        this.economicIndex += economicGrowth;
        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"ECONOMIC_GROWTH", "-" , economicGrowth.ToString());
    }

}


