using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Runtime.InteropServices;
using UnityEngine.EventSystems;

public class EndScreenFunctionalities : MonoBehaviour
{

    public GameObject poppupPrefab;


    public Button UIRestartGameButton;
    public Button UIEndGameButton;
    public Text UIWarningText;

    public GameObject UIAlbumCollectionDisplay;
    public GameObject UIIndividualTable;
    public GameObject UIIndividualTableEntryPrefab;

    public GameObject UIVictoryOverlay;
    public GameObject UILossOverlay;
    public GameObject UIFinishedGameOverlay;

    public GameObject mainScene;

    public GameObject albumUIPrefab;

    public GameObject playerUIPrefab;

    public GameObject UIVictoryBackground;
    public GameObject UILossBackground;

    private PopupScreenFunctionalities infoPoppupNeutralRef;


    [DllImport("__Internal")]
    private static extern void EnableCopyToClipboard(string text);

    private void RestartGame()
    {
        GameSceneManager.LoadStartScene();
        Debug.Log("numGamesToSimulate: " + (GameProperties.configurableProperties.numGamesToSimulate - GameGlobals.currGameId));
    }

    private IEnumerator LoadMainScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        mainScene.SetActive(true);
        LoadEndScreenUIElements();        
    }

    private void LoadEndScreenUIElements()
    {
        infoPoppupNeutralRef = new PopupScreenFunctionalities(false, null, null, poppupPrefab, mainScene, this.GetComponent<MonoBehaviourFunctionalities>(), Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f));

        GameGlobals.players.Sort(SortPlayersByMoney);
        int numPlayers = GameGlobals.players.Count;
        for (int i = 0; i < numPlayers; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            GameObject newTableEntry = Object.Instantiate(UIIndividualTableEntryPrefab, UIIndividualTable.transform);
            newTableEntry.GetComponentsInChildren<Text>()[0].text = currPlayer.GetName();
            newTableEntry.GetComponentsInChildren<Text>()[1].text = currPlayer.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC].ToString();
        }
        
        //Text UIEndGameButtonText = UIEndGameButton.GetComponentInChildren<Text>();
        Text UIRestartGameButtonText = UIRestartGameButton.GetComponentInChildren<Text>();
        if (GameProperties.configurableProperties.isAutomaticBriefing)
        {
            if (GameGlobals.currGameId >= GameProperties.configurableProperties.numSessionGames)
            {
                //infoPoppupNeutralRef.DisplayPoppup("You reached the end of the experiment. You should now fill in the first questionnaire and you need to memorize the following code and also your score.");
                infoPoppupNeutralRef.DisplayPoppup("You reached the end of the second game. Please write down your score, as well as the following gamecode, and fill the second questionnaire to finish the experiment.");
                //UIEndGameButton.gameObject.SetActive(true);
                //UIEndGameButton.interactable = true;
                //UIEndGameButtonText.text = "Final Notes";
                UIWarningText.text = "You reached the end of the second game.Please write down your score, as well as the following gamecode, and fill the second questionnaire to finish the experiment.";

                UIEndGameButton.gameObject.SetActive(false);
                UIEndGameButton.interactable = false;
                UIRestartGameButton.gameObject.SetActive(false);
                UIRestartGameButton.interactable = false;

                UIFinishedGameOverlay.SetActive(true);
            }
            else
            {
                //infoPoppupNeutralRef.DisplayPoppup("You reached the end of one of the games to play in this session. We assume that you are prepared for the experiment game. Good luck!");
                infoPoppupNeutralRef.DisplayPoppup("You reached the end of the first game. Please write down your score (the total amount of money you made) and fill the first questionnaire. Please, do not move to next game until the questionnaire mentions you to do so.");
                UIRestartGameButton.gameObject.SetActive(true);
                UIRestartGameButton.interactable = true;
                UIRestartGameButtonText.text = "Next game";
                UIWarningText.text = "Please write down your score (the total amount of money you made) and fill the first questionnaire. Please, do not move to next game until the questionnaire mentions you to do so.";
                
                UIEndGameButton.gameObject.SetActive(false);
                UIEndGameButton.interactable = false;

                UIFinishedGameOverlay.SetActive(false);
            }

        }
        else
        {
            UIRestartGameButton.gameObject.SetActive(true);
            UIRestartGameButton.interactable = true;
            UIRestartGameButtonText.text = "Restart Game";
            UIEndGameButton.gameObject.SetActive(false);
            UIEndGameButton.interactable = false;
        }

        UIRestartGameButton.onClick.AddListener(delegate () {
            RestartGame();
        });
       
    }

    //in order to sort the players list by money earned
    public int SortPlayersByMoney(Player p1, Player p2)
    {
        return -1*(p1.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC]).CompareTo(p2.GetCurrRoundInvestment()[GameProperties.InvestmentTarget.ECONOMIC]);
    }


    // Use this for initialization
    void Start()
    {
        //mock
        GameProperties.configurableProperties = new DynamicallyConfigurableGameProperties();
        GameProperties.configurableProperties.numSessionGames = 3;
        GameProperties.configurableProperties.isAutomaticBriefing = true;
        GameGlobals.currSessionId = System.DateTime.Now.ToString("yyyy/MM/dd/HH-mm-ss");
        GameGlobals.gameLogManager = new DebugLogManager();
        GameGlobals.gameLogManager.InitLogs();
        //GameGlobals.albums = new List<Album>(5);
        //Album newAlbum = new Album("1", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        //newAlbum = new Album("2", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        //newAlbum = new Album("3", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        //newAlbum = new Album("4", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        //newAlbum = new Album("5", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        //newAlbum = new Album("6", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        //newAlbum = new Album("7", albumUIPrefab);
        //GameGlobals.albums.Add(newAlbum);
        GameGlobals.players = new List<Player>(5);
        Player playerToBeAdded = new Player(playerUIPrefab, new GameObject(), GameGlobals.monoBehaviourFunctionalities, infoPoppupNeutralRef, Resources.Load<Sprite>("Textures/UI/Icons/" + 0), 0, "Troll");
        GameGlobals.players.Add(playerToBeAdded);
        //GameGlobals.players.Add(new UIPlayer(1, "PL2"));
        //GameGlobals.players.Add(new UIPlayer(2, "PL3"));
        GameGlobals.currGameState = GameProperties.GameState.VICTORY;
        GameGlobals.currGameId = 2;

        GameGlobals.gameLogManager.UpdateGameResultInLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameProperties.currSessionParameterization.id, GameGlobals.currGameState.ToString());


        UIVictoryOverlay.SetActive(false);
        UILossOverlay.SetActive(false);
        UIFinishedGameOverlay.SetActive(false);

        UIVictoryBackground.SetActive(false);
        UILossBackground.SetActive(false);

        mainScene.SetActive(false);

        if (GameGlobals.currGameState == GameProperties.GameState.VICTORY)
        {
            //UIVictoryOverlay.SetActive(true);
            UIVictoryBackground.SetActive(true);
        }
        else if (GameGlobals.currGameState == GameProperties.GameState.LOSS)
        {
            //UILossOverlay.SetActive(true);
            UILossBackground.SetActive(true);

        }
        else
        {
            Debug.Log("[ERROR]: Game state returned NON FINISHED on game end!");
            return;
        }



        //GameGlobals.gameLogManager.WriteGameToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), GameProperties.currGameParameterization.id, GameGlobals.currGameState.ToString());
        GameGlobals.gameLogManager.EndLogs();



        if (GameProperties.configurableProperties.isSimulation)
        {
            if (GameGlobals.currGameId < GameProperties.configurableProperties.numGamesToSimulate)
            {
                RestartGame();
            }
            return;
        }
        else
        {

            StartCoroutine(LoadMainScreenAfterDelay(5.0f));
        }
    }


    
}
    

