using System;
using System.Collections;
using System.IO;
using UnityEngine;

public interface ILogManager
{
    void InitLogs();
    IEnumerator WritePlayerToLog(string sessionId, string gameId, string playerId, string playername, string type);
    IEnumerator WriteGameToLog(string sessionId, string gameId, string condition, string result);
    IEnumerator UpdateGameResultInLog(string sessionId, string gameId, string condition, string result);
    IEnumerator WritePlayerResultsToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string money);
    IEnumerator WriteEventToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string eventType, string instrument, string coins);
    

    IEnumerator GetLastSessionConditionFromLog(Func<string,int> yieldedReactionToGet);
    IEnumerator EndLogs();
}
