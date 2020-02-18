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
    
    public GameObject economicTableUI;
    public GameObject environmentContributionsTableUI;
    public GameObject tableEntryPrefab;

    public GameObject victoryOverlayUI;
    public GameObject lossOverlayUI;

    public GameObject mainScene;

    public GameObject victoryBackgroundUI;
    public GameObject lossBackgroundUI;

    private PopupScreenFunctionalities infoPoppupNeutralRef;


    [DllImport("__Internal")]
    private static extern void EnableCopyToClipboard(string text);

    private void RestartGame()
    {
        GameGlobals.gameSceneManager.LoadStartScene();
        if (!GameGlobals.isSimulation)
        {
            foreach (Player player in GameGlobals.players) { 
                //destroy stuff saved between scenes (the dontdestroyonload scene is extracted through one of the players)
                foreach (var root in player.GetPlayerUI().scene.GetRootGameObjects())
                {
                    Destroy(root);
                }
            }
        }
        else
        {
            Debug.Log("Game: " + GameGlobals.currGameId +" of "+GameProperties.configurableProperties.numGamesToPlay+"]");
        }
    }

    private IEnumerator LoadMainScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadEndScreenUIElements();
    }

    private void LoadEndScreenUIElements()
    {
        mainScene.SetActive(true);
        
        infoPoppupNeutralRef = new PopupScreenFunctionalities(false, null, null, poppupPrefab, mainScene, this.GetComponent<MonoBehaviourFunctionalities>(), Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f));
        Text UIRestartGameButtonText = UIRestartGameButton.GetComponentInChildren<Text>();
        if (GameGlobals.currGameId >= GameProperties.configurableProperties.numGamesToPlay)
        {
            infoPoppupNeutralRef.DisplayPoppup("You reached the end of the second game. Please write down your score, as well as the following gamecode, and fill the second questionnaire to finish the experiment.");
            
            UIEndGameButton.gameObject.SetActive(false);
            UIEndGameButton.interactable = false;
            UIRestartGameButton.gameObject.SetActive(false);
            UIRestartGameButton.interactable = false;
            
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
        }
        UIRestartGameButtonText.text = "Restart game";
        UIRestartGameButton.onClick.AddListener(delegate () {
            RestartGame();
        });
    }

    
    

    //in order to sort the players list by money earned
    public int SortPlayersByMoney(Player p1, Player p2)
    {
        return -1*(p1.GetMoney().CompareTo(p2.GetMoney()));
    }


    // Use this for initialization
    void Start()
    {
        StartCoroutine(YieldedStart());
    }

    IEnumerator YieldedStart()
    {
        GameGlobals.players.Sort(SortPlayersByMoney);
        int numPlayers = GameGlobals.players.Count;
        for (int i = 0; i < numPlayers; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            GameObject newTableEntry = Object.Instantiate(tableEntryPrefab, economicTableUI.transform);
            newTableEntry.GetComponentsInChildren<Text>()[0].text = currPlayer.GetName();
            newTableEntry.GetComponentsInChildren<Text>()[1].text = currPlayer.GetMoney() * 100.0f + " %";
        }
        for (int i = 0; i < numPlayers; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            GameObject newTableEntry = Object.Instantiate(tableEntryPrefab, environmentContributionsTableUI.transform);
            newTableEntry.GetComponentsInChildren<Text>()[0].text = currPlayer.GetName();
            newTableEntry.GetComponentsInChildren<Text>()[1].text = currPlayer.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT].ToString();
        }


        for (int i = 0; i < GameGlobals.players.Count; i++)
        {
            Dictionary<string, string> gameLogEntry = new Dictionary<string, string>();
            gameLogEntry["sessionId"] = GameGlobals.currSessionId.ToString();
            gameLogEntry["currGameId"] = GameGlobals.currGameId.ToString();
            gameLogEntry["condition"] = GameProperties.currSessionParameterization.id;
            gameLogEntry["outcome"] = GameGlobals.currGameState.ToString();

            gameLogEntry["env_state"] = GameGlobals.envState.ToString();


            Player player = GameGlobals.players[i];
            float econInv = (float) player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ECONOMIC];
            float envInv = (float) player.GetInvestmentsHistory()[GameProperties.InvestmentTarget.ENVIRONMENT];

            gameLogEntry["playerId"] = player.GetId().ToString();

            gameLogEntry["pos"] = i.ToString();
            gameLogEntry["type"] = player.GetPlayerType();
            gameLogEntry["econ_state"] = player.GetMoney().ToString();
            gameLogEntry["econ_history_perc"] = econInv.ToString();
            gameLogEntry["env_history_perc"] = envInv.ToString();
            gameLogEntry["num_played_rounds"] = (GameGlobals.currGameRoundId + 1).ToString();

            yield return GameGlobals.gameLogManager.UpdateLog("fortheplanetlogs", "gameresultslog", "&q={\"currGameId\": \"" + GameGlobals.currGameId.ToString() + "\", \"sessionId\":\"" + GameGlobals.currSessionId.ToString() + "\", \"playerId\":\"" + player.GetId().ToString() + "\"}", gameLogEntry);
        }


        if (GameGlobals.isSimulation)
        {
            yield return GameGlobals.gameLogManager.EndLogs();

            if (GameGlobals.currGameId < GameProperties.configurableProperties.numGamesToPlay)
            {
                RestartGame();
            }
            else
            {
                Application.Quit();
            }
        }
        else
        {
            victoryOverlayUI.SetActive(false);
            lossOverlayUI.SetActive(false);

            victoryBackgroundUI.SetActive(false);
            lossBackgroundUI.SetActive(false);

            mainScene.SetActive(false);

            if (GameGlobals.currGameState == GameProperties.GameState.VICTORY)
            {
                victoryOverlayUI.SetActive(true);
                victoryBackgroundUI.SetActive(true);
            }
            else if (GameGlobals.currGameState == GameProperties.GameState.LOSS)
            {
                lossOverlayUI.SetActive(true);
                lossBackgroundUI.SetActive(true);

            }
            else
            {
                Debug.Log("[ERROR]: Game state returned NON FINISHED on game end!");
                yield break;
            }
            StartCoroutine(LoadMainScreenAfterDelay(5.0f));
        }
    }


    
}
    

