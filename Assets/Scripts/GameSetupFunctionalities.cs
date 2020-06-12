using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class GameSetupFunctionalities : MonoBehaviour {

    private bool isErrorEncountered;
    
    public GameObject poppupPrefab;
    public GameObject playerCanvas;
    private PopupScreenFunctionalities playerWarningPoppupRef;
    private PopupScreenFunctionalities setupWarningPoppupRef;

    public void SetUpParameterization(GameParameterization parameterization)
    {

        if (!GameGlobals.isSimulation)
        {
            playerWarningPoppupRef = new PopupScreenFunctionalities(false, poppupPrefab, playerCanvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
            setupWarningPoppupRef = new PopupScreenFunctionalities(false, poppupPrefab, playerCanvas, GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"), new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
        }
        
        GameGlobals.players.Clear();

        GameGlobals.isNarrated = parameterization.isNarrated;
        // Init Narrator
        if (GameGlobals.isNarrated)
        {
            GameGlobals.narrator = new Narrator(playerCanvas);
        }

        int currPlayerId = 0;
        for (int i = 0; i < parameterization.playerParameterizations.Count; i++)
        {
            PlayerParameterization currParam = parameterization.playerParameterizations[i];

            InteractionModule chosenIM = new AutisticInteractionModule();
            switch (currParam.interactionType)
            {
                case "NONE":
                    //already defined on top
                    break;
                case "BALOON":
                    chosenIM = new SpeechBaloonInteractionModule();
                    break;

                default:
                    isErrorEncountered = true;
                    setupWarningPoppupRef.DisplayPoppup("Error on parsing the interaction module named " + currParam.interactionType);
                    break;

            }
            
            switch (currParam.playerType)
            {
                case "HUMAN":
                    GameGlobals.players.Add(new Player(currParam.playerType, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/"+ currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-RANDOM":
                    GameGlobals.players.Add(new AIPlayerRandom(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-COOPERATOR":
                    GameGlobals.players.Add(new AIPlayerCooperator(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-BALANCED-COOPERATOR":
                    GameGlobals.players.Add(new AIPlayerBalancedCooperator(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-TIT-FOR-TAT":
                    GameGlobals.players.Add(new AIPlayerTitForTat(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-COMPENSATOR":
                    GameGlobals.players.Add(new AIPlayerCompensator(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-ULTRA-FAIR":
                    GameGlobals.players.Add(new AIPlayerUltraFair(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-DEFECTOR":
                    GameGlobals.players.Add(new AIPlayerDefector(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                case "AI-BALANCED-DEFECTOR":
                    GameGlobals.players.Add(new AIPlayerBalancedDefector(currParam.playerType, chosenIM, playerCanvas, playerWarningPoppupRef, Resources.Load<Sprite>("Textures/UI/Icons/" + currParam.spriteIndex), currPlayerId++, currParam.name));
                    break;

                default:
                    isErrorEncountered = true;
                    string errorStr = "Error on parsing the player type of " + currParam.name;
                    if (!GameGlobals.isSimulation)
                    {
                        setupWarningPoppupRef.DisplayPoppup(errorStr);
                    }
                    else
                    {
                        Debug.Log(errorStr);
                    }
                    break;
            }
        }

        GameGlobals.diceLogic = new RandomDiceLogic();
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
        switch (parameterization.diceLogic)
        {
            case "RANDOM":
                GameGlobals.diceLogic = new RandomDiceLogic();
                break;
            default:
                isErrorEncountered = true;
                setupWarningPoppupRef.DisplayPoppup("Error on parsing the dice logic of parameterization " + parameterization.diceLogic);
                break;
        }
        //}
    }

    void Start ()
    {
        isErrorEncountered = false;
        if (!GameGlobals.isSimulation)
        {
            playerWarningPoppupRef = new PopupScreenFunctionalities(true, null, null, poppupPrefab, playerCanvas,
                GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"),
                new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
            setupWarningPoppupRef = new PopupScreenFunctionalities(true, null, null, poppupPrefab, playerCanvas,
                GameGlobals.monoBehaviourFunctionalities, Resources.Load<Sprite>("Textures/UI/Icons/Info"),
                new Color(0.9f, 0.9f, 0.9f), "Audio/snap");
            Object.DontDestroyOnLoad(playerCanvas);
        }

        //auto fetch config
        List<GameParameterization> gameParameterizations = GameProperties.currSessionParameterization.gameParameterizations;
        GameProperties.currGameParameterization = gameParameterizations[(GameGlobals.currGameId - 1) % gameParameterizations.Count];
        StartGame();
    }
	
	void StartGame()
    {
        SetUpParameterization(GameProperties.currGameParameterization);

        if (isErrorEncountered)
        {
            return;
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
//        UINameSelectionInputBox.text = "";
        if (param.playerParameterizations.Count == GameGlobals.players.Count)
        {
            GameProperties.currGameParameterization = param;

//            UIStartGameButton.gameObject.SetActive(true);
//            UIAddPlayerButton.gameObject.SetActive(false);
//            UINameSelectionInputBox.gameObject.SetActive(false);
//
//            UIAIPlayerSelectionButtonsObject.SetActive(false);
//            configSelectionButtonsObject.SetActive(false);
//            UIResetButton.gameObject.SetActive(false);
        }
    }
    
}
