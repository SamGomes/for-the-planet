using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Does this need a game manager reference like the player?
public class Narrator
{
    public List<NarrativeInterpretation> NarrativeInterpretations;
    public List<NarrativeFragment> NarrativeFragments;

    private GameObject NarratorUI;
    private GameObject CanvasUI;

    private SpeechBaloonInteractionModule InteractionModule;


    public Narrator(GameObject narratorCanvas)
    {
        NarrativeInterpretations = new List<NarrativeInterpretation>();
        NarrativeFragments = new List<NarrativeFragment>();

        CanvasUI = narratorCanvas;
        NarratorUI = Resources.Load<GameObject>("Prefabs/NarratorUI");

        InteractionModule = new SpeechBaloonInteractionModule();
        InteractionModule.Init(NarratorUI, CanvasUI, true);
    }

    public SpeechBaloonInteractionModule GetInteractionModule()
    {
        return this.InteractionModule;
    }

    private void InitNarrativeFragments()
    {
        InitGameStartNarrativeFragment();

        InitRoundStartNarrativeFragments();

        InitEnvironmentInvestmentNarrativeFragments();
        InitEconomyInvestmentNarrativeFragments();

        InitEnvironmentDecayNarrativeFragments();
        InitEconomyDecayNarrativeFragments();
    }

    private void InitGameStartNarrativeFragment()
    {
        //  The number of players should be fetch
        string text = GameGlobals.players.Count + " neighbouring countries are attempting to develop their economies. " +
            "Unfortunately, a lot of damage has already been done to the environment, and they need to make sure that the planet survives their endeavours.";
        NarrativeFragment gameStart = new NarrativeFragment
        {
            Type = "GAME_START",
            Action = text
        };
        NarrativeFragments.Add(gameStart);
    }

    private void InitRoundStartNarrativeFragments()
    {
        // To be Implemented
    }

    // Placeholders
    private void InitEnvironmentInvestmentNarrativeFragments()
    {
        AddEnvironmentInvestmentNarrativeFragment("Implement a Plastic Recycling Program",
            "Despite the Plastic Recycling Program, most of the Plastic still ended up in the dumps.",
            "Some Plastic still reaches the dump, but a substancial ammount is being recycled.",
            "The sight of Plastic in a trash bin is now a rare occurrence, and most of the Plastic comes from a recycled source.", 1);

        AddEnvironmentInvestmentNarrativeFragment("River Protection and Cleanup Operation",
            "Most of the floating debris in the River is gone, but the water is still opaque and the smell is still vomit-inducing.", 
            "Fishes have returned to the river, but the population would still think twice about eating them.", 
            "The once polluted river is now pristine and regular leisure spot for the population.", 2);

        AddEnvironmentInvestmentNarrativeFragment("Reforestation Effort",
            "Despite the investment, only a few symbolic gestures have had success, there are some new trees in most of the cities.", 
            "A significant reduction in deforestation in city areas has been achieved by creating some parks.", 
            "Several of the existing areas have been classified as protected environment and areas that were previously barren have been reforested.", 3);

        AddEnvironmentInvestmentNarrativeFragment("Improving the Public Transport Infrastructure",
            "Some new Bus Routes have been created, but due to congestion and limited offer, most people still prefer to take their private transport.",
            "Expanding the schedule of the Public Transport Infrastructure, allowed a significant reduction of the pollution caused by traffic.",
            "The addition of tracks to the Subway System, as well as the reinforcement of the available trains, allowed a huge reduction in commuting traffic.", 4);

        AddEnvironmentInvestmentNarrativeFragment("Installation a Solar Power Plant",
            "Due to poor planning, the location of the Solar Power Plant was badly chosen, and only allowed a slight decrease in the consumption of fossil fuels",
            "The installation of a Solar Power Plant has allowed a significant reduction on the pollution required to generate electricity.", 
            "Good planning and investing in a state of the art Solar Power Plant permitted the decommission of a Coal Power Plant.", 5);
    }

    private void AddEnvironmentInvestmentNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh, int value)
    {
        NarrativeFragment narrative = new NarrativeFragment
        {
            Type = "ENVIRONMENT_INVESTMENT",
            Action = action,
            Outcome = new Dictionary<string, string>(),
            Value = value
        };

        narrative.Outcome["LOW"] = outcomeLow;
        narrative.Outcome["MEDIUM"] = outcomeMedium;
        narrative.Outcome["HIGH"] = outcomeHigh;

        NarrativeFragments.Add(narrative);
    }

    // Economy Invest Actions
    // raw material industry (extraction)
    // Manufacture industry
    // Tourism industry
    // Outsourcing industry
    // Intellectual Property industry

    private void InitEconomyInvestmentNarrativeFragments()
    {

        AddEconomyInvestmentNarrativeFragment("Opening an area for Rare Earth Elements Mining",
            "Unfortunately, the price of the Rare Earth Elements has come down due to a decrease in demand, and the investment underperformed.",
            "The market for Rare Earth Elements stayed stable, and the Mining Operation had the predicted impact in the economy.",
            "A sudden jump in price of Rare Earth Elements has allowed the Mining Operation to have better than expected economical impact.", 1);

        AddEconomyInvestmentNarrativeFragment("Creation of an Electronics Manufacture Plant",
            "Due to the cheaper labour offered elsewhere, the Creation of an Electronics Manufacture Plant has had less impact in the economy than was expected.",
            "The Creation of an Electronics Manufacture Plant had the expected impact in the economy",
            "Due to the recognition of the quality of the product, the Creation of an Electronics Manufacture Plant had an extremely positive impact in the economy", 2);

        AddEconomyInvestmentNarrativeFragment("Building a new Hotel",
            "The new Hotel has been at a low occupancy rate, due to the sudden popularity of other destinations",
            "The influx of tourists allowed the new Hotel to have the expected returns.", 
            "The new Hotel has been getting extremely positive reviews, and has been at permanently at capacity.", 3);

        AddEconomyInvestmentNarrativeFragment("Creating a new Helpdesk Center",
            "The ease of transfering Helpdesk operations to the cheapest bidder, has made it difficult for the project to deliver on its promisses.",
            "The new Helpdesk center managed to get enough clients for it to have the expected economical impact.",
            "Due to the availabilty of speakers of different languages, several firms are currently outsourcing their Helpdesk to the new Center.", 4);

        AddEconomyInvestmentNarrativeFragment("Research and Development of Intellectual Property",
            "The industry has shifted in unexpected directions, and most of the solutions that have come from Research and Development were not profitable",
            "The Research and Development of Intellectual Property has been contributing to an uptick in the economy.",
            "Several new game changing ideas have made the returns from Research and Development of Intellectual Property to be the main driver of the economy", 5);


    }

    private void AddEconomyInvestmentNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh, int value)
    {
        NarrativeFragment narrative = new NarrativeFragment
        {
            Type = "ECONOMY_INVESTMENT",
            Action = action,
            Outcome = new Dictionary<string, string>(),
            Value = value
        };

        narrative.Outcome["LOW"] = outcomeLow;
        narrative.Outcome["MEDIUM"] = outcomeMedium;
        narrative.Outcome["HIGH"] = outcomeHigh;

        NarrativeFragments.Add(narrative);
    }


    // Placeholders
    private void InitEnvironmentDecayNarrativeFragments()
    {
        // Action text is currently unused for the decay
        AddEnvironmentDecayNarrativeFragment("ENVIRONMENT_DECAY_ACTION", 
            "Extra plastic has been discarded due to the sudden popularity of swirly straws.",
            "Some corporations have been contaminating the water streams.",
            "Due to increasing power demands, there has been an increase in the burning of Fossil Fuels.");
    }

    private void AddEnvironmentDecayNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh)
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

        NarrativeFragments.Add(narrative);
    }

    private void InitEconomyDecayNarrativeFragments()
    {
        AddEconomyDecayNarrativeFragment("ECONOMY_DECAY_ACTION", 
            "Death and Taxes are inevitable, but in the Modern World, so is Inflation.", 
            "The stock marked is now classified as a Bear Market.", 
            "A recession has reared its head.");
    }

    private void AddEconomyDecayNarrativeFragment(string action, string outcomeLow, string outcomeMedium, string outcomeHigh)
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

        NarrativeFragments.Add(narrative);
    }

    private NarrativeFragment GetGameStartNarrativeFragment()
    {
        return NarrativeFragments.Find(x => x.Type == "GAME_START");
    }

    private List<NarrativeFragment> GetRoundStartNarrativeFragments()
    {
        return NarrativeFragments.FindAll(x => x.Type == "ROUND_START");
    }

    private List<NarrativeFragment> GetEnvironmentInvestmentNarrativeFragments()
    {
        return NarrativeFragments.FindAll(x => x.Type == "ENVIRONMENT_INVESTMENT");
    }

    private List<NarrativeFragment> GetEnvironmentInvestmentNarrativeFragments(int value)
    {
        return NarrativeFragments.FindAll(x => x.Type == "ENVIRONMENT_INVESTMENT").FindAll(x=> x.Value == value);
    }

    private List<NarrativeFragment> GetEconomyInvestmentNarrativeFragments()
    {
        return NarrativeFragments.FindAll(x => x.Type == "ECONOMY_INVESTMENT");
    }

    private List<NarrativeFragment> GetEconomyInvestmentNarrativeFragments(int value)
    {
        return NarrativeFragments.FindAll(x => x.Type == "ECONOMY_INVESTMENT").FindAll(x => x.Value == value);
    }

    private List<NarrativeFragment> GetEnvironmentDecayNarrativeFragments()
    {
        return NarrativeFragments.FindAll(x => x.Type == "ENVIRONMENT_DECAY");
    }

    private List<NarrativeFragment> GetEconomyDecayNarrativeFragments()
    {
        return NarrativeFragments.FindAll(x => x.Type == "ECONOMY_DECAY");
    }

    private List<NarrativeFragment> GetNarrativeFragments()
    {
        return NarrativeFragments;
    }


    // Narrative Interpretations
    // Need to validate What I will actually need, trim this as required (over-engineered implementation)
    private List<NarrativeInterpretation> GetNarrativeInterpretations()
    {
        return NarrativeInterpretations;
    }

    private List<NarrativeInterpretation> GetNarrativeInterpretations(Player player)
    {
        return NarrativeInterpretations.FindAll(x => x.Player != null)
            .FindAll(x => x.Player.GetId() == player.GetId()); ;
    }

    private List<NarrativeInterpretation> GetNarrativeInterpretations(Player player, int round)
    {
        return NarrativeInterpretations.FindAll(x => x.Player != null)
            .FindAll(x => x.Player.GetId() == player.GetId()).FindAll(x => x.Round == round);
    }

    private NarrativeInterpretation GetNarrativeInterpretation(Player player, int round, string type)
    {
        return NarrativeInterpretations.FindAll(x => x.Player != null)
            .FindAll(x => x.Player.GetId() == player.GetId()).FindAll(x => x.Round == round).Find(x => x.Type == type);
    }

    private List<NarrativeInterpretation> GetNarrativeInterpretations(int round, string type)
    {
        return NarrativeInterpretations.FindAll(x => x.Type == type).FindAll(x => x.Round == round);
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
    // should use a narrative fragment somehow
    // Speak about the current status of the game, environment and each player's economy ???
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
            NarrativeInterpretations.Add(environmentInvestmentNarrativeInterpretation);

            // Economy Investment Fragment
            // Compute Narrator Text (symbol interpretation)
            // Should fetch a random
            List<NarrativeFragment> environmentInvestmentFragments = GetEnvironmentInvestmentNarrativeFragments(environment);
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
            NarrativeInterpretations.Add(economyInvestmentNarrativeInterpretation);

            // Economy Investment Fragment
            // Compute Narrator Text (symbol interpretation)
            // Should fetch a random
            List<NarrativeFragment> economyInvestmentFragments = GetEconomyInvestmentNarrativeFragments(economy);
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
        NarrativeInterpretations.Add(economyDecayNarrativeInterpretation);

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
        NarrativeInterpretations.Add(environmentDecayNarrativeInterpretation);

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
    private IEnumerator GameEnd()
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
    public int? Value { get; set; }
}