using AssetManagerPackage;
using FAtiMAScripts;

using IntegratedAuthoringTool;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using System.Runtime.InteropServices;
using UnityEngine.Networking;
using System.Globalization;
using System;

[Serializable]
public class DataEntryGameResultLog
{
    public string sessionId;
    public string currGameId;
    public string condition;
    public string outcome;
}

[Serializable]
public class DataEntryGameResultLogQueryResult
{
    public List<DataEntryGameResultLog> results;
}



public class StartScreenFunctionalities : MonoBehaviour {
    private StreamReader fileReader;
    
    private Transform UIStartScreen;
    public GameObject UILoadExternalConfigPrefab;

    private Button UIStartGameButton;
    public GameObject UIGameCodeDisplayPrefab;
    public GameObject monoBehaviourDummyPrefab;

    private int autoSessionConfigurationIndex;

    [DllImport("__Internal")]
    private static extern void EnableFileLoad(string fileText);


    private IEnumerator InitGameGlobals()
    {
        string configText = "";
        GameProperties.configurableProperties = new DynamicallyConfigurableGameProperties();


        //Assign configurable game properties from file if any
        //Application.ExternalEval("console.log('streaming assets: "+ Application.streamingAssetsPath + "')");

        string path = Application.streamingAssetsPath + "/config.cfg";
        if (path.Contains("://") || path.Contains(":///")) //url instead of path
        {
            UnityWebRequest www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();
            configText = www.downloadHandler.text;
        }
        else
        {
            configText = File.ReadAllText(path);
        }

        DynamicallyConfigurableGameProperties configs = JsonUtility.FromJson<DynamicallyConfigurableGameProperties>(configText);
        GameProperties.configurableProperties = configs;


        GameGlobals.isSimulation = configs.isSimulation || Application.isBatchMode;

        GameGlobals.numberOfSpeakingPlayers = 0;
        GameGlobals.currGameId++;
        GameGlobals.currGameRoundId = 0;

        GameGlobals.audioManager = new AudioManager();
        GameGlobals.gameSceneManager = new GameSceneManager();

        GameGlobals.currGameState = GameProperties.GameState.NOT_FINISHED;
        
        GameGlobals.players = new List<Player>();

        //GameGlobals.gameLogManager = new SilentLogManager();
        GameGlobals.gameLogManager = new MongoDBLogManager();
        GameGlobals.gameLogManager.InitLogs(GameGlobals.monoBehaviourFunctionalities);

        GameGlobals.roundBudget = configs.roundBudget;
        GameGlobals.environmentDecayBudget = configs.environmentDecayBudget;
        GameGlobals.playerDecayBudget = configs.playerDecayBudget;

        //only generate session data in the first game
        if (GameGlobals.currGameId == 1)
        {
            //GameGlobals.gameLogManager = new DebugLogManager();
            string date = System.DateTime.Now.ToString("ddHHmm");

            //generate external game code from currsessionid and lock it in place
            //gamecode is in the format ddmmhhmmss<3RandomLetters>[TestGameCondition]
            string generatedCode = date; //sb.ToString();
            //generate 3 random letters
            for (int i = 0; i < 3; i++)
            {
                generatedCode += (char)('A' + UnityEngine.Random.Range(0, 26));
            }
            GameGlobals.currSessionId = generatedCode;

            //update the gamecode UI
            //GameObject UIGameCodeDisplay = Object.Instantiate(UIGameCodeDisplayPrefab);
            //UIGameCodeDisplay.GetComponentInChildren<Text>().text = "Game Code: " + GameGlobals.currSessionId;
            //Object.DontDestroyOnLoad(UIGameCodeDisplay);
        }
        else
        {
            this.UIStartGameButton.interactable = true;
        }

        if (GameProperties.configurableProperties.isAutomaticBriefing) //generate condition automatically (asynchronous)
        {
            StartCoroutine(GameGlobals.gameLogManager.GetFromLog("fortheplanetlogs","gameresultslog", "&s={\"_id\": -1}&l=1", YieldedActionsAfterGet)); //changes session code
        }
        else
        {
            //create session parameterization
            SessionParameterization mock = new SessionParameterization("mock");
            GameProperties.configurableProperties.possibleParameterizations.Add(mock);
            this.UIStartGameButton.interactable = true;

            GameProperties.configurableProperties.numSessionGames = 0; //not used
        }

        //init fatima strings
        GameGlobals.FAtiMAScenarioPath = "/Scenarios/ForThePlanet.iat";

        AssetManager.Instance.Bridge = new AssetManagerBridge();
        GameGlobals.FAtiMAIat = IntegratedAuthoringToolAsset.LoadFromFile(GameGlobals.FAtiMAScenarioPath);
    }


