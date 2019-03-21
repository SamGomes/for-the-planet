using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlayer : Player
{
    protected MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities;

    private GameObject canvas;
    private GameObject playerUI;
    private GameObject playerMarkerUI;
    private GameObject playerDisablerUI;
    private GameObject playerSelfDisablerUI;

    protected Button UIplayerActionButton;

    private Text UInameText;
    private Text UImoneyValue;
    
    private Text[] UISkillLevelsTexts;
    protected List<Button> UISkillIconsButtons;
    

    private GameObject UIBudgetAllocationScreen;
    private GameObject UIDisplayHistoryScreen;
    private GameObject UIBudgetExecutionScreen;

    private GameObject UILastDecisionsMegaHitScreen;
    protected Button UIReceiveMegaHitButton;
    protected Button UIStickWithMarketingMegaHitButton;

    private GameObject UILastDecisionsFailScreen;
    protected Button UIReceiveFailButton;

    protected Button UIspendTokenButton;

    protected Button UIbuyTokenButton;
    protected Button UIdiscardChangesButton;

    protected Button UIspendTokenInEnvironmentButton;
    protected Button UIspendTokenInEconomyButton;

    protected Button UInotRollDicesButton;
    protected Button UIrollForPreferredInstrumentButton;

    private PoppupScreenFunctionalities warningScreenRef;

    protected GameObject UISpeechBalloon;


    public UIPlayer(GameObject playerUIPrefab, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PoppupScreenFunctionalities warningScreenRef, int id, string name) : base(id, name)
    {
        this.type = GameProperties.PlayerType.HUMAN;

        InitUI(playerUIPrefab, playerCanvas, warningScreenRef);
        
        //position UI on canvas
        this.GetPlayerUI().transform.Translate(new Vector3(0, -GameGlobals.players.Count * (0.2f*Screen.height), 0));

        //position UI correctly depending on players number (table layout)
        //float refAngle = (180.0f / (numPlayers - 1));
        //((UIPlayer)currPlayer).GetPlayerUI().transform.RotateAround(new Vector3(510, 490, 0), new Vector3(0, 0, 1), (i * refAngle));
        //((UIPlayer)currPlayer).GetPlayerUI().transform.Rotate(new Vector3(0, 0, 1), -(i*refAngle) + 90.0f);
        
        //temporarily on canvas...
        this.playerMonoBehaviourFunctionalities = playerMonoBehaviourFunctionalities;
        //this.GetSpeechBaloonUI().GetComponentInChildren<Text>().text = Application.streamingAssetsPath +" "+ GameGlobals.FAtiMAScenarioPath;
        //this.GetSpeechBaloonUI().SetActive(true);
    }

    //simulation constructor makes UIPlayer init like Player
    public UIPlayer(int id, string name) : base(id, name)
    {
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
        return this.UISpeechBalloon;
    }

    public virtual void InitUI(GameObject playerUIPrefab, GameObject canvas, PoppupScreenFunctionalities warningScreenRef)
    {
        this.canvas = canvas;
        this.warningScreenRef = warningScreenRef;

        this.playerUI = Object.Instantiate(playerUIPrefab, canvas.transform);
        this.playerMarkerUI = playerUI.transform.Find("marker").gameObject;
        this.playerDisablerUI = playerUI.transform.Find("disabler").gameObject;
        this.playerSelfDisablerUI = playerUI.transform.Find("selfDisabler").gameObject;
        playerSelfDisablerUI.SetActive(false); //provide interaction by default

        GameObject UISpeechBalloonLeft = playerUI.transform.Find("speechBalloonLeft").gameObject;
        GameObject UISpeechBalloonRight = playerUI.transform.Find("speechBalloonRight").gameObject;
        this.UISpeechBalloon = (this.id%2==0)? UISpeechBalloonLeft : UISpeechBalloonRight;
        UISpeechBalloonLeft.SetActive(false);
        UISpeechBalloonRight.SetActive(false);

        this.UIplayerActionButton = playerUI.transform.Find("playerActionSection/playerActionButton").gameObject.GetComponent<Button>();

        this.UInameText = playerUI.transform.Find("nameText").gameObject.GetComponent<Text>();
        this.UImoneyValue = playerUI.transform.Find("playerStateSection/moneyValue").gameObject.GetComponent<Text>();

        this.UISkillLevelsTexts = playerUI.transform.Find("playerStateSection/skillTable/skillLevels").GetComponentsInChildren<Text>();
        this.UISkillIconsButtons = new List<Button>(playerUI.transform.Find("playerStateSection/skillTable/skillIcons").GetComponentsInChildren<Button>());
        //foreach(GameProperties.InvestmentTarget target in currRoundInvestment.Keys)
        //{
        //    UISkillIconsButtons[(int)target].GetComponent<Outline>().enabled = false;
        //    UISkillLevelsTexts[(int)target].transform.gameObject.SetActive(false);
        //    UISkillIconsButtons[(int)target].transform.gameObject.SetActive(false);
        //}
        

        this.UIBudgetAllocationScreen = playerUI.transform.Find("playerActionSection/playForInstrumentUI").gameObject;
        this.UIDisplayHistoryScreen = playerUI.transform.Find("playerActionSection/playForInstrumentUI").gameObject;
        this.UIBudgetExecutionScreen = playerUI.transform.Find("playerActionSection/lastDecisionsUI").gameObject;
        
        UInameText.text = this.name;
    }
    

    public void DisableAllInputs()
    {
        playerSelfDisablerUI.SetActive(true);
    }
    public void EnableAllInputs()
    {
        playerSelfDisablerUI.SetActive(false);
    }


    public override int SendBudgetAllocationPhaseResponse()
    {
        base.SendBudgetAllocationPhaseResponse();
        return 0;
    }
    public override int SendHistoryDisplayPhaseResponse()
    {
        base.SendHistoryDisplayPhaseResponse();
        return 0;
    }
    public override int SendBudgetExecutionPhaseResponse()
    {
        base.SendBudgetExecutionPhaseResponse();
        return 0;
    }
    public override void ResetPlayer(params object[] args) { }

    public override void BudgetAllocation() { }
    public override void HistoryDisplay() { }
    public override void BudgetExecution() { }
}
