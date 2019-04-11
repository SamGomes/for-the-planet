using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    IDiceLogic diceLogic;
    GameObject diceUIPrefab;

    public DiceManager(GameObject diceUIPrefab, IDiceLogic diceLogic)
    {
        this.diceUIPrefab = diceUIPrefab;
        this.diceLogic = diceLogic;
    }

    public void RollTheDice(Player whoRollsTheDice, GameProperties.InvestmentTarget diceTarget, int diceNumbers, int rollOrderNumber, int currNumberOfRolls)
    {
        diceLogic.RollTheDice(whoRollsTheDice, diceTarget, diceNumbers, rollOrderNumber, currNumberOfRolls);

    }
}
