using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Does this need a game manager reference like the player?
public class Narrator
{
    public List<PlayerRoundInvestment> playerRoundInvestments;
    public List<PlayerRoundEconomyDecay> playerRoundEconomyDecays;
    public List<RoundEnvironmentDecay> roundEnvironmentDecays;


    public Narrator()
    {
        playerRoundInvestments = new List<PlayerRoundInvestment>();
        playerRoundEconomyDecays = new List<PlayerRoundEconomyDecay>();
        roundEnvironmentDecays = new List<RoundEnvironmentDecay>();
    }

    public List<PlayerRoundInvestment> getInvestments(Player player)
    {
        return playerRoundInvestments.FindAll(x => x.Player.GetId() == player.GetId());
    }

    public PlayerRoundInvestment getInvestment(Player player, int round)
    {
        return playerRoundInvestments.FindAll(x => x.Player.GetId() == player.GetId()).Find(x => x.Round == round);
    }

    public List<PlayerRoundEconomyDecay> getRoundEconomyDecays(Player player)
    {
        return playerRoundEconomyDecays.FindAll(x => x.Player.GetId() == player.GetId());
    }

    public PlayerRoundEconomyDecay getRoundEconomyDecays(Player player, int round)
    {
        return playerRoundEconomyDecays.FindAll(x => x.Player.GetId() == player.GetId()).Find(x => x.Round == round);
    }

    public RoundEnvironmentDecay getRoundEnvironmentDecay(int round)
    {
        return roundEnvironmentDecays.Find(x => x.Round == round);
    }


    // Narrator Actions during Budget Allocation
    // can potentially receive a player and the budget allocated
    public IEnumerator BudgetAllocation(Player player, int round)
    {
        Dictionary<GameProperties.InvestmentTarget, int> investment = player.GetCurrRoundInvestment();

        int economy = investment[GameProperties.InvestmentTarget.ECONOMIC];
        int environment = investment[GameProperties.InvestmentTarget.ENVIRONMENT];

        // create round investment
        PlayerRoundInvestment playerRoundInvestment = new PlayerRoundInvestment(player, round, economy, environment);

        // Register the round investment
        playerRoundInvestments.Add(playerRoundInvestment);

        yield return null;
    }

    // Narrator Actions during Display History
    // should receive a player (something like that)
    public IEnumerator DisplayHistory(Player player, int round)
    {
        PlayerRoundInvestment playerRoundInvestment = this.getInvestment(player, round);

        // Compute Narrator Investment Symbols
        playerRoundInvestment.EconomyInvestmentSymbol = "DEFAULT_ECONOMY_INVESTMENT_SYMBOL";
        playerRoundInvestment.EnvironmentInvestmentSymbol = "DEFAULT_ENVIRONMENT_INVESTMENT_SYMBOL";

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Display History";
        text += "\n" + playerRoundInvestment;

        // Output Narrator Text
        Debug.Log(text);

        yield return null;
    }

    // Narrator Actions during Economy Budget Execution
    public IEnumerator EconomyBudgetExecution(Player player, int round, int economyResult)
    {
        PlayerRoundInvestment playerRoundInvestment = this.getInvestment(player, round);

        // Compute Narrator Investment Result Symbol
        playerRoundInvestment.EconomyResult = economyResult;
        playerRoundInvestment.EconomyResultSymbol = "DEFAULT_ECONOMY_RESULT_SYMBOL";

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Economy Budget Simulation";
        text += "\n" + playerRoundInvestment;

        // Output Narrator Text
        if(economyResult != 0)
        {
            Debug.Log(text);
        }

        yield return null;
    }

    // Narrator Actions during Environment Budget Execution
    public IEnumerator EnvironmentBudgetExecution(Player player, int round, int environmentResult)
    {
        PlayerRoundInvestment playerRoundInvestment = this.getInvestment(player, round);

        // Compute Narrator Investment Result Symbol
        playerRoundInvestment.EnvironmentResult = environmentResult;
        playerRoundInvestment.EnvironmentResultSymbol = "DEFAULT_ENVIRONMENT_RESULT_SYMBOL";

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Environment Budget Simulation";
        text += "\n" + playerRoundInvestment;

        // Output Narrator Text
        if (environmentResult != 0)
        {
            Debug.Log(text);
        }

        yield return null;
    }

