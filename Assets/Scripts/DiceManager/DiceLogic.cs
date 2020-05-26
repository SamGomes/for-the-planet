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


public class AlwaysHalfDiceLogic : IDiceLogic
{
    private System.Random random = new System.Random();

    public int RollTheDice(int numberOfDiceSides, int currNumberOfRolls)
    {
        float half = numberOfDiceSides / 2.0f;
        return random.Next(Mathf.FloorToInt(half), Mathf.CeilToInt(half));
    }
}


public abstract class FixedDiceLogic : IDiceLogic
{
    public abstract int RollTheDice(int numberOfDiceSides, int currNumberOfRolls);
}