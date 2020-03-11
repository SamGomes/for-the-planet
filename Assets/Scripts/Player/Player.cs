using System;
using RolePlayCharacter;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using Object = UnityEngine.Object;

public class Player
{

    public float lastEconomicDecay;
    public float lastEconomicResult;
    public float lastEnvironmentResult;

    protected int id;
    protected GameManager gameManagerRef;
    protected string type;
    protected string name;
    protected int roundBudget;
    protected int unallocatedBudget;
    protected float money;

    protected Dictionary<GameProperties.InvestmentTarget, int> currRoundInvestment;
    protected Dictionary<GameProperties.InvestmentTarget, int> investmentHistory;


    //UI Stuff
    protected MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities;
    private Sprite UIAvatar;

    private GameObject canvas;
    protected GameObject playerUI;
    protected GameObject playerMarkerUI;
    protected GameObject playerDisablerUI;
    protected GameObject playerSelfDisablerUI;
    protected Button playerActionButtonUI;

    private Text nameTextUI;
    private Slider moneySliderUI;

    protected GameObject budgetAllocationScreenUI;
    protected GameObject displayHistoryScreenUI;
    protected GameObject budgetExecutionScreenUI;
    protected GameObject investmentSimulationScreenUI;

    protected Button spendTokenInEconomicGrowthButtonUI;
    private Text economicGrowthTokensDisplayUI;
    private Text economicGrowthHistoryDisplay;

    protected Button spendTokenInEnvironmentButtonUI;
    private Text environmentTokensDisplayUI;
    private Text environmentHistoryDisplay;

    protected Button executeBudgetButton;
    protected PopupScreenFunctionalities warningScreenRef;
    private DynamicSlider dynamicSlider;

    public Player(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
    {
        gameManagerRef = GameGlobals.gameManager;
        this.id = id;
        this.name = name;
        playerMonoBehaviourFunctionalities = GameGlobals.monoBehaviourFunctionalities;
        this.warningScreenRef = warningScreenRef;

        money = 0.0f;
        this.type = type;

        investmentHistory = new Dictionary<GameProperties.InvestmentTarget, int>();
        investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        currRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        if (!GameGlobals.isSimulation)
        {
            InitUI(playerCanvas, warningScreenRef, UIAvatar);
        }
    }


    public void SpendTokenInEconomicGrowth()
    {
        if (currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] == 0)
        {
            warningScreenRef.DisplayPoppup("There is no budget to allocate!");
            return;
        }
        this.RemoveInvestment(GameProperties.InvestmentTarget.ENVIRONMENT, 1);
        this.AddInvestment(GameProperties.InvestmentTarget.ECONOMIC, 1);

    }

