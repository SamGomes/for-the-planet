﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupFunctionalities : MonoBehaviour {

    private bool isErrorEncountered;
    private GameObject customizeLabel;

    private InputField UINameSelectionInputBox;
    private Button UIStartGameButton;
    private Button UIAddPlayerButton;
    private Button UIResetButton;

    private GameObject UIAIPlayerSelectionButtonsObject;
    private GameObject configSelectionButtonsObject;

    
    public GameObject poppupPrefab;
    public GameObject playerUIPrefab;
    public GameObject playerCanvas;
    private PopupScreenFunctionalities playerWarningPoppupRef;
    private PopupScreenFunctionalities setupWarningPoppupRef;

    public void SetUpParameterization(GameParameterization parameterization)
    {
        GameGlobals.players.Clear();


        // Init Narrator
        GameGlobals.narrator = new Narrator(playerCanvas);


        int currPlayerId = 0;
        for (int i = 0; i < parameterization.playerParameterizations.Count; i++)
        {
            PlayerParameterization currParam = parameterization.playerParameterizations[i];

            
            switch (currParam.playerType)
            {
                case "HUMAN":
                    GameGlobals.players.Add(new Player(playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/"+ currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI_EMOTIONAL":
                    GameGlobals.players.Add(new EmotionalAIPlayer(new RobotInteractionModule(), playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name, 1.0f));
                    break;

                case "AI_COOPERATOR":
                    GameGlobals.players.Add(new AIPlayerCooperator(new RobotInteractionModule(), playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI_BALANCED_COOPERATOR":
                    GameGlobals.players.Add(new AIPlayerBalancedCooperator(new LegendsInteractionModule(), playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI_DEFECTOR":
                    GameGlobals.players.Add(new AIPlayerDefector(new LegendsInteractionModule(), playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI_BALANCED_DEFECTOR":
                    GameGlobals.players.Add(new AIPlayerBalancedDefector(new LegendsInteractionModule(), playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                default:
                    isErrorEncountered = true;
                    setupWarningPoppupRef.DisplayPoppup("Error on parsing the player type of " + currParam.name);
                    Debug.Log("[ERROR]: Error on parsing the player type of " + currParam.name);
                    break;
            }
        }

        //string pattern = "FIXED:[WL]+";
        //Match m = Regex.Match(parameterization.ngType, pattern, RegexOptions.IgnoreCase);
        //if (m.Success)
        //{
        //    int index = parameterization.ngType.IndexOf(":");
        //    string gameResults = parameterization.ngType.Substring(index + 1);
        //    GameGlobals.gameDiceNG = new PredefinedDiceNG();
        //}
        //else
        //{
        //    switch (parameterization.ngType)
        //    {
        //        case "RANDOM":
        //            GameGlobals.gameDiceNG = new RandomDiceNG();
        //            break;
        //        case "FIXED:LOSS":
        //            GameGlobals.gameDiceNG = new LossDiceNG();
        //            break;
        //        case "FIXED:VICTORY":
        //            GameGlobals.gameDiceNG = new VictoryDiceNG();
        //            break;
        //        default:
        //            isErrorEncountered = true;
        //            setupWarningPoppupRef.DisplayPoppup("Error on parsing the NG Type of parameterization " + parameterization.ngType);
        //            Debug.Log("[ERROR]: Error on parsing the NG Type of parameterization " + parameterization.ngType);
        //            break;
        //    }
        //}
    }

    void Start ()
    {
        isErrorEncountered = false;
        playerWarningPoppupRef = new PopupScreenFunctionalities(true, null, null, poppupPrefab, playerCanvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
        setupWarningPoppupRef = new PopupScreenFunctionalities(true, null, null, poppupPrefab, playerCanvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
        Object.DontDestroyOnLoad(playerCanvas);

       
        if (!GameProperties.configurableProperties.isSimulation && !GameProperties.configurableProperties.isAutomaticBriefing)
        {
            GameParameterization manualGameParam = new GameParameterization();
            manualGameParam.playerParameterizations = new List<PlayerParameterization>();


            this.customizeLabel = GameObject.Find("Canvas/SetupScreen/customizeLabel").gameObject;

            this.UIResetButton = GameObject.Find("Canvas/SetupScreen/resetButton").gameObject.GetComponent<Button>();
            this.UINameSelectionInputBox = GameObject.Find("Canvas/SetupScreen/nameSelectionInputBox").gameObject.GetComponent<InputField>();
            this.UIStartGameButton = GameObject.Find("Canvas/SetupScreen/startGameButton").gameObject.GetComponent<Button>();
            this.UIAddPlayerButton = GameObject.Find("Canvas/SetupScreen/addPlayerGameButton").gameObject.GetComponent<Button>();

            this.UIAIPlayerSelectionButtonsObject = GameObject.Find("Canvas/SetupScreen/addAIPlayerGameButtons").gameObject;
            Button[] UIAIPlayerSelectionButtons= UIAIPlayerSelectionButtonsObject.GetComponentsInChildren<Button>();

            this.configSelectionButtonsObject = GameObject.Find("Canvas/SetupScreen/configButtons").gameObject;
            Button[] UIConfigButtons = configSelectionButtonsObject.GetComponentsInChildren<Button>();

            UIResetButton.onClick.AddListener(delegate {
                GameGlobals.players.Clear();
                foreach (Button button in UIAIPlayerSelectionButtons)
                {
                    button.interactable = true;
                }
            });


            UIStartGameButton.gameObject.SetActive(false);

            UIStartGameButton.onClick.AddListener(delegate { StartGame(); });
            UIAddPlayerButton.onClick.AddListener(delegate {
                manualGameParam.playerParameterizations.Add(new PlayerParameterization("Sam", "HUMAN"));
                CheckForAllPlayersRegistered(manualGameParam);
            });


            manualGameParam.ngType = "RANDOM";
            for (int i=0; i < UIAIPlayerSelectionButtons.Length; i++)
            {
                GameGlobals.numberOfSpeakingPlayers++;
                Button button = UIAIPlayerSelectionButtons[i];
                button.onClick.AddListener(delegate
                {
                    int index = new List<Button>(UIAIPlayerSelectionButtons).IndexOf(button);
                    switch ((GameProperties.PlayerType) (index+4))
                    {
                        //case GameProperties.PlayerType.SIMPLE:
                        //    manualGameParam.playerParameterizations.Add(new PlayerParameterization("Sam", "SIMPLE", false));
                        //    break;
                        case GameProperties.PlayerType.COOPERATIVE:
                            manualGameParam.playerParameterizations.Add(new PlayerParameterization("Cristoph", "COOPERATIVE", false));
                            break;
                        case GameProperties.PlayerType.GREEDY:
                            manualGameParam.playerParameterizations.Add(new PlayerParameterization("Giovanni", "GREEDY", false));
                            break;
                        case GameProperties.PlayerType.BALANCED:
                            manualGameParam.playerParameterizations.Add(new PlayerParameterization("Brian", "BALANCED", false));
                            break;
                        case GameProperties.PlayerType.UNBALANCED:
                            manualGameParam.playerParameterizations.Add(new PlayerParameterization("Ulrich", "UNBALANCED", false));
                            break;
                        case GameProperties.PlayerType.TITFORTAT:
                            manualGameParam.playerParameterizations.Add(new PlayerParameterization("Tim", "TITFORTAT", false));
                            break;
                    }
                    CheckForAllPlayersRegistered(manualGameParam);
                });
            }

            for (int i = 0; i < UIConfigButtons.Length; i++)
            {
                Button button = UIConfigButtons[i];
                button.onClick.AddListener(delegate
                {
                    if (button.gameObject.name.EndsWith("1"))
                    {
                        manualGameParam.playerParameterizations.Add(new PlayerParameterization("Player", "HUMAN"));
                        manualGameParam.playerParameterizations.Add(new PlayerParameterization("Player", "HUMAN"));
                        manualGameParam.playerParameterizations.Add(new PlayerParameterization("Player", "HUMAN"));
                        manualGameParam.ngType = "RANDOM";
                    }
                    else if (button.gameObject.name.EndsWith("2"))
                    {
                        manualGameParam.playerParameterizations.Add(new PlayerParameterization("Player", "COOPERATIVE", false));
                        manualGameParam.playerParameterizations.Add(new PlayerParameterization("Player", "GREEDY", false));
                        manualGameParam.playerParameterizations.Add(new PlayerParameterization("Player", "HUMAN"));
                        manualGameParam.ngType = "RANDOM";
                    }
                    button.interactable = false;
                    CheckForAllPlayersRegistered(manualGameParam);
                });
            }

        }
        else
        {
            //auto fetch config
            List<GameParameterization> gameParameterizations = GameProperties.currSessionParameterization.gameParameterizations;
            GameProperties.currGameParameterization = gameParameterizations[(GameGlobals.currGameId - 1) % gameParameterizations.Count];
            StartGame();
        }
    }
	
	void StartGame()
    {
        SetUpParameterization(GameProperties.currGameParameterization);

        if (isErrorEncountered)
        {
            return;
        }

        //write players in log before starting the game
        Player currPlayer = null;
        for (int i = 0; i < GameProperties.configurableProperties.numberOfPlayersPerGame; i++)
        {
            currPlayer = GameGlobals.players[i];
            StartCoroutine(GameGlobals.gameLogManager.WritePlayerToLog(GameGlobals.currSessionId.ToString(), GameGlobals.currGameId.ToString(), currPlayer.GetId().ToString(), currPlayer.GetName(), "-"));
        }

        string json = JsonUtility.ToJson(GameProperties.configurableProperties);

        //this init is not nice
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button button in allButtons)
        {
            button.onClick.AddListener(delegate () {
                GameGlobals.audioManager.PlayClip("Audio/snap");
            });
        }

        GameGlobals.gameSceneManager.LoadMainScene();
    }

    void CheckForAllPlayersRegistered(GameParameterization param)
    {
        UINameSelectionInputBox.text = "";
        if (param.playerParameterizations.Count == GameProperties.configurableProperties.numberOfPlayersPerGame)
        {
            GameProperties.currGameParameterization = param;

            UIStartGameButton.gameObject.SetActive(true);
            customizeLabel.gameObject.SetActive(false);
            UIAddPlayerButton.gameObject.SetActive(false);
            UINameSelectionInputBox.gameObject.SetActive(false);

            UIAIPlayerSelectionButtonsObject.SetActive(false);
            configSelectionButtonsObject.SetActive(false);
            UIResetButton.gameObject.SetActive(false);
        }
    }
    
}