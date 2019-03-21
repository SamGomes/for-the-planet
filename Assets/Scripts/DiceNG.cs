using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using Utilities;

public interface IDiceNG
{
    int RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls);
}

public class RandomDiceNG : IDiceNG
{
    private System.Random random = new System.Random();

    public int RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls)
    {
        return random.Next(1, diceNumbers + 1);
    }
}

public abstract class FixedDiceNG : IDiceNG
{

    //associate one player and one round to dice results (1 dice, 2 dices, etc. )
    private Dictionary<Player, Dictionary<GameProperties.InvestmentTarget, Dictionary<int, int[]>>> seriesForDices6;
    private List<int> currIndividualDiceValues;
    protected System.Random random = new System.Random();

    public FixedDiceNG(): base()
    {
        currIndividualDiceValues = new List<int>();

        //init dictionary
        seriesForDices6 = new Dictionary<Player, Dictionary<GameProperties.InvestmentTarget, Dictionary<int, int[]>>>();
        for (int i=0; i< GameGlobals.players.Count; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            seriesForDices6[currPlayer] = new Dictionary<GameProperties.InvestmentTarget, Dictionary<int, int[]>>();
            foreach (GameProperties.InvestmentTarget target in System.Enum.GetValues(typeof(GameProperties.InvestmentTarget)))
            {
                seriesForDices6[currPlayer][target] = new Dictionary<int, int[]>();
                for (int j = 0; j < GameProperties.configurableProperties.numberOfAlbumsPerGame; j++)
                {
                    seriesForDices6[currPlayer][target][j] = new int[GameProperties.configurableProperties.numberOfAlbumsPerGame];
                }
            }
        }
        

        for (int i=0; i<GameGlobals.players.Count; i++)
        {
            Player currPlayer = GameGlobals.players[i];
            PlayerParameterization currPlayerParam = GameProperties.currGameParameterization.playerParameterizations[i];

            int lastVisitedIndex = 0;

            for (int j = 0; j < GameProperties.configurableProperties.numberOfAlbumsPerGame; j++)
            {
                foreach (GameProperties.InvestmentTarget target in System.Enum.GetValues(typeof(GameProperties.InvestmentTarget)))
                {
                    int[] currPossibleResults = new int[j+1];
                    for (int k = 0; k <= j; k++)
                    {
                        if (currPlayerParam.fixedEconomicDiceResults == null || currPlayerParam.fixedEnvironmentDiceResults == null) //failsafe
                        {
                            currPossibleResults[k] = 0;
                            continue;
                        }

                        if (target == GameProperties.InvestmentTarget.ECONOMIC)
                        {
                            currPossibleResults[k] = currPlayerParam.fixedEconomicDiceResults[lastVisitedIndex + k];
                        }
                        else
                        {
                            currPossibleResults[k] = currPlayerParam.fixedEnvironmentDiceResults[lastVisitedIndex + k];
                        }
                    }

                    seriesForDices6[currPlayer][target][j] = currPossibleResults;
                }
                lastVisitedIndex += (j+1);
            }
        }
}

    protected int RollDiceFor6(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls)
    {
        if (rollOrderNumber == 0) //first die being launched
        {
            currIndividualDiceValues.Clear();

            //achieve the right value
            int remainingPlayValue = seriesForDices6[whoRollsTheDice][diceTarget][GameGlobals.currGameRoundId][currNumberOfRolls-1];
            float decimalValueForOneDice = (float)remainingPlayValue / (float)currNumberOfRolls;
            int valueForOneDice = Mathf.FloorToInt(decimalValueForOneDice);

            for (int i=0; i< currNumberOfRolls; i++)
            {
                currIndividualDiceValues.Add(valueForOneDice);
            }

            int remainder = remainingPlayValue - (valueForOneDice * currNumberOfRolls);
            for (int i = 0; i < currNumberOfRolls; i++)
            {
                if (remainder == 0)
                {
                    break;
                }
                currIndividualDiceValues[i] += 1;
                remainder -= 1;
            }

            //randomize dices values
            //get increase
            for (int i = 0; i < currNumberOfRolls; i++)
            {
                if (currIndividualDiceValues[i] == 1)
                {
                    continue;
                }
                int randomDice = Random.Range(0, currNumberOfRolls);
                int remainderIncrease = (6 - currIndividualDiceValues[i]);
                remainderIncrease = (remainderIncrease < currIndividualDiceValues[randomDice]) ? remainderIncrease : currIndividualDiceValues[randomDice]-1;

                int randomDecrease = Random.Range(0, remainderIncrease+1);

                currIndividualDiceValues[randomDice] -= randomDecrease;
                currIndividualDiceValues[i] += randomDecrease;
            }


        }
        return currIndividualDiceValues[rollOrderNumber];
    }

    public abstract int RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls);
}

public class VictoryDiceNG : FixedDiceNG
{
    public override int RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls)
    {
        return RollDiceFor6(whoRollsTheDice, diceTarget, diceNumbers, rollOrderNumber, currNumberOfRolls);
    }
}
public class LossDiceNG : FixedDiceNG
{
    public override int RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls)
    {
        return RollDiceFor6(whoRollsTheDice, diceTarget, diceNumbers, rollOrderNumber, currNumberOfRolls);
    }
}
public class PredefinedDiceNG : FixedDiceNG
{
    public PredefinedDiceNG()
    {
    }

    public override int RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls)
    {
        return RollDiceFor6(whoRollsTheDice, diceTarget, diceNumbers, rollOrderNumber, currNumberOfRolls);   
    }
}