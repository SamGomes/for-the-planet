using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class Player
{
    //General Stuff
    protected GameProperties.PlayerType type;

    protected int id;
    protected GameManager gameManagerRef;
    protected string name;
    protected int currBudget;
    protected float economicIndex;

    protected Dictionary<GameProperties.InvestmentTarget, int> currRoundInvestment;
    protected List<Dictionary<GameProperties.InvestmentTarget, int>> investmentHistory;


    //UI Stuff
    protected MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities;
    private Sprite UIAvatar;

    private GameObject canvas;
    private GameObject playerUI;
    private GameObject playerMarkerUI;
    private GameObject playerDisablerUI;
    private GameObject playerSelfDisablerUI;
    protected Button playerActionButtonUI;

    private Text currBudgetUI;

    private Text nameTextUI;
    private Text moneySliderUI;

    private GameObject budgetAllocationScreenUI;
    private GameObject displayHistoryScreenUI;
    private GameObject budgetExecutionScreenUI;

    protected Button spendTokenInEconomicGrowthButtonUI;
    protected Button removeTokenInEconomicGrowthButtonUI;
    protected Text economicGrowthTokensDisplayUI;

    protected Button spendTokenInEnvironmentButtonUI;
    protected Button removeTokenInEnvironmentButtonUI;
    protected Text environmentTokensDisplayUI;

    private PoppupScreenFunctionalities warningScreenRef;

    protected GameObject speechBalloonUI;


    public Player(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PoppupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
    {
        //General stuff
        this.gameManagerRef = GameGlobals.gameManager;
        this.id = id;
        this.name = name;
        this.economicIndex = 0.0f;

        //UI stuff
        this.type = GameProperties.PlayerType.HUMAN;
        InitUI(playerUIPrefab, playerCanvas, warningScreenRef, UIAvatar);

        //position UI on canvas
        this.GetPlayerUI().transform.Translate(new Vector3(0, -GameGlobals.players.Count * (0.2f * Screen.height), 0));
        this.playerMonoBehaviourFunctionalities = playerMonoBehaviourFunctionalities;

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
        currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC] = 0;
        currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT] = 0;
        UpdateUI();
        BudgetAllocation();
    }
    public void HistoryDisplayPhaseRequest()
    {
        UpdateUI();
        HistoryDisplay();
    }
    public void BudgetExecutionPhaseRequest()
    {
        UpdateUI();
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

    public void AddInvestment(GameProperties.InvestmentTarget target)
    {
        if (currBudget == 0)
        {
            warningScreenRef.DisplayPoppup("No more dices available in the current budget!");
            return;
        }
        currBudget--;
        currRoundInvestment[target]++;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name,"ADDED_INVESTMENT", target.ToString() , "-");
        UpdateUI();
    }
    public void RemoveInvestment(GameProperties.InvestmentTarget target)
    {
        currBudget++;
        currRoundInvestment[target]--;

        GameGlobals.gameLogManager.WriteEventToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameGlobals.currGameRoundId.ToString(), this.id.ToString(), this.name, "REMOVED_INVESTMENT", target.ToString(), "-");
        UpdateUI();
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






    //UI Stuff

    private void UpdateUI()
    {
        economicGrowthTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        environmentTokensDisplayUI.text = currRoundInvestment[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
        currBudgetUI.text = currBudget.ToString();
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

        this.nameTextUI = playerUI.transform.Find("nameText").gameObject.GetComponent<Text>();
        this.moneySliderUI = playerUI.transform.Find("playerStateSection/InvestmentUI/Slider").gameObject.GetComponent<Text>();


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

        this.budgetAllocationScreenUI = playerUI.transform.Find("playerActionSection/playForInstrumentUI").gameObject;
        this.displayHistoryScreenUI = playerUI.transform.Find("playerActionSection/playForInstrumentUI").gameObject;
        this.budgetExecutionScreenUI = playerUI.transform.Find("playerActionSection/lastDecisionsUI").gameObject;

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


    public void ResetPlayer(params object[] args) { }

    public void BudgetAllocation() { }
    public void HistoryDisplay() { }
    public void BudgetExecution() { }
    
}