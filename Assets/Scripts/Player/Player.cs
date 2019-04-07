﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


public class Player
{
    //General Stuff
    protected GameProperties.PlayerType type;

    protected int id;
    protected GameManager gameManagerRef;
    protected string name;
    protected int currBudget;
    protected float money;

    protected Dictionary<GameProperties.InvestmentTarget, int> currRoundInvestment;
    protected Dictionary<GameProperties.InvestmentTarget, int> investmentHistory;


    //UI Stuff
    protected MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities;
    private Sprite UIAvatar;

    private GameObject canvas;
    private GameObject playerUI;
    private GameObject playerMarkerUI;
    private GameObject playerDisablerUI;
    private GameObject playerSelfDisablerUI;
    protected Button playerActionButtonUI;
    private UnityAction playerActionCall;
    
    private Text nameTextUI;
    private Slider moneySliderUI;

    private GameObject budgetAllocationScreenUI;
    private GameObject displayHistoryScreenUI;
    private GameObject budgetExecutionScreenUI;
    private GameObject investmentSimulationScreenUI;

    protected Button spendTokenInEconomicGrowthButtonUI;
    protected Button removeTokenInEconomicGrowthButtonUI;
    protected Text economicGrowthTokensDisplayUI;
    protected Text economicGrowthHistoryDisplay;

    protected Button spendTokenInEnvironmentButtonUI;
    protected Button removeTokenInEnvironmentButtonUI;
    protected Text environmentTokensDisplayUI;
    protected Text environmentHistoryDisplay;

    protected Button executeBudgetButton;

    private PopupScreenFunctionalities warningScreenRef;

    protected GameObject speechBalloonUI;

    public Player(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
    {
        this.gameManagerRef = GameGlobals.gameManager;
        this.id = id;
        this.name = name;
        this.playerMonoBehaviourFunctionalities = playerMonoBehaviourFunctionalities;
        this.warningScreenRef = warningScreenRef;

        this.money = 0.0f;

        this.type = GameProperties.PlayerType.HUMAN;

        investmentHistory = new Dictionary<GameProperties.InvestmentTarget, int>();
        investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        currRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        InitUI(playerUIPrefab, playerCanvas, warningScreenRef, UIAvatar);
    }

    public virtual void InitUI(GameObject playerUIPrefab, GameObject canvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar)
    {
        this.canvas = canvas;

        this.playerUI = Object.Instantiate(playerUIPrefab, canvas.transform);

        this.playerMarkerUI = playerUI.transform.Find("marker").gameObject;
        this.playerDisablerUI = playerUI.transform.Find("disabler").gameObject;
        this.playerSelfDisablerUI = playerUI.transform.Find("selfDisabler").gameObject;
        playerSelfDisablerUI.SetActive(false); //provide interaction by default

        this.UIAvatar = UIAvatar;

        GameObject UISpeechBalloonLeft = playerUI.transform.Find("speechBalloonLeft").gameObject;
        GameObject UISpeechBalloonRight = playerUI.transform.Find("speechBalloonRight").gameObject;
        this.speechBalloonUI = (this.id % 2 == 0) ? UISpeechBalloonLeft : UISpeechBalloonRight;
        UISpeechBalloonLeft.SetActive(false);
        UISpeechBalloonRight.SetActive(false);

        this.playerActionButtonUI = playerUI.transform.Find("playerActionSection/playerActionButton").gameObject.GetComponent<Button>();
        this.playerActionCall = delegate () { };

        this.nameTextUI = playerUI.transform.Find("nameText").gameObject.GetComponent<Text>();
        this.moneySliderUI = playerUI.transform.Find("playerStateSection/InvestmentUI/Slider").gameObject.GetComponent<Slider>();


        this.spendTokenInEconomicGrowthButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/Button").gameObject.GetComponent<Button>();
        this.spendTokenInEconomicGrowthButtonUI.onClick.AddListener(delegate () {
            if (currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] == 0)
            {
                warningScreenRef.DisplayPoppup("There is no budget to allocate!");
                return;
            }
            this.AddInvestment(GameProperties.InvestmentTarget.ECONOMIC, 1);
            this.RemoveInvestment(GameProperties.InvestmentTarget.ENVIRONMENT, 1);

        });
        this.economicGrowthTokensDisplayUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/display").gameObject.GetComponent<Text>();

        this.spendTokenInEnvironmentButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/Button").gameObject.GetComponent<Button>();
        this.spendTokenInEnvironmentButtonUI.onClick.AddListener(delegate () {
            if (currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] == 0)
            {
                warningScreenRef.DisplayPoppup("There is no budget to allocate!");
                return;
            }
            this.AddInvestment(GameProperties.InvestmentTarget.ENVIRONMENT, 1);
            this.RemoveInvestment(GameProperties.InvestmentTarget.ECONOMIC, 1);
        });
        this.environmentTokensDisplayUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/display").gameObject.GetComponent<Text>();

