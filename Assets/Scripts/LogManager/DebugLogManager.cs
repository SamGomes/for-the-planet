using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Debug log manager
public class DebugLogManager : LogManager
{
    public override void InitLogs()
    {
        Debug.Log("Log Initialzed.");
    }
    public override IEnumerator WritePlayerToLog(string sessionId, string currGameId, string playerId, string playername, string type)
    {
        Debug.Log("WritePlayerToLog: " + sessionId + ";" + currGameId + ";" + playerId + ";" + playername + ";" + type);
        yield return null;
    }
    public override IEnumerator WriteGameToLog(string sessionId, string currGameId, string condition, string result)
    {
        Debug.Log("WriteGameToLog: " + sessionId + ";" + currGameId + ";" + result);
        yield return null;
    }
    public override IEnumerator UpdateGameResultInLog(string sessionId, string gameId, string condition, string result)
    {
        Debug.Log("UpdateGameInLog: " + sessionId + ";" + gameId + ";" + result);
        yield return null;
    }
    public override IEnumerator WritePlayerResultsToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string money)
    {
        Debug.Log("WritePlayerResultsToLog: " + sessionId + ";" + currGameId + ";" + currGameRoundId + ";" + playerId + ";" + playerName + ";" + money);
        yield return null;
    }
    public override IEnumerator WriteEventToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string eventType, Dictionary<string, string> descriptionElements)
    {
        Debug.Log("WriteEventToLog: " + sessionId + ";" + currGameId + ";" + currGameRoundId + ";" + playerId + ";" + playerName + ";" + eventType + ";" + StringifyDictionaryForLogs(descriptionElements));
        yield return null;
    }

    public override IEnumerator GetLastSessionConditionFromLog(Func<string,int> yieldedReactionToGet)
    {
        Debug.Log("GotLastSessionConditionFromLog");
        yieldedReactionToGet("B");
        yield return null;
    }

    public override IEnumerator EndLogs()
    {
        Debug.Log("Log Closed.");
        yield return null;
    }
}

