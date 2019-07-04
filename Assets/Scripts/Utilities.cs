using IntegratedAuthoringTool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GameGlobals
{
    public static MonoBehaviourFunctionalities monoBehaviourFunctionalities;

    public static List<Player> players;

    // Narrator
    public static Narrator narrator;
    public static bool isNarrated;

    public static string currSessionId;
    public static int currGameId;
    public static int currGameRoundId;


    public static GameProperties.GameState currGameState;

    public static int numberOfSpeakingPlayers;

    public static IDiceLogic diceLogic;

    public static LogManager gameLogManager;
    public static AudioManager audioManager;
    public static GameManager gameManager;

    public static GameSceneManager gameSceneManager;


    public static float commonEnvironmentInvestment;

    //fatima stuff
    public static string FAtiMAScenarioPath;
    public static IntegratedAuthoringToolAsset FAtiMAIat;

    // @jbgrocha: Auto play if batchmode (or explicitly set it to true);
    public static bool isSimulation;

    public static int roundBudget;
}

public static class GameProperties
{
    public enum GameState
    {
        NOT_FINISHED,
        VICTORY, //reached the end of the maximum rounds
        LOSS //catastrophic
    }

    public enum InvestmentTarget
    {
        ENVIRONMENT,
        ECONOMIC
    }

    public enum PlayerType
    {
        AIPLAYER_NOT_ASSIGNED,
        HUMAN,
        SIMPLE,
        RANDOM,
        COOPERATIVE,
        GREEDY,
        BALANCED,
        UNBALANCED,
        TITFORTAT
    }

    public enum NGType
    {
        RANDOM,
        VICTORY,
        LOSS
    }

    public static GameParameterization currGameParameterization; //assigned automatically when isAutomaticDebriefing or isSimulation is assigned
    public static SessionParameterization currSessionParameterization; //assigned automatically when isAutomaticDebriefing or isSimulation is assigned
    
    public static DynamicallyConfigurableGameProperties configurableProperties;
}

//configurations classes

[Serializable]
public class DynamicallyConfigurableGameProperties
{ 
    //(default configurations already assigned)

    public int numberOfPlayersPerGame = 3;


    //----------- AutomaticBriefing -------------------
    public bool isAutomaticBriefing = false;
    public int numSessionGames = 0; //no tutorials

    //------------ Simulation --------------------
    public bool isSimulation = false;
    public int numGamesToSimulate = 1;

    public List<SessionParameterization> possibleParameterizations = new List<SessionParameterization>(); //only used when generating the AI types automatically (for example when "isSimulation=true or isAutomaticBriefing==true")

    public int roundBudget = 5;
}

[Serializable]
public struct SessionParameterization
{
    public string id;
    public List<GameParameterization> gameParameterizations;

    public SessionParameterization(string id)
    {
        this.id = id;
        this.gameParameterizations = new List<GameParameterization>();
    }

    public SessionParameterization(string id, List<GameParameterization> gameParameterizations)
    {
        this.id = id;
        this.gameParameterizations = gameParameterizations;
    }

}


[Serializable]
public struct GameParameterization
{
    public List<PlayerParameterization> playerParameterizations;
    public string ngType;
    public bool isNarrated;

    public GameParameterization(string id, List<PlayerParameterization> playerParameterizations, string ngType, bool isNarrated)
    {
        this.playerParameterizations = playerParameterizations;
        this.ngType = ngType;
        this.isNarrated = isNarrated;
    }

}

[Serializable]
public struct PlayerParameterization
{
    public string name;
    public string playerType;
    public string interactionType;
    public bool isSpeechAllowed;

    //used when the the game ngType is not RANDOM
    public int[] fixedEnvironmentDiceResults; 
    public int[] fixedEconomicDiceResults;
    
    public int spriteIndex;

    public PlayerParameterization(string name, string playerType, string interactionType, bool isSpeechAllowed, int[] fixedEnvironmentDiceResults, int[] fixedEconomicDiceResults, int spriteIndex)
    {
        this.name = name;
        this.isSpeechAllowed = isSpeechAllowed;
        this.playerType = playerType;
        this.interactionType = interactionType;

        this.fixedEnvironmentDiceResults = fixedEnvironmentDiceResults;
        this.fixedEconomicDiceResults = fixedEconomicDiceResults;

        this.spriteIndex = spriteIndex;
    }
    public PlayerParameterization(string name, string playerType, string interactionType, bool isSpeechAllowed, int[] fixedInstrumentDiceResults, int[] fixedMarketingDiceResults) : this(name, playerType, interactionType, isSpeechAllowed, fixedInstrumentDiceResults, fixedMarketingDiceResults, 0) { }
    public PlayerParameterization(string name, string playerType, string interactionType, bool isSpeechAllowed) : this(name, playerType, interactionType, isSpeechAllowed, new int[] { }, new int[] { }) { }
    public PlayerParameterization(string name, string playerType, string interactionType) : this(name, playerType, interactionType, false) { }
}