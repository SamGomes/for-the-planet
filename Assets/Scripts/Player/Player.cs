using System.Collections;
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

    private Text currBudgetUI;

    private Text nameTextUI;
    private Slider moneySliderUI;

    private GameObject budgetAllocationScreenUI;
    private GameObject displayHistoryScreenUI;
    private GameObject budgetExecutionScreenUI;

    protected Button spendTokenInEconomicGrowthButtonUI;
    protected Button removeTokenInEconomicGrowthButtonUI;
    protected Text economicGrowthTokensDisplayUI;
    protected Text economicGrowthHistoryDisplay;

    protected Button spendTokenInEnvironmentButtonUI;
    protected Button removeTokenInEnvironmentButtonUI;
    protected Text environmentTokensDisplayUI;
    protected Text environmentHistoryDisplay;

    protected Button executeBudgetButton;

    private PoppupScreenFunctionalities warningScreenRef;

    protected GameObject speechBalloonUI;

    public Player(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PoppupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
    {
        this.gameManagerRef = GameGlobals.gameManager;
        this.id = id;
        this.name = name;

        this.type = GameProperties.PlayerType.HUMAN;

        InitDynamicData();
        InitUI(playerUIPrefab, playerCanvas, warningScreenRef, UIAvatar);

        //position UI on canvas
        this.GetPlayerUI().transform.Translate(new Vector3(0, -GameGlobals.players.Count * (0.2f * Screen.height), 0));
        this.playerMonoBehaviourFunctionalities = playerMonoBehaviourFunctionalities;
    }

    public void InitDynamicData()
    {
        this.money = 0.0f;

        investmentHistory = new Dictionary<GameProperties.InvestmentTarget, int>();
        investmentHistory[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        currRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;
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
        this.playerActionButtonUI.gameObject.SetActive(true);

        //reinit before starting phase (on request)
        currRoundInvestment = new Dictionary<GameProperties.InvestmentTarget, int>();
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;

        this.playerActionButtonUI.onClick.RemoveListener(playerActionCall);
        playerActionCall = delegate ()
        {
            if (currBudget > 0)
            {
                warningScreenRef.DisplayPoppup("There is still budget to allocate!");
                return;
            }
            SendBudgetAllocationPhaseResponse();
        };
        this.playerActionButtonUI.onClick.AddListener(playerActionCall);

        UpdateUI();
        BudgetAllocation();
    }
    public void HistoryDisplayPhaseRequest()
    {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(true);
        this.budgetExecutionScreenUI.SetActive(false);
        this.playerActionButtonUI.gameObject.SetActive(true);

        this.playerActionButtonUI.onClick.RemoveListener(playerActionCall);
        playerActionCall = delegate ()
        {
            SendHistoryDisplayPhaseResponse();
        };
        this.playerActionButtonUI.onClick.AddListener(playerActionCall);

        UpdateUI();
        HistoryDisplay();
    }
    public void BudgetExecutionPhaseRequest()
    {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(false);
        this.budgetExecutionScreenUI.SetActive(true);
        this.playerActionButtonUI.gameObject.SetActive(false);

        this.playerActionButtonUI.onClick.RemoveListener(playerActionCall);
        playerActionCall = delegate ()
        {
            SendBudgetExecutionPhaseResponse();
        };
        this.playerActionButtonUI.onClick.AddListener(playerActionCall);
        this.executeBudgetButton.onClick.AddListener(playerActionCall);

        UpdateUI();
        BudgetExecution();
    }

    public virtual int SendBudgetAllocationPhaseResponse()
    {
        this.economicGrowthTokensDisplayUI.text = "-";
        this.environmentTokensDisplayUI.text = "-";
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

    public void AddInvestment(GameProperties.InvestmentTarget target)
    {
        if (currBudget == 0)
        {
            warningScreenRef.DisplayPoppup("No more dices available in the current budget!");
            return;
        }
        currBudget--;
        currRoundInvestment[target]++;
        investmentHistory[target]++;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"ADDED_INVESTMENT", target.ToString() , "-");
        UpdateUI();
    }
    public void RemoveInvestment(GameProperties.InvestmentTarget target)
    {
        currBudget++;
        currRoundInvestment[target]--;
        investmentHistory[target]--;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name, "REMOVED_INVESTMENT", target.ToString(), "-");
        UpdateUI();
    }

    public void TakeAllMoney()
    {
        this.money = 0;
        UpdateUI();
    }
    public void UpdateMoney(float economicGrowth)
    {
        this.money += economicGrowth;
        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"ECONOMIC_GROWTH", "-" , economicGrowth.ToString());
        UpdateUI();
    }


    //UI Stuff

    private void UpdateUI()
    {
        economicGrowthTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
        currBudgetUI.text = currBudget.ToString();

        economicGrowthHistoryDisplay.text = investmentHistory[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentHistoryDisplay.text = investmentHistory[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();


        moneySliderUI.value = money;
    }

    public PoppupScreenFunctionalities GetWarningScreenRef()
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

    public virtual void InitUI(GameObject playerUIPrefab, GameObject canvas, PoppupScreenFunctionalities warningScreenRef, Sprite UIAvatar)
    {
        this.canvas = canvas;
        this.warningScreenRef = warningScreenRef;


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


        this.spendTokenInEconomicGrowthButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/more").gameObject.GetComponent<Button>();
        this.spendTokenInEconomicGrowthButtonUI.onClick.AddListener(delegate () {
            this.AddInvestment(GameProperties.InvestmentTarget.ECONOMIC);
        });
        this.removeTokenInEconomicGrowthButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/less").gameObject.GetComponent<Button>();
        this.removeTokenInEconomicGrowthButtonUI.onClick.AddListener(delegate () {
            this.RemoveInvestment(GameProperties.InvestmentTarget.ECONOMIC);
        });
        this.economicGrowthTokensDisplayUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEconomicGrowth/display").gameObject.GetComponent<Text>();


        this.spendTokenInEnvironmentButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/more").gameObject.GetComponent<Button>();
        this.spendTokenInEnvironmentButtonUI.onClick.AddListener(delegate () {
            this.AddInvestment(GameProperties.InvestmentTarget.ENVIRONMENT);
        });
        this.removeTokenInEnvironmentButtonUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/less").gameObject.GetComponent<Button>();
        this.removeTokenInEnvironmentButtonUI.onClick.AddListener(delegate () {
            this.RemoveInvestment(GameProperties.InvestmentTarget.ENVIRONMENT);
        });
        this.environmentTokensDisplayUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI/tokenSelection/alocateEnvironment/display").gameObject.GetComponent<Text>();

        this.currBudgetUI = playerUI.transform.Find("playerStateSection/currBudget").gameObject.GetComponent<Text>();

        this.economicGrowthHistoryDisplay = playerUI.transform.Find("playerActionSection/displayHistoryUI/economicGrowthHistory").gameObject.GetComponent<Text>();
        this.environmentHistoryDisplay = playerUI.transform.Find("playerActionSection/displayHistoryUI/environmentHistory").gameObject.GetComponent<Text>();

        this.executeBudgetButton = playerUI.transform.Find("playerActionSection/budgetExecutionUI/rollForInvestmentButton").gameObject.GetComponent<Button>();


        this.budgetAllocationScreenUI = playerUI.transform.Find("playerActionSection/budgetAllocationUI").gameObject;
        this.displayHistoryScreenUI = playerUI.transform.Find("playerActionSection/displayHistoryUI").gameObject;
        this.budgetExecutionScreenUI = playerUI.transform.Find("playerActionSection/budgetExecutionUI").gameObject;

        nameTextUI.text = this.name;
    }


    public void DisableAllInputs()
    {
        playerSelfDisablerUI.SetActive(true);
    }
    public void EnableAllInputs()
    {
        playerSelfDisablerUI.SetActive(false);
    }
    
    public void ResetPlayerUI() {
        this.budgetAllocationScreenUI.SetActive(false);
        this.displayHistoryScreenUI.SetActive(false);
        this.budgetExecutionScreenUI.SetActive(false);
        this.playerActionButtonUI.gameObject.SetActive(false);
    }
    public void ResetPlayer()
    {
        InitDynamicData();
        ResetPlayerUI(); //no need to init them again just hiding them
    }


    public void BudgetAllocation() { }
    public void HistoryDisplay() { }
    public void BudgetExecution() { }
    
}