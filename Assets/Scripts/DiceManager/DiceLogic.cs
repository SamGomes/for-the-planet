using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

public interface IDiceLogic
{
    int RollTheDice(int numberOfDiceSides, int currNumberOfRolls);
}

public class RandomDiceLogic : IDiceLogic
{
    private System.Random random = new System.Random();

    public int RollTheDice(int numberOfDiceSides, int currNumberOfRolls)
    {
        return random.Next(1, numberOfDiceSides + 1);
    }
}

public abstract class FixedDiceLogic : IDiceLogic
{
    public abstract int RollTheDice(int numberOfDiceSides, int currNumberOfRolls);
}