using System;
using System.Collections;
using UnityEngine;

//Debug log manager
public class DebugLogManager : ILogManager
{
    public void InitLogs()
    {
        Debug.Log("Log Initialzed.");
    }
    public IEnumerator WritePlayerToLog(string sessionId, string currGameId, string playerId, string playername, string type)
    {
        Debug.Log("WritePlayerToLog: " + sessionId + ";" + currGameId + ";" + playerId + ";" + playername + ";" + type);
        yield return null;
    }
    public IEnumerator WriteGameToLog(string sessionId, string currGameId, string condition, string result)
    {
        Debug.Log("WriteGameToLog: " + sessionId + ";" + currGameId + ";" + result);
        yield return null;
    }
    public IEnumerator UpdateGameResultInLog(string sessionId, string gameId, string condition, string result)
    {
        Debug.Log("UpdateGameInLog: " + sessionId + ";" + gameId + ";" + result);
        yield return null;
    }
    public IEnumerator WritePlayerResultsToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string money)
    {
        Debug.Log("WritePlayerResultsToLog: " + sessionId + ";" + currGameId + ";" + currGameRoundId + ";" + playerId + ";" + playerName + ";" + money);
        yield return null;
    }
    public IEnumerator WriteEventToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string eventType, string skill, string coins)
    {
        Debug.Log("WriteEventToLog: " + sessionId + ";" + currGameId + ";" + currGameRoundId + ";" + playerId + ";" + playerName + ";" + eventType + ";" + skill + ";" + coins);
        yield return null;
    }

    public IEnumerator GetLastSessionConditionFromLog(Func<string,int> yieldedReactionToGet)
    {
        Debug.Log("GotLastSessionConditionFromLog");
        yieldedReactionToGet("B");
        yield return null;
    }

    public IEnumerator EndLogs()
    {
        Debug.Log("Log Closed.");
        yield return null;
    }
}