    private int YieldedActionsAfterGet(string getResult)
    {
        getResult = "{ \"results\":"+ getResult + "}";
        string lastConditionString = "";
        List <DataEntryGameResultLog> results = JsonUtility.FromJson<DataEntryGameResultLogQueryResult>(getResult).results;
        if (results.Count < 1) //no games were found
        {
            List<SessionParameterization> possibleConditions = GameProperties.configurableProperties.possibleParameterizations;
            lastConditionString = possibleConditions[0].id;
        }
        else
        {
            lastConditionString = results[results.Count - 1].condition.ToString();
        }
        //if (GameGlobals.currGameId == 1)
        //{

        SetParameterizationCondition(lastConditionString);
        GameProperties.configurableProperties.numSessionGames = GameProperties.currSessionParameterization.gameParameterizations.Count;
        if (GameProperties.configurableProperties.numSessionGames >= 1)
        {
            this.UIStartGameButton.interactable = true;
        }
        else {
            Debug.Log("number of session games cannot be less than 1");
            this.UIStartGameButton.interactable = false;
        }
        //}
       
        // @jbgrocha: auto start if on batchmode
        if (GameGlobals.isSimulation)
        {
            StartGame();
            Debug.Log("In BatchMode");
        }

        return 0;
    }

    private int SetParameterizationCondition(string lastConditionString)
    {
        List<SessionParameterization> possibleConditions = GameProperties.configurableProperties.possibleParameterizations;

        int lastConditionIndex = -1;
        if (lastConditionString != "")
        {
            for (int i = 0; i < possibleConditions.Count; i++)
            {
                SessionParameterization currSessionParams = possibleConditions[i];
                if (currSessionParams.id == lastConditionString)
                {
                    lastConditionIndex = i;
                    break;
                }
            }
        }

        if (possibleConditions.Count <= 0)
        {
            Debug.Log("[WARNING]: isSimulation/ isAutomaticDebriefing is enabled but possibleConditions is still empty!");
        }
        else
        {
            autoSessionConfigurationIndex = (((int)lastConditionIndex) +1) % (possibleConditions.Count);
            GameProperties.currSessionParameterization = GameProperties.configurableProperties.possibleParameterizations[autoSessionConfigurationIndex];
            if (GameGlobals.currGameId == 1) GameGlobals.currSessionId += GameProperties.currSessionParameterization.id; //session code with last digit being the condition if any
        }
        return 0;
    }


    private void StartGame()
    {
        GameGlobals.gameSceneManager.LoadPlayersSetupScene();
    }

    public void InitGame()
    {
        //play theme song
        //GameGlobals.audioManager.PlayInfinitClip("Audio/theme/themeIntro", "Audio/theme/themeLoop");
        if(UIStartGameButton!=null)
            UIStartGameButton.onClick.AddListener(delegate () { StartGame(); });

        //thanks WebGL, because of you I've got to init a game global to init the rest of the game globals!
        GameObject monoBehaviourDummy = Instantiate(monoBehaviourDummyPrefab);
        DontDestroyOnLoad(monoBehaviourDummy);
        GameGlobals.monoBehaviourFunctionalities = monoBehaviourDummy.GetComponent<MonoBehaviourFunctionalities>();
        GameGlobals.monoBehaviourFunctionalities.StartCoroutine(InitGameGlobals());
        
    }


    private void Start()
    {
        CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;


        // Make the game perform as good as possible
        Application.targetFrameRate = 40;
        UIStartScreen = GameObject.Find("Canvas/StartScreen").transform;
       
        this.UIStartGameButton = GameObject.Find("Canvas/StartScreen/startGameButton").gameObject.GetComponent<Button>();
        this.UIStartGameButton.interactable = false;

        InitGame();
        
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(delegate () {
                GameGlobals.audioManager.PlayClip("Audio/snap");
            });
        }
    }
}
