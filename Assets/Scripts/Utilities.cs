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
    public static string currGameCondition;

    public static GameProperties.GameState currGameState;

    public static int numberOfSpeakingPlayers;

    public static IDiceLogic diceLogic;

    public static LogManager gameLogManager;
    public static AudioManager audioManager;
    public static GameManager gameManager;
    public static MongoAtlasLogManager mongoAtlasLogManager;

    public static GameSceneManager gameSceneManager;


    public static float envState;
    public static float envThreshold;
    public static double envRenew;
    public static float envRenewperRound;
    public static int generation;
    public static Boolean skipTutorial;
    public static Boolean firstGeneration;
    public static List<int> envStatePerRound;
    public static float diffCP = 0;

    //fatima stuff
    public static string FAtiMAScenarioPath;

    // @jbgrocha: Auto play if batchmode (or explicitly set it to true);
    public static bool isSimulation;

    public static int roundBudget;
    public static int[] environmentDecayBudget;
    public static int[] playerDecayBudget;


    // Intergenerational Goods Game stuff
    public static string participantName;
}

public static class GameProperties
{
    public enum GameState
    {
        NOT_FINISHED,
        VICTORY, //reached the end of the maximum rounds
        LOSS //catastrophic
    }
    public enum GamePhase
    {
        BUDGET_ALLOCATION,
        HISTORY_DISPLAY,
        BUDGET_EXECUTION,
        INVESTMENT_SIMULATION
    }

    public enum InvestmentTarget
    {
        ENVIRONMENT,
        ECONOMIC
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

    //------------ Simulation --------------------
    public bool isSimulation = false;
    public int numGamesToPlay = 1;

    public string logManagerStyle;
    public string mongoConnector;

    public List<SessionParameterization> possibleParameterizations = new List<SessionParameterization>(); //only used when generating the AI types automatically (for example when "isSimulation=true or isAutomaticBriefing==true")

    public int roundBudget = 20; //Change in IGG to 2x Fair
    public int[] environmentDecayBudget = new int[]{5,5};
    public int[] playerDecayBudget = new int[]{5,5};
    public int maxNumRounds = 10; //10 generations
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
    public string diceLogic;
    public bool isNarrated;

    public GameParameterization(string id, List<PlayerParameterization> playerParameterizations, string diceLogic, bool isNarrated)
    {
        this.playerParameterizations = playerParameterizations;
        this.diceLogic = diceLogic;
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

    public string fatimaRpcPath;

    public PlayerParameterization(string name, string playerType, string interactionType, bool isSpeechAllowed, int[] fixedEnvironmentDiceResults, int[] fixedEconomicDiceResults, int spriteIndex, string fatimaRpcPath)
    {
        this.name = name;
        this.isSpeechAllowed = isSpeechAllowed;
        this.playerType = playerType;
        this.interactionType = interactionType;

        this.fixedEnvironmentDiceResults = fixedEnvironmentDiceResults;
        this.fixedEconomicDiceResults = fixedEconomicDiceResults;

        this.spriteIndex = spriteIndex;

        this.fatimaRpcPath = fatimaRpcPath;
    }
    public PlayerParameterization(string name, string playerType, string interactionType, bool isSpeechAllowed, int[] fixedInstrumentDiceResults, int[] fixedMarketingDiceResults, string fatimaRpcPath) : this(name, playerType, interactionType, isSpeechAllowed, fixedInstrumentDiceResults, fixedMarketingDiceResults, 0, fatimaRpcPath) { }
    public PlayerParameterization(string name, string playerType, string interactionType, bool isSpeechAllowed) : this(name, playerType, interactionType, isSpeechAllowed, new int[] { }, new int[] { }, "") { }
    public PlayerParameterization(string name, string playerType, string interactionType) : this(name, playerType, interactionType, false) { }
}