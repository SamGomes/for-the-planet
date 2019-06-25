using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Does this need a game manager reference like the player?
public class Narrator
{
    public List<NarrativeInterpretation> narrativeInterpretations;

    private GameObject NarratorUI;
    private GameObject CanvasUI;

    private LegendsInteractionModule InteractionModule;


    public Narrator(GameObject narratorCanvas)
    {
        narrativeInterpretations = new List<NarrativeInterpretation>();


        CanvasUI = narratorCanvas;
        NarratorUI = Resources.Load<GameObject>("Prefabs/NarratorUI");

        InteractionModule = new LegendsInteractionModule();
        InteractionModule.Init(NarratorUI, CanvasUI);
    }


    // Narrative Interpretations
    // Need to validate What I will actually need, trim this as required (over-engineered implementation)
    public List<NarrativeInterpretation> getNarrativeInterpretations()
    {
        return narrativeInterpretations;
    }

    public List<NarrativeInterpretation> getNarrativeInterpretations(Player player)
    {
        return narrativeInterpretations.FindAll(x => x.Player.GetId() == player.GetId()); ;
    }

    public List<NarrativeInterpretation> getNarrativeInterpretations(Player player, int round)
    {
        return narrativeInterpretations.FindAll(x => x.Player.GetId() == player.GetId()).FindAll(x => x.Round == round);
    }

    public NarrativeInterpretation getNarrativeInterpretation(Player player, int round, string type)
    {
        return narrativeInterpretations.FindAll(x => x.Player.GetId() == player.GetId()).FindAll(x => x.Round == round).Find(x => x.Type == type);
    }

    public List<NarrativeInterpretation> getNarrativeInterpretations(int round, string type)
    {
        return narrativeInterpretations.FindAll(x => x.Type == type).FindAll(x => x.Round == round);
    }

    // Narrator Actions during Budget Allocation
    // can potentially receive a player and the budget allocated
    public IEnumerator BudgetAllocation(Player player, int round)
    {
        Dictionary<GameProperties.InvestmentTarget, int> investment = player.GetCurrRoundInvestment();

        int economy = investment[GameProperties.InvestmentTarget.ECONOMIC];
        int environment = investment[GameProperties.InvestmentTarget.ENVIRONMENT];

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

        // Create NarrativeInterpretation for Environment Investment
        NarrativeInterpretation economyInvestmentNarrativeInterpretation = new NarrativeInterpretation
        {
            Player = player,
            Round = round,
            Value = economy,
            Type = "ECONOMY_INVESTMENT"
        };

        // Register NarrativeInterpretation for Environment Investment
        narrativeInterpretations.Add(economyInvestmentNarrativeInterpretation);

        yield return null;
    }

    // Narrator Actions during Display History
    public IEnumerator DisplayHistory(Player player, int round)
    {
        string text = "Narrator: Display History\n";

        getNarrativeInterpretations(player, round).ForEach(delegate (NarrativeInterpretation interpretation)
        {
            text += interpretation.ToString() + "\n";
        });

        // Output Narrator Text
        InteractionModule.Speak(text);

        yield return null;
    }

    // Narrator Actions during Economy Budget Execution
    public IEnumerator EconomyBudgetExecution(Player player, int round, int economyResult)
    {
        NarrativeInterpretation narrativeInterpretation = getNarrativeInterpretation(player, round, "ECONOMY_INVESTMENT");
        narrativeInterpretation.Result = economyResult;

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Economy Budget Simulation";
        text += "\n" + narrativeInterpretation.ToString();

        // Output Narrator Text
        if(economyResult != 0)
        {
            InteractionModule.Speak(text);
        }

        yield return null;
    }

    // Narrator Actions during Environment Budget Execution
    public IEnumerator EnvironmentBudgetExecution(Player player, int round, int environmentResult)
    {
        NarrativeInterpretation narrativeInterpretation = getNarrativeInterpretation(player, round, "ENVIRONMENT_INVESTMENT");
        narrativeInterpretation.Result = environmentResult;

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Environment Budget Simulation";
        text += "\n" + narrativeInterpretation.ToString();

        // Output Narrator Text
        if (environmentResult != 0)
        {
            InteractionModule.Speak(text);
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
        string text = "Narrator: Economy Decay Simulation";
        text += "\n" + economyDecayNarrativeInterpretation.ToString();


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
        string text = "Narrator: Environment Decay Simulation";
        text += "\n" + environmentDecayNarrativeInterpretation.ToString();

        // Output Narrator Text
        if (decay != 0)
        {
            InteractionModule.Speak(text);
        }

        yield return null;
    }

    // Narrator Actions during Game Start
    // should give game setting context
    public IEnumerator GameStart()
    {
        // Compute Narrator Text
        // Placeholder
        string text = "Three neighbouring countries are attempting to develop their economies. " +
            "Unfortunately, a lot of damage has already been done to the environment, and they need to make sure that the planet survives their endeavours.";

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
}