using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Object = UnityEngine.Object;

public class Player
{
    // //global state change
    // protected float lastEconDelta;
    // protected float lastEnvDelta;

    //my state increases
    protected float myLastEconIncrease;
    protected float myLastEnvIncrease;
    
    //my state decays
    protected float myLastEconDecay;
    
    protected int id;
    protected GameManager gameManagerRef;
    protected readonly string playerType;
    protected readonly string name;
    protected int unallocatedBudget;
    protected float econ;

    private Dictionary<GameProperties.InvestmentTarget, int> currRoundInvestment;
    private Dictionary<GameProperties.InvestmentTarget, int> prevRoundInvestment;
    private Dictionary<GameProperties.InvestmentTarget, int> investmentHistory;

    protected float coopPerc; //based on previous actions

    //UI Stuff
    protected MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities;
    // private Sprite UIAvatar;

    protected GameObject playerUI;
    private GameObject playerMarkerUI;
    private GameObject playerDisablerUI;
    protected GameObject playerSelfDisablerUI;
    protected Button playerActionButtonUI;

    private Text nameTextUI;

    private GameObject budgetAllocationScreenUI;
    private GameObject displayHistoryScreenUI;
    private GameObject budgetExecutionScreenUI;
    private GameObject investmentSimulationScreenUI;

    protected Button spendTokenInEconomicGrowthButtonUI;
    private Text economicGrowthTokensDisplayUI;
    private Text economicGrowthHistoryDisplay;

    protected Button spendTokenInEnvironmentButtonUI;
    private Text environmentTokensDisplayUI;
    private Text environmentHistoryDisplay;

    protected Button executeBudgetButton;
    protected PopupScreenFunctionalities warningScreenRef;
    private DynamicSlider dynamicSlider;

    public Player(string playerType, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
    {

        myLastEconIncrease = 0.0f;
        myLastEnvIncrease = 0.0f;
        
        myLastEconDecay = 0.0f;
    
        coopPerc = 0.5f;
        
        gameManagerRef = GameGlobals.gameManager;
        this.id = id;
        this.name = name;
        playerMonoBehaviourFunctionalities = GameGlobals.monoBehaviourFunctionalities;
        this.warningScreenRef = warningScreenRef;

        econ = 0.0f;
        this.playerType = playerType;

        investmentHistory = new Dictionary<GameProperties.InvestmentTarget, int>();
        investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        currRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        if (!GameGlobals.isSimulation)
        {
            InitUI(playerCanvas, UIAvatar);
        }
    }


    public float GetMyLastEnvIncrease()
    {
        return myLastEnvIncrease;
    }
    public float GetMyLastEconIncrease()
    {
        return myLastEconIncrease;
    }
    public float GetMyLastEconDecay()
    {
        return myLastEconDecay;
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
    


    public void InitUI(GameObject canvas, Sprite UIAvatar)
    {
        playerUI = Object.Instantiate(Resources.Load<GameObject>("Prefabs/PlayerUI/playerUI"), canvas.transform);

        playerMarkerUI = playerUI.transform.Find("marker").gameObject;
        playerDisablerUI = playerUI.transform.Find("disabler").gameObject;
        playerSelfDisablerUI = playerUI.transform.Find("selfDisabler").gameObject;
        playerSelfDisablerUI.SetActive(false); //provide interaction by default

        // this.UIAvatar = UIAvatar;

        playerActionButtonUI = playerUI.transform.Find("playerActionSection/playerActionButton").gameObject.GetComponent<Button>();

        nameTextUI = playerUI.transform.Find("nameText").gameObject.GetComponent<Text>();


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

        dynamicSlider = new DynamicSlider(this.playerUI.transform.Find("playerStateSection/InvestmentUI/Slider").gameObject);

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
        return id;
    }
    public string GetName()
    {
        return name;
    }
    public float GetMoney()
    {
        return econ;
    }
    public Dictionary<GameProperties.InvestmentTarget, int> GetCurrRoundInvestment()
    {
        return currRoundInvestment;
    }
    public void SetCurrRoundInvestment(Dictionary<GameProperties.InvestmentTarget, int> currRoundInvestment)
    {
        this.currRoundInvestment = currRoundInvestment;
    }
    public Dictionary<GameProperties.InvestmentTarget, int> GetPrevRoundInvestment()
    {
        return prevRoundInvestment;
    }
    public Dictionary<GameProperties.InvestmentTarget, int> GetInvestmentsHistory()
    {
        return investmentHistory;
    }

    public float GetCoopPerc()
    {
        return coopPerc;
    }

    public virtual void BudgetAllocationPhaseRequest()
    {
        if (!GameGlobals.isSimulation)
        {
            budgetAllocationScreenUI.SetActive(true);
            displayHistoryScreenUI.SetActive(false);
            budgetExecutionScreenUI.SetActive(false);
            investmentSimulationScreenUI.SetActive(false);
            playerActionButtonUI.gameObject.SetActive(true);
        }
        

        unallocatedBudget = GameGlobals.roundBudget;
        int halfBudget = Mathf.FloorToInt(unallocatedBudget / 2.0f);
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

        coopPerc = 0.5f*coopPerc + 0.5f*((float) investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] /
                   (float) (investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] +
                            investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT]));
        
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
        //this forces passing by value
        prevRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        prevRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC];
        prevRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT];
        
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

    public int SendBudgetAllocationPhaseResponse()
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

    //UI Stuff
    public IEnumerator SetEconomicValue(float econ)
    {
        this.econ = Mathf.Clamp01(econ);
        if (dynamicSlider != null)
        {
            yield return playerMonoBehaviourFunctionalities.StartCoroutine(this.dynamicSlider.UpdateSliderValue(econ));
        }
    }
    
    private void UpdateTokensUI()
    {
        economicGrowthTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
    }
    private void UpdateHistoryUI()
    {
        economicGrowthHistoryDisplay.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentHistoryDisplay.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
    }

    public PopupScreenFunctionalities GetWarningScreenRef()
    {
        return warningScreenRef;
    }
    public GameObject GetPlayerUI()
    {
        return playerUI;
    }
    public GameObject GetPlayerMarkerUI()
    {
        return playerMarkerUI;
    }
    public GameObject GetPlayerDisablerUI()
    {
        return playerDisablerUI;
    }

    public IEnumerator UpdateEconomy(float myEconomicIncrease)
    {
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(SetEconomicValue(econ + myEconomicIncrease));
    }
    
    public void UpdateLastEconIncrease(float myEconomicIncrease)
    {
        myLastEconIncrease = myEconomicIncrease;
    }
    public void UpdateLastEnvIncrease(float myEnvIncrease)
    {
        myLastEnvIncrease = myEnvIncrease;
    }
    
    public void UpdateLastEconDecay(float myEconomicDecay)
    {
        myLastEconDecay = myEconomicDecay;
    }

    public string GetPlayerType()
    {
        return playerType;
    }

    public virtual void Perceive(List<WellFormedNames.Name> events) { }
}