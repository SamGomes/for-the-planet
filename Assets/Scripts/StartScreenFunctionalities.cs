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
using GAIPS.Rage;

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
        string path = Application.streamingAssetsPath + "/config.json";
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

        //only init audio manager if is not simulation
        if (!GameGlobals.isSimulation)
        {
            GameGlobals.audioManager = new AudioManager();
        }
        GameGlobals.gameSceneManager = new GameSceneManager();

        GameGlobals.currGameState = GameProperties.GameState.NOT_FINISHED;
        
        GameGlobals.players = new List<Player>();

        //GameGlobals.gameLogManager = new SilentLogManager();
//        GameGlobals.gameLogManager = new DebugLogManager();
        GameGlobals.gameLogManager = new MongoDBLogManager();

        GameGlobals.gameLogManager.InitLogs(GameGlobals.monoBehaviourFunctionalities);

        GameGlobals.roundBudget = configs.roundBudget;
        GameGlobals.environmentDecayBudget = configs.environmentDecayBudget;
        GameGlobals.playerDecayBudget = configs.playerDecayBudget;

        if (GameGlobals.storedRPCs == null) //maintain rpc cache when restarting
        {
            GameGlobals.storedRPCs = new Dictionary<string, RolePlayCharacter.RolePlayCharacterAsset>();
        }

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
        }
        
        StartCoroutine(GameGlobals.gameLogManager.GetFromLog("fortheplanetlogs","gameresultslog", "&s={\"_id\": -1}&l=1", YieldedActionsAfterGet)); //changes session code
        if (!GameGlobals.isSimulation)
        {
            this.UIStartGameButton.interactable = true;
        }

        
        
        // Making sure it works on Android and Web-GL
        var storagePath = "[\"EmotionalAppraisalAsset\",{\"root\":{\"classId\":0,\"Description\":null,\"AppraisalRules\":{\"AppraisalWeight\":1,\"Rules\":[{\"EventName\":\"Event(Property-Change, *, HistoryDisplay([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([econ], Minus, [env]), Div, Math([env], Plus, [econ])), Times, 10) = [p]\",\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Praiseworthiness\",\"Value\":\"[p]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, SELF, BeforeBudgetAllocation([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([econ], Minus, [env]), Div, Math([env], Plus, [econ])), Times, 10) = [d]\",\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Desirability\",\"Value\":\"[d]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, *, DecaySimulation([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, BudgetExecution([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, HistoryDisplay([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([env], Minus, [econ]), Div, Math([env], Plus, [econ])), Times, 10) = [p]\",\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Praiseworthiness\",\"Value\":\"[p]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, SELF, BeforeBudgetAllocation([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([econ], Minus, [env]), Div, Math([env], Plus, [econ])), Times, 10) = [d]\",\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Desirability\",\"Value\":\"[d]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, *, DecaySimulation([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, BudgetExecution([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, HistoryDisplay([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([env], Minus, [econ]), Div, Math([env], Plus, [econ])), Times, 10) = [p]\",\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Praiseworthiness\",\"Value\":\"[p]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, SELF, BeforeBudgetAllocation([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([env], Minus, [econ]), Div, Math([env], Plus, [econ])), Times, 10) = [d]\",\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Desirability\",\"Value\":\"[d]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, *, DecaySimulation([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, BudgetExecution([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, HistoryDisplay([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([env], Minus, [econ]), Div, Math([env], Plus, [econ])), Times, 10) = [p]\",\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Praiseworthiness\",\"Value\":\"[p]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, SELF, BeforeBudgetAllocation([econ], [env]), [target])\",\"Conditions\":{\"Set\":[\"Math(Math(Math([econ], Minus, [env]), Div, Math([env], Plus, [econ])), Times, 10) = [d]\",\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Desirability\",\"Value\":\"[d]\",\"Target\":\"[target]\"}]}},{\"EventName\":\"Event(Property-Change, *, DecaySimulation([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}},{\"EventName\":\"Event(Property-Change, *, BudgetExecution([gs]), [target])\",\"Conditions\":{\"Set\":[\"Ask(SELF) = AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC\"]},\"AppraisalVariables\":{\"AppraisalVariables\":[{\"Name\":\"Goal Success Probablity\",\"Value\":\"[gs]\",\"Target\":\"WinGame\"}]}}]}},\"types\":[{\"TypeId\":0,\"ClassName\":\"EmotionalAppraisal.EmotionalAppraisalAsset, EmotionalAppraisal, Version=1.4.1.0, Culture=neutral, PublicKeyToken=null\"}]},\"EmotionalDecisionMakingAsset\",{\"root\":{\"classId\":0,\"ActionTendencies\":[]},\"types\":[{\"TypeId\":0,\"ClassName\":\"EmotionalDecisionMaking.EmotionalDecisionMakingAsset, EmotionalDecisionMaking, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null\"}]},\"SocialImportanceAsset\",{\"root\":{\"classId\":0,\"AttributionRules\":[]},\"types\":[{\"TypeId\":0,\"ClassName\":\"SocialImportance.SocialImportanceAsset, SocialImportance, Version=1.5.0.0, Culture=neutral, PublicKeyToken=null\"}]},\"CommeillFautAsset\",{\"root\":{\"classId\":0,\"SocialExchanges\":[]},\"types\":[{\"TypeId\":0,\"ClassName\":\"CommeillFaut.CommeillFautAsset, CommeillFaut, Version=1.7.0.0, Culture=neutral, PublicKeyToken=null\"}]}]";
        var storage = AssetStorage.FromJson(storagePath);
        
        //init fatima strings
        GameGlobals.FAtiMAScenarioPath = Application.streamingAssetsPath + "/Scenarios/scenario.json";
        GameGlobals.FAtiMAIat = IntegratedAuthoringToolAsset.FromJson("{\"root\":{\"classId\":0,\"ScenarioName\":\"For-the-planet\",\"Description\":null,\"Characters\":[{\"KnowledgeBase\":{\"Perspective\":\"AI-EMOTIONAL-CONSTRUCTIVE-COLLECTIVIST\",\"Knowledge\":{}},\"BodyName\":null,\"VoiceName\":null,\"EmotionalState\":{\"Mood\":0,\"initialTick\":0,\"EmotionalPool\":[],\"AppraisalConfiguration\":{\"HalfLifeDecayConstant\":0.5,\"EmotionInfluenceOnMoodFactor\":0.3,\"MoodInfluenceOnEmotionFactor\":0.3,\"MinimumMoodValueForInfluencingEmotions\":0.5,\"EmotionalHalfLifeDecayTime\":15,\"MoodHalfLifeDecayTime\":60}},\"AutobiographicMemory\":{\"Tick\":0,\"records\":[]},\"OtherAgents\":{\"dictionary\":[]},\"Goals\":[{\"Name\":\"WinGame\",\"Significance\":5,\"Likelihood\":0.5}]},{\"KnowledgeBase\":{\"Perspective\":\"AI-EMOTIONAL-CONSTRUCTIVE-INDIVIDUALISTIC\",\"Knowledge\":{}},\"BodyName\":null,\"VoiceName\":null,\"EmotionalState\":{\"Mood\":0,\"initialTick\":0,\"EmotionalPool\":[],\"AppraisalConfiguration\":{\"HalfLifeDecayConstant\":0.5,\"EmotionInfluenceOnMoodFactor\":0.3,\"MoodInfluenceOnEmotionFactor\":0.3,\"MinimumMoodValueForInfluencingEmotions\":0.5,\"EmotionalHalfLifeDecayTime\":15,\"MoodHalfLifeDecayTime\":60}},\"AutobiographicMemory\":{\"Tick\":0,\"records\":[]},\"OtherAgents\":{\"dictionary\":[]},\"Goals\":[{\"Name\":\"WinGame\",\"Significance\":5,\"Likelihood\":0.5}]},{\"KnowledgeBase\":{\"Perspective\":\"AI-EMOTIONAL-DISRUPTIVE-COLLECTIVIST\",\"Knowledge\":{}},\"BodyName\":null,\"VoiceName\":null,\"EmotionalState\":{\"Mood\":0,\"initialTick\":0,\"EmotionalPool\":[],\"AppraisalConfiguration\":{\"HalfLifeDecayConstant\":0.5,\"EmotionInfluenceOnMoodFactor\":0.3,\"MoodInfluenceOnEmotionFactor\":0.3,\"MinimumMoodValueForInfluencingEmotions\":0.5,\"EmotionalHalfLifeDecayTime\":15,\"MoodHalfLifeDecayTime\":60}},\"AutobiographicMemory\":{\"Tick\":0,\"records\":[]},\"OtherAgents\":{\"dictionary\":[]},\"Goals\":[{\"Name\":\"WinGame\",\"Significance\":5,\"Likelihood\":0.5}]},{\"KnowledgeBase\":{\"Perspective\":\"AI-EMOTIONAL-DISRUPTIVE-INDIVIDUALISTIC\",\"Knowledge\":{}},\"BodyName\":null,\"VoiceName\":null,\"EmotionalState\":{\"Mood\":0,\"initialTick\":0,\"EmotionalPool\":[],\"AppraisalConfiguration\":{\"HalfLifeDecayConstant\":0.5,\"EmotionInfluenceOnMoodFactor\":0.3,\"MoodInfluenceOnEmotionFactor\":0.3,\"MinimumMoodValueForInfluencingEmotions\":0.5,\"EmotionalHalfLifeDecayTime\":15,\"MoodHalfLifeDecayTime\":60}},\"AutobiographicMemory\":{\"Tick\":0,\"records\":[]},\"OtherAgents\":{\"dictionary\":[]},\"Goals\":[{\"Name\":\"WinGame\",\"Significance\":5,\"Likelihood\":0.5}]}],\"WorldModel\":{\"Effects\":{\"dictionary\":[]},\"Priorities\":{\"dictionary\":[]}}},\"types\":[{\"TypeId\":0,\"ClassName\":\"IntegratedAuthoringTool.IntegratedAuthoringToolAsset, IntegratedAuthoringTool, Version=1.7.0.0, Culture=neutral, PublicKeyToken=null\"}]}", storage);
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
        SetParameterizationCondition(lastConditionString);
        GameGlobals.currGameCondition = lastConditionString;

        // @jbgrocha: auto start if on batchmode
        if (GameGlobals.isSimulation)
        {
            StartGame();
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
        if (!GameGlobals.isSimulation)
        {
            //GameGlobals.audioManager.PlayInfinitClip("Audio/theme/themeIntro", "Audio/theme/themeLoop");
            if (UIStartGameButton != null)
                UIStartGameButton.onClick.AddListener(delegate() { StartGame(); });
        }

        //thanks WebGL, because of you I've got to init a game global to init the rest of the game globals!
        if (GameGlobals.monoBehaviourFunctionalities == null)
        {
            GameObject monoBehaviourDummy = Instantiate(monoBehaviourDummyPrefab);
            DontDestroyOnLoad(monoBehaviourDummy);
            GameGlobals.monoBehaviourFunctionalities = monoBehaviourDummy.GetComponent<MonoBehaviourFunctionalities>();
        }
        GameGlobals.monoBehaviourFunctionalities.StartCoroutine(InitGameGlobals());
    }


    private void Start()
    {
        CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        // Make the game perform as good as possible
        if (!GameGlobals.isSimulation)
        {
            Application.targetFrameRate = 40;
            UIStartScreen = GameObject.Find("Canvas/StartScreen").transform;

            this.UIStartGameButton =
                GameObject.Find("Canvas/StartScreen/startGameButton").gameObject.GetComponent<Button>();
            this.UIStartGameButton.interactable = false;
        }

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
