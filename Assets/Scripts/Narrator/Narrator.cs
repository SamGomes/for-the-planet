using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Does this need a game manager reference like the player?
public class Narrator
{
    public List<NarrativeInterpretation> narrativeInterpretations;
    public List<NarrativeFragment> narrativeFragments;

    private GameObject NarratorUI;
    private GameObject CanvasUI;

    private LegendsInteractionModule InteractionModule;


    public Narrator(GameObject narratorCanvas)
    {
        narrativeInterpretations = new List<NarrativeInterpretation>();
        narrativeFragments = new List<NarrativeFragment>();

        CanvasUI = narratorCanvas;
        NarratorUI = Resources.Load<GameObject>("Prefabs/NarratorUI");

        InteractionModule = new LegendsInteractionModule();
        InteractionModule.Init(NarratorUI, CanvasUI);
    }

    public void InitNarrativeFragments()
    {
        InitGameStartNarrativeFragment();

        InitRoundStartNarrativeFragments();

        InitEnvironmentInvestmentNarrativeFragments();
        InitEconomyInvestmentNarrativeFragments();

        InitEnvironmentDecayNarrativeFragments();
        InitEconomyDecayNarrativeFragments();
    }

    public void InitGameStartNarrativeFragment()
    {
        //  The number of players should be fetch
        string text = GameGlobals.players.Count + " neighbouring countries are attempting to develop their economies. " +
            "Unfortunately, a lot of damage has already been done to the environment, and they need to make sure that the planet survives their endeavours.";
        NarrativeFragment gameStart = new NarrativeFragment
        {
            Type = "GAME_START",
            Action = text
        };
        narrativeFragments.Add(gameStart);
    }

    public void InitRoundStartNarrativeFragments()
    {
        // To be Implemented
    }

    // Placeholders
    public void InitEnvironmentInvestmentNarrativeFragments()
    {
        AddEnvironmentInvestmentNarrativeFragment("ENVIRONMENT_INVEST_ACTION",
            "ENVIRONMENT_INVEST_OUTCOME_LOW", "ENVIRONMENT_INVEST_OUTCOME_MEDIUM", "ENVIRONMENT_INVEST_OUTCOME_HIGH");
    }

    public void AddEnvironmentInvestmentNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh)
    {
        NarrativeFragment narrative = new NarrativeFragment
        {
            Type = "ENVIRONMENT_INVESTMENT",
            Action = action,
            Outcome = new Dictionary<string, string>()
        };

        narrative.Outcome["LOW"] = outcomeLow;
        narrative.Outcome["MEDIUM"] = outcomeMedium;
        narrative.Outcome["HIGH"] = outcomeHigh;

        narrativeFragments.Add(narrative);
    }

    public void InitEconomyInvestmentNarrativeFragments()
    {

        AddEconomyInvestmentNarrativeFragment("ECONOMY_INVEST_ACTION",
            "ECONOMY_INVEST_OUTCOME_LOW", "ECONOMY_INVEST_OUTCOME_MEDIUM", "ECONOMY_INVEST_OUTCOME_HIGH");

        
    }

    public void AddEconomyInvestmentNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh)
    {
        NarrativeFragment narrative = new NarrativeFragment
        {
            Type = "ECONOMY_INVESTMENT",
            Action = action,
            Outcome = new Dictionary<string, string>()
        };

        narrative.Outcome["LOW"] = outcomeLow;
        narrative.Outcome["MEDIUM"] = outcomeMedium;
        narrative.Outcome["HIGH"] = outcomeHigh;

        narrativeFragments.Add(narrative);
    }


    // Placeholders
    public void InitEnvironmentDecayNarrativeFragments()
    {
        AddEnvironmentDecayNarrativeFragment("ENVIRONMENT_DECAY_ACTION", 
            "ENVIRONMENT_DECAY_OUTCOME_LOW", "ENVIRONMENT_DECAY_OUTCOME_MEDIUM", "ENVIRONMENT_DECAY_OUTCOME_HIGH");
    }

    public void AddEnvironmentDecayNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh)
    {
        NarrativeFragment narrative = new NarrativeFragment
        {
            Type = "ENVIRONMENT_DECAY",
            Action = action,
            Outcome = new Dictionary<string, string>()
        };

        narrative.Outcome["LOW"] = outcomeLow;
        narrative.Outcome["MEDIUM"] = outcomeMedium;
        narrative.Outcome["HIGH"] = outcomeHigh;

        narrativeFragments.Add(narrative);
    }

    public void InitEconomyDecayNarrativeFragments()
    {
        AddEconomyDecayNarrativeFragment("ECONOMY_DECAY_ACTION", 
            "ECONOMY_DECAY_OUTCOME_LOW", "ECONOMY_DECAY_OUTCOME_MEDIUM", "ECONOMY_DECAY_OUTCOME_HIGH");
    }

    public void AddEconomyDecayNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh)
    {
        NarrativeFragment narrative = new NarrativeFragment
        {
            Type = "ECONOMY_DECAY",
            Action = action,
            Outcome = new Dictionary<string, string>()
        };

        narrative.Outcome["LOW"] = outcomeLow;
        narrative.Outcome["MEDIUM"] = outcomeMedium;
        narrative.Outcome["HIGH"] = outcomeHigh;

        narrativeFragments.Add(narrative);
    }

    public NarrativeFragment GetGameStartNarrativeFragment()
    {
        return narrativeFragments.Find(x => x.Type == "GAME_START");
    }

    public List<NarrativeFragment> GetRoundStartNarrativeFragments()
    {
        return narrativeFragments.FindAll(x => x.Type == "ROUND_START");
    }

    public List<NarrativeFragment> GetEnvironmentInvestmentNarrativeFragments()
    {
        return narrativeFragments.FindAll(x => x.Type == "ENVIRONMENT_INVESTMENT");
    }

    public List<NarrativeFragment> GetEconomyInvestmentNarrativeFragments()
    {
        return narrativeFragments.FindAll(x => x.Type == "ECONOMY_INVESTMENT");
    }

    public List<NarrativeFragment> GetEnvironmentDecayNarrativeFragments()
    {
        return narrativeFragments.FindAll(x => x.Type == "ENVIRONMENT_DECAY");
    }

    public List<NarrativeFragment> GetEconomyDecayNarrativeFragments()
    {
        return narrativeFragments.FindAll(x => x.Type == "ECONOMY_DECAY");
    }

    public List<NarrativeFragment> GetNarrativeFragments()
    {
        return narrativeFragments;
    }


    // Narrative Interpretations
    // Need to validate What I will actually need, trim this as required (over-engineered implementation)
    public List<NarrativeInterpretation> getNarrativeInterpretations()
    {
        return narrativeInterpretations;
    }

    public List<NarrativeInterpretation> GetNarrativeInterpretations(Player player)
    {
        return narrativeInterpretations.FindAll(x => x.Player.GetId() == player.GetId()); ;
    }

    public List<NarrativeInterpretation> GetNarrativeInterpretations(Player player, int round)
    {
        return narrativeInterpretations.FindAll(x => x.Player.GetId() == player.GetId()).FindAll(x => x.Round == round);
    }

    public NarrativeInterpretation GetNarrativeInterpretation(Player player, int round, string type)
    {
        return narrativeInterpretations.FindAll(x => x.Player.GetId() == player.GetId()).FindAll(x => x.Round == round).Find(x => x.Type == type);
    }

    public List<NarrativeInterpretation> GetNarrativeInterpretations(int round, string type)
    {
        return narrativeInterpretations.FindAll(x => x.Type == type).FindAll(x => x.Round == round);
    }

    // Narrator Actions during Game Start
    // should give game setting context
    public IEnumerator GameStart()
    {
        // Sorta Delayed Initialization of the Narrative Fragments
        InitNarrativeFragments();

        NarrativeFragment gameStart = GetGameStartNarrativeFragment();

        // Add NarrativeInterpretation?
        // Does this make sense at game start?

        // Compute Narrator Text
        string text = gameStart.Action;

        // Output Narrator Text
        InteractionModule.Speak(text);

        yield return null;
    }

    // Narrator Actions during Round Start
    // should give game status context
    public IEnumerator RoundStart()
    {
        // Compute Narrator Text
        string text = "Narrator: Round Start";

        // Output Narrator Text
        InteractionModule.Speak(text);

        yield return null;
    }

    // Narrator Actions during Budget Allocation
    // can potentially receive a player and the budget allocated
    public IEnumerator BudgetAllocation(Player player, int round)
    {
        Dictionary<GameProperties.InvestmentTarget, int> investment = player.GetCurrRoundInvestment();

        int economy = investment[GameProperties.InvestmentTarget.ECONOMIC];
        int environment = investment[GameProperties.InvestmentTarget.ENVIRONMENT];

        if(environment != 0)
        {
            // Create NarrativeInterpretation for Environment Investment
            NarrativeInterpretation environmentInvestmentNarrativeInterpretation = new NarrativeInterpretation
            {
                Player = player,
                Round = round,
                Value = environment,
                Type = "ENVIRONMENT_INVESTMENT"
            };

            // Register NarrativeInterpretation for Environment Investment
            narrativeInterpretations.Add(environmentInvestmentNarrativeInterpretation);

            // Economy Investment Fragment
            // Compute Narrator Text (symbol interpretation)
            // Should fetch a random
            List<NarrativeFragment> environmentInvestmentFragments = GetEnvironmentInvestmentNarrativeFragments();
            NarrativeFragment environmentInvestmentFragment = environmentInvestmentFragments.FirstOrDefault();

            environmentInvestmentNarrativeInterpretation.Narrative = environmentInvestmentFragment;
        }

        if(economy != 0)
        {
            // Create NarrativeInterpretation for Economy Investment
            NarrativeInterpretation economyInvestmentNarrativeInterpretation = new NarrativeInterpretation
            {
                Player = player,
                Round = round,
                Value = economy,
                Type = "ECONOMY_INVESTMENT"
            };

            // Register NarrativeInterpretation for Economy Investment
            narrativeInterpretations.Add(economyInvestmentNarrativeInterpretation);

            // Economy Investment Fragment
            // Compute Narrator Text (symbol interpretation)
            // Should fetch a random
            List<NarrativeFragment> economyInvestmentFragments = GetEconomyInvestmentNarrativeFragments();
            NarrativeFragment economyInvestmentFragment = economyInvestmentFragments.FirstOrDefault();

            economyInvestmentNarrativeInterpretation.Narrative = economyInvestmentFragment;
        }

        yield return null;
    }

    // Narrator Actions during Display History
    public IEnumerator DisplayHistory(Player player, int round)
    {
        string text = "Narrator: Display History\n";

        GetNarrativeInterpretations(player, round).ForEach(delegate (NarrativeInterpretation interpretation)
        {
            //text += interpretation.ToString() + "\n";
            text += player.GetName() + " : " + interpretation.Narrative.Action + "\n";
        });

        // Output Narrator Text
        InteractionModule.Speak(text);

        yield return null;
    }

    // Narrator Actions during Economy Budget Execution
    public IEnumerator EconomyBudgetExecution(Player player, int round, int economyResult)
    {
        NarrativeInterpretation narrativeInterpretation = GetNarrativeInterpretation(player, round, "ECONOMY_INVESTMENT");

        if(narrativeInterpretation != null)
        {
            narrativeInterpretation.Result = economyResult;

            // Compute Narrator Text (symbol interpretation)
            string text = player.GetName() + " : " + narrativeInterpretation.Outcome();

            // Output Narrator Text
            if (economyResult != 0)
            {
                InteractionModule.Speak(text);
            }
        }
        

        yield return null;
    }

    // Narrator Actions during Environment Budget Execution
    public IEnumerator EnvironmentBudgetExecution(Player player, int round, int environmentResult)
    {
        NarrativeInterpretation narrativeInterpretation = GetNarrativeInterpretation(player, round, "ENVIRONMENT_INVESTMENT");

        if (narrativeInterpretation != null)
        {
            narrativeInterpretation.Result = environmentResult;

            // Compute Narrator Text (symbol interpretation)
            string text = player.GetName() + " : " + narrativeInterpretation.Outcome();

            // Output Narrator Text
            if (environmentResult != 0)
            {
                InteractionModule.Speak(text);
            }
        }
        

        yield return null;
    }

    // Narrator Actions during Economy Decay Simulation
    public IEnumerator EconomyDecaySimulation(Player player, int round, int decay)
    {
        NarrativeInterpretation economyDecayNarrativeInterpretation = new NarrativeInterpretation
        {
            Player = player,
            Round = round,
            Value = 2,
            Result = decay,
            Type = "ECONOMY_DECAY"
        };

        // Register NarrativeInterpretation for Economy Decay
        narrativeInterpretations.Add(economyDecayNarrativeInterpretation);

        // Compute Narrator Text (symbol interpretation)
        // Should fetch a random
        List<NarrativeFragment> economyDecayFragments = GetEconomyDecayNarrativeFragments();
        NarrativeFragment economyDecayFragment = economyDecayFragments.FirstOrDefault();

        economyDecayNarrativeInterpretation.Narrative = economyDecayFragment;

        // Compute Narrator Text
        string text = player.GetName() + " : " + economyDecayNarrativeInterpretation.Outcome();


        // Output Narrator Text
        if (decay != 0)
        {
            InteractionModule.Speak(text);
        }

        yield return null;
    }

    // Narrator Actions during Environment Decay Simulation
    public IEnumerator EnvironmentDecaySimulation(int round, int decay)
    {
        NarrativeInterpretation environmentDecayNarrativeInterpretation = new NarrativeInterpretation
        {
            Round = round,
            Value = 2,
            Result = decay,
            Type = "ENVIRONMENT_DECAY"
        };

        // Register NarrativeInterpretation for Economy Decay
        narrativeInterpretations.Add(environmentDecayNarrativeInterpretation);

        // Compute Narrator Text (symbol interpretation)
        // Should fetch a random
        List<NarrativeFragment> environmentDecayFragments = GetEnvironmentDecayNarrativeFragments();
        NarrativeFragment environmentDecayFragment = environmentDecayFragments.FirstOrDefault();

        environmentDecayNarrativeInterpretation.Narrative = environmentDecayFragment;

        // Compute Narrator Text
        string text = environmentDecayNarrativeInterpretation.Outcome();

        // Output Narrator Text
        if (decay != 0)
        {
            InteractionModule.Speak(text);
        }

        yield return null;
    }

    // Narrator Actions during Game End
    public IEnumerator GameEnd()
    {
        // Compute Narrator Text
        string text = "Narrator: Game End";

        // Output Narrator Text
        InteractionModule.Speak(text);

        yield return null;
    }

    // Narrator Text Bubble
    // Don't Think I need this
    private IEnumerator DisplayNarratorText(string message, float delay)
    {
        yield return null;
    }

}

public class NarrativeInterpretation
{
    public Player Player { get; set; }
    public int Round { get; set; }

    public string Type { get; set; }

    public int? Value { get; set; }
    public int? Result { get; set; }

    public NarrativeFragment Narrative { get; set; }

    public override string ToString()
    {
        string result = "Round " + Round + ": "
            + Type + " - " + Value + " (" + Result + ") ";

        if (Player != null)
        {
            result = Player.GetName() + " Round " + Round + ": "
            + Type + " - " + Value + " (" + Result + ") ";
        }

        return result;
    }

    public string Outcome()
    {
        // Assuming 6 sided dice
        Dictionary<int, string> resultClassification = new Dictionary<int, string>();
        resultClassification[1] = "LOW";
        resultClassification[2] = "LOW";
        resultClassification[3] = "MEDIUM";
        resultClassification[4] = "MEDIUM";
        resultClassification[5] = "HIGH";
        resultClassification[6] = "HIGH";

        int diceRollNormalization = (int) Result / (int) Value;

        return Narrative.Outcome[resultClassification[diceRollNormalization]];
    }
}

public class NarrativeFragment
{
    public string Type { get; set; }
    public string Action { get; set; }
    public Dictionary<string, string> Outcome {get; set;}
}