    // Narrator Actions during Economy Decay Simulation
    public IEnumerator EconomyDecaySimulation(Player player, int round, int decay)
    {
        PlayerRoundEconomyDecay playerRoundEconomyDecay = new PlayerRoundEconomyDecay(player, round, decay);

        // Compute Narrator Investment Result Symbol
        playerRoundEconomyDecay.DecaySymbol = "DEFAULT_ECONOMY_DECAY_SYMBOL";

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Economy Decay Simulation";
        text += "\n" + playerRoundEconomyDecay;

        // Output Narrator Text
        if (decay != 0)
        {
            Debug.Log(text);
        }

        yield return null;
    }

    // Narrator Actions during Environment Decay Simulation
    public IEnumerator EnvironmentDecaySimulation(int round, int decay)
    {
        RoundEnvironmentDecay roundEnvironmentDecay = new RoundEnvironmentDecay(round, decay);

        // Compute Narrator Decay Result Symbol
        roundEnvironmentDecay.DecaySymbol = "DEFAULT_ENVIRONMENT_DECAY_SYMBOL";

        // Compute Narrator Text (symbol interpretation)
        string text = "Narrator: Environment Decay Simulation";
        text += "\n" + roundEnvironmentDecay;

        // Output Narrator Text
        if (decay != 0)
        {
            Debug.Log(text);
        }

        yield return null;
    }

    // Narrator Actions during Game Start
    public IEnumerator GameStart()
    {
        // Compute Narrator Text
        string text = "Narrator: Game Start";

        // Output Narrator Text
        Debug.Log(text);

        yield return null;
    }

    // Narrator Actions during Round Start
    public IEnumerator RoundStart()
    {
        // Compute Narrator Text
        string text = "Narrator: Round Start";

        // Output Narrator Text
        Debug.Log(text);

        yield return null;
    }

    // Narrator Actions during Game End
    public IEnumerator GameEnd()
    {
        // Compute Narrator Text
        string text = "Narrator: Game End";

        // Output Narrator Text
        Debug.Log(text);

        yield return null;
    }

    // Narrator Text Bubble
    private IEnumerator DisplayNarratorText(string message, float delay)
    {
        yield return null;
    }

}


public class PlayerRoundInvestment {

    public Player Player { get; set;}

    public int Round { get; set; }

    public int EconomyInvestment { get; set; }
    public string EconomyInvestmentSymbol { get; set; }

    public int? EconomyResult { get; set; }
    public string EconomyResultSymbol { get; set; }

    public int EnvironmentInvestment { get; set; }
    public string EnvironmentInvestmentSymbol { get; set; }

    public int? EnvironmentResult { get; set; }
    public string EnvironmentResultSymbol { get; set; }

    public PlayerRoundInvestment(Player player, int round, int economyInvestment, int environmentInvestment)
    {
        Player = player;
        Round = round;

        EconomyInvestment = economyInvestment;
        EconomyInvestmentSymbol = null;

        EconomyResult = null;
        EconomyResultSymbol = null;

        EnvironmentInvestment = environmentInvestment;
        EnvironmentInvestmentSymbol = null;

        EnvironmentResult = null;
        EnvironmentResultSymbol = null;
    }

    public override string ToString()
    {
        // needs to be fixed so that it also outputs the symbol and the rolls
        return "Player " + Player.GetName() + " - Round " + Round + ": " 
            + "Economy - " + EconomyInvestmentSymbol + " (" + EconomyInvestment + ") " + EconomyResultSymbol + " (" + EconomyResult + ") "
            +  " Environment - " + EnvironmentInvestmentSymbol + " (" + EnvironmentInvestment + ") " + EnvironmentResultSymbol + " (" + EnvironmentResult + ") ";
    }
}

public class PlayerRoundEconomyDecay
{

    public Player Player { get; set; }

    public int Round { get; set; }

    public int DecayResult { get; set; }
    public string DecaySymbol { get; set; }

    public PlayerRoundEconomyDecay(Player player, int round, int decay)
    {
        Player = player;
        Round = round;

        DecayResult = decay;
        DecaySymbol = null;
    }

    public override string ToString()
    {
        // needs to be fixed so that it also outputs the symbol and the rolls
        return "Player " + Player.GetName() + " - Round " + Round + ": "
            + "Economy Decay - " + DecaySymbol + " (" + DecayResult + ") ";
    }
}

public class RoundEnvironmentDecay
{

    public int Round { get; set; }

    public int DecayResult { get; set; }
    public string DecaySymbol { get; set; }

    public RoundEnvironmentDecay(int round, int decay)
    {
        Round = round;

        DecayResult = decay;
        DecaySymbol = null;
    }

    public override string ToString()
    {
        // needs to be fixed so that it also outputs the symbol and the rolls
        return "Round " + Round + ": "
            + "Environment Decay - " + DecaySymbol + " (" + DecayResult + ") ";
    }
}