        this.economicGrowthHistoryDisplay = playerUI.transform.Find("playerActionSection/displayHistoryUI/tokenSelection/alocateEconomicGrowth/display").gameObject.GetComponent<Text>();
        this.environmentHistoryDisplay = playerUI.transform.Find("playerActionSection/displayHistoryUI/tokenSelection/alocateEnvironment/display").gameObject.GetComponent<Text>();

        this.executeBudgetButton = playerUI.transform.Find("playerActionSection/budgetExecutionUI/rollForInvestmentButton").gameObject.GetComponent<Button>();


        this.budgetAllocationScreenUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI").gameObject;
        this.displayHistoryScreenUI = playerUI.transform.Find("playerActionSection/displayHistoryUI").gameObject;
        this.budgetExecutionScreenUI = playerUI.transform.Find("playerActionSection/budgetExecutionUI").gameObject;
        this.investmentSimulationScreenUI = playerUI.transform.Find("playerActionSection/investmentSimulationUI").gameObject;

        nameTextUI.text = this.name;

        //position UI on canvas
        this.GetPlayerUI().transform.Translate(new Vector3(0, -GameGlobals.players.Count * (0.2f * Screen.height), 0));
    }

    public void ReceiveGameManager(GameManager gameManagerRef) {
        this.gameManagerRef = gameManagerRef;
    }

    public virtual void RegisterMeOnPlayersLog()
    {
        GameGlobals.gameLogManager.WritePlayerToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), this.id.ToString(), this.name, "-");
    }

    //public abstract void InformChoosePreferredInstrument(Player nextPlayer);
    //public abstract void InformBudgetAllocation(Player invoker, GameProperties.Instrument leveledUpInstrument);
    //public abstract void InformPlayForInstrument(Player nextPlayer);
    //public abstract void InformLastDecision(Player nextPlayer);
    //public abstract void InformRollDicesValue(Player invoker, int maxValue, int obtainedValue);
    //public abstract void InformAlbumResult(int albumValue, int marketValue);
    //public abstract void InformGameResult(GameProperties.GameState state);
    //public abstract void InformNewAlbum();

    public int GetCurrBudget()
    {
        return this.currBudget;
    }
    public void SetCurrBudget(int newBudget)
    {
        this.currBudget = newBudget;
    }

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
    public float GetMoney()
    {
        return this.money;
    }
    public Dictionary<GameProperties.InvestmentTarget, int> GetCurrRoundInvestment()
    {
        return this.currRoundInvestment;
    }
    public Dictionary<GameProperties.InvestmentTarget, int> GetInvestmentsHistory()
    {
        return this.investmentHistory;
    }

    public void BudgetAllocationPhaseRequest()
    {
        this.budgetAllocationScreenUI.SetActive(true);
        this.displayHistoryScreenUI.SetActive(false);
        this.budgetExecutionScreenUI.SetActive(false);
        this.investmentSimulationScreenUI.SetActive(false);
        this.playerActionButtonUI.gameObject.SetActive(true);
        
        int halfBudget = Mathf.FloorToInt(this.currBudget / 2.0f);
        int startingEconomicInv = halfBudget;
        int startingEnvironmentInv = currBudget - halfBudget;

        AddInvestment(GameProperties.InvestmentTarget.ECONOMIC, startingEconomicInv);
        AddInvestment(GameProperties.InvestmentTarget.ENVIRONMENT, startingEnvironmentInv);

        this.playerActionButtonUI.onClick.RemoveListener(playerActionCall);
        playerActionCall = delegate ()
        {
            //if (currBudget > 0)
            //{
            //    warningScreenRef.DisplayPoppup("There is still budget to allocate!");
            //    return;
            //}
            SendBudgetAllocationPhaseResponse();
        };
        this.playerActionButtonUI.onClick.AddListener(playerActionCall);
        
        BudgetAllocation();
    }
    public void HistoryDisplayPhaseRequest()
    {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(true);
        this.budgetExecutionScreenUI.SetActive(false);
        this.investmentSimulationScreenUI.SetActive(false);
        this.playerActionButtonUI.gameObject.SetActive(false);

        //this.playerActionButtonUI.onClick.RemoveListener(playerActionCall);
        //playerActionCall = delegate ()
        //{
        //SendHistoryDisplayPhaseResponse();
        //};
        //this.playerActionButtonUI.onClick.AddListener(playerActionCall);

        HistoryDisplay();
        SendHistoryDisplayPhaseResponse();
    }
    public void BudgetExecutionPhaseRequest()
    {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(false);
        this.budgetExecutionScreenUI.SetActive(true);
        this.investmentSimulationScreenUI.SetActive(false);
        this.playerActionButtonUI.gameObject.SetActive(false);

        this.playerActionButtonUI.onClick.RemoveListener(playerActionCall);
        playerActionCall = delegate ()
        {
            SendBudgetExecutionPhaseResponse();
        };
        this.playerActionButtonUI.onClick.AddListener(playerActionCall);
        this.executeBudgetButton.onClick.AddListener(playerActionCall);
        
        BudgetExecution();
    }
    public void InvestmentSimulationRequest()
    {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(false);
        this.budgetExecutionScreenUI.SetActive(false);
        this.investmentSimulationScreenUI.SetActive(true);
        this.playerActionButtonUI.gameObject.SetActive(false);
        InvestmentSimulation();
    }

    public virtual int SendBudgetAllocationPhaseResponse()
    {
        //hide chosen investments before next player turn
        this.economicGrowthTokensDisplayUI.text = "-";
        this.environmentTokensDisplayUI.text = "-";

        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.BudgetAllocationPhaseResponse(this));
        return 0;
    }
    public virtual int SendHistoryDisplayPhaseResponse()
    {
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.HistoryDisplayPhaseResponse(this));
        return 0;
    }
    public virtual int SendBudgetExecutionPhaseResponse()
    {
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.BudgetExecutionPhaseResponse(this));
        return 0;
    }
    public virtual int SendInvestmentSimulationPhaseResponse()
    {
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.InvestmentSimulationPhaseResponse(this));
        return 0;
    }

    public void AddInvestment(GameProperties.InvestmentTarget target, int amount)
    {
        if (currBudget == 0)
        {
            return;
        }
        currBudget--;
        currRoundInvestment[target]+=amount;
        investmentHistory[target]+=amount;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"ADDED_INVESTMENT", target.ToString() , "-");
        UpdateTokensUI();
        UpdateHistoryUI();
    }
    public void RemoveInvestment(GameProperties.InvestmentTarget target, int amount)
    {
        int currTargetInvestment = currRoundInvestment[target];
        if (currTargetInvestment == 0)
        {
            return;
        }
        currBudget++;
        currRoundInvestment[target]-=amount;
        investmentHistory[target]-=amount;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name, "REMOVED_INVESTMENT", target.ToString(), "-");
        UpdateTokensUI();
        UpdateHistoryUI();
    }

    public IEnumerator TakeAllMoney()
    {
        this.money = 0;
        yield return playerMonoBehaviourFunctionalities.StartCoroutine(AuxiliaryMethods.UpdateSliderValue(moneySliderUI, money));
    }
    public IEnumerator SetMoney(float money)
    {
        this.money = money;
        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"SET_MONEY", "-" , money.ToString());
        yield return playerMonoBehaviourFunctionalities.StartCoroutine(AuxiliaryMethods.UpdateSliderValue(moneySliderUI, money));
    }


    //UI Stuff
    private void UpdateTokensUI()
    {
        economicGrowthTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
    }
    private void UpdateHistoryUI()
    {
        economicGrowthHistoryDisplay.text = investmentHistory[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentHistoryDisplay.text = investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
    }

    public PopupScreenFunctionalities GetWarningScreenRef()
    {
        return this.warningScreenRef;
    }
    public GameObject GetPlayerCanvas()
    {
        return this.canvas;
    }
    public GameObject GetPlayerUI()
    {
        return this.playerUI;
    }
    public GameObject GetPlayerMarkerUI()
    {
        return this.playerMarkerUI;
    }
    public GameObject GetPlayerDisablerUI()
    {
        return this.playerDisablerUI;
    }
    public GameObject GetSpeechBaloonUI()
    {
        return this.speechBalloonUI;
    }
    public Sprite GetAvatarUI()
    {
        return this.UIAvatar;
    }

    public void DisableAllInputs()
    {
        playerSelfDisablerUI.SetActive(true);
    }
    public void EnableAllInputs()
    {
        playerSelfDisablerUI.SetActive(false);
    }

    public void ResetPlayerUI()
    {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(false);
        this.budgetExecutionScreenUI.SetActive(false);
        this.playerActionButtonUI.gameObject.SetActive(false);
        //this.playerMarkerUI.SetActive(false);
        //this.playerDisablerUI.SetActive(true);
    }
    //public void ResetPlayer()
    //{
    //    ResetPlayerUI(); //no need to init them again just hiding them
    //}


    public void BudgetAllocation() { }
    public void HistoryDisplay() { }
    public void BudgetExecution() { }
    public void InvestmentSimulation() { }
}