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

    public int WinnerID;
    
    public GameObject economicTableUI;
    public GameObject environmentContributionsTableUI;
    public GameObject tableEntryPrefab;
    public Text summaryText;

    public GameObject victoryOverlayUI;
    public GameObject lossOverlayUI;

    public GameObject mainScene;
    public GameObject endScreen;

    public GameObject victoryBackgroundUI;
    public GameObject lossBackgroundUI;

    public int lastRound;

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
            GameGlobals.gameSceneManager.LoadStartScene();
        }
        else
        {
            Debug.Log("[Game: " + GameGlobals.currGameId +" of "+GameProperties.configurableProperties.numGamesToPlay+"]");
        }
    }

    private IEnumerator LoadMainScreenAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadEndScreenUIElements();
    }

    private void LoadEndScreenUIElements()
    {
        endScreen = GameObject.Find("EndScreen");
        mainScene.SetActive(true);

            infoPoppupNeutralRef = new PopupScreenFunctionalities(false, null, null, poppupPrefab, mainScene, this.GetComponent<MonoBehaviourFunctionalities>(), Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f));
            Button UIRestartGameButton = endScreen.transform.Find("restartGameButton").gameObject.GetComponent<Button>();
            Text UIRestartGameButtonText = UIRestartGameButton.GetComponentInChildren<Text>();
            UIRestartGameButton.gameObject.SetActive(false);
            UIRestartGameButton.interactable = false;
            //Button UIEndGameButton = mainScreen.transform.Find("endGameButton").gameObject.GetComponent<Button>();

            if (GameGlobals.currGameId >= GameProperties.configurableProperties.numGamesToPlay)
            {
                infoPoppupNeutralRef.DisplayPoppup("You reached the end of the second game. Please write down your score, as well as the following gamecode, and fill the second questionnaire to finish the experiment.");

                /*UIEndGameButton.gameObject.SetActive(false);
                UIEndGameButton.interactable = false;
                UIRestartGameButton.gameObject.SetActive(false);
                UIRestartGameButton.interactable = false;

            /*
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
            }*/
            UIRestartGameButtonText.text = "Restart game";
            UIRestartGameButton.onClick.AddListener(delegate () {
                RestartGame();
            });

            /*UIEndGameButton.onClick.AddListener(delegate () {
                //Do nothing
            });*/
        }
    }

    
    

    //in order to sort the players list by money earned
    public int SortPlayersByMoney(Player p1, Player p2)
    {
        return -1*(p1.GetMoney().CompareTo(p2.GetMoney()));
    }

    //check which player won
    public void CheckWinner()
    {
        int gains = 0;
        foreach (Player p in GameGlobals.players)
        {
            if(p.gains > gains)
            {
                gains = p.gains;
                WinnerID = p.GetId();
            }
        }
    }

    public void TakeEnvExplodeGains()
    {
        foreach (Player p in GameGlobals.players)
        {
            p.gains -= p.environmentInvestmentPerRound[this.lastRound-1];
        }
    }

    public bool CheckEnvExplode()
    {
        for (int i = 0; i < GameProperties.configurableProperties.maxNumRounds; i++)
        {
            if (i < GameGlobals.envStatePerRound.Count)
            {
                //DO NOTHING
            }
            else
            {
                this.lastRound = i;
                return true;
            }
        }
        return false;
    }

    public void SendLastMongo()
    {
        GameGlobals.callMongoLogServer = gameObject.AddComponent<CallMongoLogServer>();
        int EndenvState = 0;
        if (System.Convert.ToInt32(GameGlobals.envState) < 0)
        {
            EndenvState = 0;
        }
        else
        {
            EndenvState = System.Convert.ToInt32(GameGlobals.envState);
        }
        string endstate;
        if (System.Convert.ToInt32(GameGlobals.envState) < GameGlobals.envThreshold)
        {
            endstate = "LOSER";
        }
        else
        {
            endstate = "WINNER";
        }

        Dictionary<string, string> logEntry = new Dictionary<string, string>()
                    {
                        {"currSessionId", GameGlobals.currSessionId},
                        {"currGameId", GameGlobals.currGameId.ToString()},
                        {"generation", GameGlobals.generation.ToString()},
                        {"playerName", GameGlobals.players[WinnerID].GetName().ToString()},
                        {"playerType", endstate},
                        {"playerTookFromCP", "LASTROUND"},
                        {"playerGain", GameGlobals.players[WinnerID].GetGains().ToString()},
                        {"nCollaboration", GameGlobals.players[WinnerID].GetNCollaboration().ToString()},
                        {"envState", EndenvState.ToString()}
                    };

        GameGlobals.callMongoLogServer.SentLog(logEntry);
    }

    // Use this for initialization
    void Start()
    {
        StartCoroutine(YieldedStart());
    }

    IEnumerator YieldedStart()
    {
        CheckWinner();
        //Check if Env explode
        if (CheckEnvExplode())
        {
            TakeEnvExplodeGains();
        }
        foreach (Player p in GameGlobals.players)
            {
                GameObject newTableEntry = Object.Instantiate(tableEntryPrefab, environmentContributionsTableUI.transform);
                if(p.GetId() == this.WinnerID && p.GetId() == 0)
                {
                    newTableEntry.GetComponentsInChildren<Text>()[0].text = p.GetName() + " (YOU)";
                    newTableEntry.GetComponentsInChildren<Text>()[0].color = Color.yellow;
                    newTableEntry.GetComponentsInChildren<Text>()[0].fontStyle = FontStyle.Bold;
                }
                else if(p.GetId() == 0)
                {
                    newTableEntry.GetComponentsInChildren<Text>()[0].text = p.GetName() + "(YOU)";
                }

                else if (p.GetId() == this.WinnerID)
                {
                    newTableEntry.GetComponentsInChildren<Text>()[0].text = p.GetName();
                    newTableEntry.GetComponentsInChildren<Text>()[0].color = Color.yellow;
                }

                else {
                    newTableEntry.GetComponentsInChildren<Text>()[0].text = p.GetName();

                }
                
                for (int i = 0; i < GameProperties.configurableProperties.maxNumRounds; i++)
                {
                    if (i < p.environmentInvestmentPerRound.Count)
                    {
                        int playerInvestmentPerRound = p.environmentInvestmentPerRound[i];
                        Text textEntry = newTableEntry.GetComponentsInChildren<Text>()[i+1];
                        textEntry.text = playerInvestmentPerRound.ToString();

                        /*if (playerInvestmentPerRound == p.environmentMedianInvestmentPerRound[i])
                        {
                            textEntry.color = Color.yellow;
                        }*/
                    }
                    else
                    {
                        newTableEntry.GetComponentsInChildren<Text>()[i + 1].text = "-";
                    }
                }
            newTableEntry.GetComponentsInChildren<Text>()[11].text = p.gains.ToString();
            }
            GameObject newDummyLineEntry = Object.Instantiate(tableEntryPrefab, environmentContributionsTableUI.transform);
            newDummyLineEntry.GetComponentsInChildren<Text>()[0].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[1].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[2].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[3].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[4].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[5].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[6].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[7].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[8].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[9].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[10].text = "";
            newDummyLineEntry.GetComponentsInChildren<Text>()[11].text = "";

            GameObject environmentEntry = Object.Instantiate(tableEntryPrefab, environmentContributionsTableUI.transform);
            Text textGameObject = environmentEntry.GetComponentsInChildren<Text>()[0];
            textGameObject.text = "ENVIRONMENT";
            environmentEntry.GetComponentsInChildren<Text>()[11].text = "";
            textGameObject.fontStyle = FontStyle.Bold;
            for (int i = 0; i < GameProperties.configurableProperties.maxNumRounds; i++)
            {
                if (i < GameGlobals.envStatePerRound.Count)
                {
                    textGameObject = environmentEntry.GetComponentsInChildren<Text>()[i + 1];
                    textGameObject.text = GameGlobals.envStatePerRound[i].ToString();
                    textGameObject.fontStyle = FontStyle.Bold;

                    if (GameGlobals.envStatePerRound[i] < GameGlobals.envThreshold)
                    {
                        textGameObject.color = Color.red;
                    }
                    else if (i == GameProperties.configurableProperties.maxNumRounds - 1)
                    {
                        textGameObject.color = Color.green;
                    }
                }
                else
                {
                    textGameObject = environmentEntry.GetComponentsInChildren<Text>()[i + 1];
                    textGameObject.text = "-";
                    textGameObject.fontStyle = FontStyle.Bold;
                    textGameObject.color = Color.red;
                }

            }
        SendLastMongo();
            summaryText = GameObject.Find("SummaryText").GetComponent<Text>();
            if(GameGlobals.envStatePerRound[GameGlobals.currGameRoundId - 1]>0)
            {
            summaryText.text = "Winner of the Game: " + GameGlobals.players[WinnerID].GetName() + "!\n" +
            "You started with 60 and ended with " + GameGlobals.envStatePerRound[GameGlobals.currGameRoundId - 1].ToString();
            }
            else
            {
            summaryText.text = "You started with 60 and ended with 0";


             }
            //summaryText.text += "\n" + "YOU WON";
        /*
        else
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
        }*/



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
    