    public void SpendTokenInEnvironment()
    {
        if (currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] == 0)
        {
            warningScreenRef.DisplayPoppup("There is no budget to allocate!");
            return;
        }
        this.RemoveInvestment(GameProperties.InvestmentTarget.ECONOMIC, 1);
        this.AddInvestment(GameProperties.InvestmentTarget.ENVIRONMENT, 1);
    }
    


    public void InitUI(GameObject canvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar)
    {
        this.canvas = canvas;

        if (GameGlobals.areHumansOnSyncTablets)
        {
            playerUI = Object.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerUI/playerUItablet"), canvas.transform);
        }
        else
        {
            playerUI = Object.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerUI/playerUI"), canvas.transform);
        }

        playerMarkerUI = playerUI.transform.Find("marker").gameObject;
        playerDisablerUI = playerUI.transform.Find("disabler").gameObject;
        playerSelfDisablerUI = playerUI.transform.Find("selfDisabler").gameObject;
        playerSelfDisablerUI.SetActive(false); //provide interaction by default

        this.UIAvatar = UIAvatar;

        playerActionButtonUI = playerUI.transform.Find("playerActionSection/playerActionButton").gameObject.GetComponent<Button>();

        nameTextUI = playerUI.transform.Find("nameText").gameObject.GetComponent<Text>();
        moneySliderUI = playerUI.transform.Find("playerStateSection/InvestmentUI/Slider").gameObject.GetComponent<Slider>();

        spendTokenInEconomicGrowthButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/Button").gameObject.GetComponent<Button>();
        spendTokenInEconomicGrowthButtonUI.onClick.AddListener(SpendTokenInEconomicGrowth);
        economicGrowthTokensDisplayUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/display").gameObject.GetComponent<Text>();

        spendTokenInEnvironmentButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/Button").gameObject.GetComponent<Button>();
        spendTokenInEnvironmentButtonUI.onClick.AddListener(SpendTokenInEnvironment);
        environmentTokensDisplayUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/display").gameObject.GetComponent<Text>();

        economicGrowthHistoryDisplay = playerUI.transform.Find("playerActionSection/displayHistoryUI/tokenSelection/alocateEconomicGrowth/display").gameObject.GetComponent<Text>();
        environmentHistoryDisplay = playerUI.transform.Find("playerActionSection/displayHistoryUI/tokenSelection/alocateEnvironment/display").gameObject.GetComponent<Text>();

        executeBudgetButton = playerUI.transform.Find("playerActionSection/budgetExecutionUI/rollForInvestmentButton").gameObject.GetComponent<Button>();
        executeBudgetButton.onClick.AddListener(SendBudgetExecutionPhaseResponseDelegate);

        budgetAllocationScreenUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI").gameObject;
        displayHistoryScreenUI = playerUI.transform.Find("playerActionSection/displayHistoryUI").gameObject;
        budgetExecutionScreenUI = playerUI.transform.Find("playerActionSection/budgetExecutionUI").gameObject;
        investmentSimulationScreenUI = playerUI.transform.Find("playerActionSection/investmentSimulationUI").gameObject;

        nameTextUI.text = name;

        if (GameGlobals.areHumansOnSyncTablets)
        {
            dynamicSlider = new DynamicSlider(this.playerUI.transform.Find("playerStateSection/InvestmentUI/Slider").gameObject, true);
        }
        else
        {
            dynamicSlider = new DynamicSlider(this.playerUI.transform.Find("playerStateSection/InvestmentUI/Slider").gameObject);
        }

        //position UI on canvas
        playerUI.transform.Translate(new Vector3(0, -GameGlobals.players.Count * (0.2f * Screen.height)*0.9f, 0));

        ResetPlayerUI();
    }

    public void ResetPlayerUI()
    {
        budgetAllocationScreenUI.SetActive(false);
        displayHistoryScreenUI.SetActive(false);
        budgetExecutionScreenUI.SetActive(false);
        investmentSimulationScreenUI.SetActive(false);
        playerActionButtonUI.gameObject.SetActive(false);
        playerActionButtonUI.gameObject.SetActive(false);
    }


    public void ReceiveGameManager(GameManager gameManagerRef) {
        this.gameManagerRef = gameManagerRef;
    }


    public int GetId()
    {
        return this.id;
    }
    public string GetName()
    {
        return this.name;
    }
    public void SetName(string _name)
    {
        this.name = _name;
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


    public virtual void BudgetAllocationPhaseRequest()
    {
        if (!GameGlobals.isSimulation)
        {
            this.budgetAllocationScreenUI.SetActive(true);
            this.displayHistoryScreenUI.SetActive(false);
            this.budgetExecutionScreenUI.SetActive(false);
            this.investmentSimulationScreenUI.SetActive(false);
            this.playerActionButtonUI.gameObject.SetActive(true);
        }
        

        unallocatedBudget = GameGlobals.roundBudget;
        int halfBudget = Mathf.FloorToInt(this.unallocatedBudget / 2.0f);
        int startingEconomicInv = halfBudget;
        int startingEnvironmentInv = unallocatedBudget - halfBudget;

        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        AddInvestment(GameProperties.InvestmentTarget.ECONOMIC, startingEconomicInv);
        AddInvestment(GameProperties.InvestmentTarget.ENVIRONMENT, startingEnvironmentInv);

        if (!GameGlobals.isSimulation)
        {
            playerActionButtonUI.onClick.RemoveListener(SendBudgetAllocationPhaseResponseDelegate);
            playerActionButtonUI.onClick.AddListener(SendBudgetAllocationPhaseResponseDelegate);
        }
    }
    public virtual void HistoryDisplayPhaseRequest()
    {
        if (!GameGlobals.isSimulation)
        {
            budgetAllocationScreenUI.SetActive(false);
            displayHistoryScreenUI.SetActive(true);
            budgetExecutionScreenUI.SetActive(false);
            investmentSimulationScreenUI.SetActive(false);
            playerActionButtonUI.gameObject.SetActive(false);
            playerMarkerUI.SetActive(false);
            playerDisablerUI.SetActive(false);
        }

        SendHistoryDisplayPhaseResponse();
    }
    public virtual void BudgetExecutionPhaseRequest()
    {
        if (!GameGlobals.isSimulation)
        {
            budgetAllocationScreenUI.SetActive(false);
            displayHistoryScreenUI.SetActive(false);
            budgetExecutionScreenUI.SetActive(true);
            investmentSimulationScreenUI.SetActive(false);
            playerActionButtonUI.gameObject.SetActive(false);
        }
    }
    public virtual void InvestmentSimulationRequest()
    {
        if (!GameGlobals.isSimulation)
        {
            budgetAllocationScreenUI.SetActive(false);
            displayHistoryScreenUI.SetActive(false);
            budgetExecutionScreenUI.SetActive(false);
            investmentSimulationScreenUI.SetActive(true);
            playerActionButtonUI.gameObject.SetActive(false);
            playerMarkerUI.SetActive(false);
            playerDisablerUI.SetActive(false);
        }
        SendInvestmentSimulationPhaseResponse();
    }

    public virtual int SendBudgetAllocationPhaseResponse()
    {
        if (!GameGlobals.isSimulation)
        {
            economicGrowthTokensDisplayUI.text = "-";
            environmentTokensDisplayUI.text = "-";
        }
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.BudgetAllocationPhaseResponse(this));
        return 0;
    }
    public void SendBudgetAllocationPhaseResponseDelegate()
    {
        SendBudgetAllocationPhaseResponse();
    }
    public int SendHistoryDisplayPhaseResponse()
    {
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.HistoryDisplayPhaseResponse(this));
        return 0;
    }
    public int SendBudgetExecutionPhaseResponse()
    {
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.BudgetExecutionPhaseResponse(this));
        return 0;
    }
    public void SendBudgetExecutionPhaseResponseDelegate()
    {
        SendBudgetExecutionPhaseResponse();
    }
    public int SendInvestmentSimulationPhaseResponse()
    {
        playerMonoBehaviourFunctionalities.StartCoroutine(gameManagerRef.InvestmentSimulationPhaseResponse(this));
        return 0;
    }
    

    public void AddInvestment(GameProperties.InvestmentTarget target, int amount)
    {
        if (unallocatedBudget - amount < 0)
        {
            return;
        }
        unallocatedBudget -= amount;
        currRoundInvestment[target] += amount;
        investmentHistory[target] += amount;

        if (!GameGlobals.isSimulation)
        {
            UpdateTokensUI();
            UpdateHistoryUI();
        }
    }
    public void RemoveInvestment(GameProperties.InvestmentTarget target, int amount)
    {
        int currTargetInvestment = currRoundInvestment[target];
        if (currTargetInvestment - amount < 0)
        {
            return;
        }
        unallocatedBudget += amount;
        currRoundInvestment[target] -= amount;
        investmentHistory[target] -= amount;

        if (!GameGlobals.isSimulation)
        {
            UpdateTokensUI();
            UpdateHistoryUI();
        }
    }

    public IEnumerator TakeAllMoney()
    {
        this.money = 0;
        yield return playerMonoBehaviourFunctionalities.StartCoroutine(this.dynamicSlider.UpdateSliderValue(money));
    }
    public IEnumerator SetMoney(float money)
    {
        this.money = money;
        if (dynamicSlider != null)
        {
            yield return playerMonoBehaviourFunctionalities.StartCoroutine(this.dynamicSlider.UpdateSliderValue(money));
        }
    }


    //UI Stuff
    public void UpdateNameUI()
    {
        nameTextUI.text = name;
    }
    private void UpdateTokensUI()
    {
        economicGrowthTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
    }
    public void UpdateHistoryUI()
    {
        if (GameGlobals.areHumansOnSyncTablets)
        {
            economicGrowthHistoryDisplay.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
            environmentHistoryDisplay.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
        }
        else
        {
            economicGrowthHistoryDisplay.text = investmentHistory[GameProperties.InvestmentTarget.ECONOMIC].ToString();
            environmentHistoryDisplay.text = investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
        }
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

    public Sprite GetAvatarUI()
    {
        throw new NotImplementedException();
        return this.UIAvatar;
    }

    
    public IEnumerator SetEconomicDecay(float economicDecay)
    {
        this.lastEconomicDecay = economicDecay;
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(SetMoney(money - economicDecay));
    }

    public IEnumerator SetEconomicResult(float economicIncrease)
    {
        this.lastEconomicResult = economicIncrease;
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(SetMoney(money + economicIncrease));
    }
    public void SetEnvironmentResult(float environmentResult)
    {
        this.lastEconomicResult = environmentResult;
    }


    public string GetPlayerType()
    {
        return this.type;
    }

    public virtual void Perceive(List<WellFormedNames.Name> events) { }
}