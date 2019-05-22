using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public abstract class LogManager
{
    public string StringifyDictionaryForLogs(Dictionary<string,string> dict)
    {
        string result = "{";
        List<string> dictKeys = new List<string>(dict.Keys);
        for (int keyI=0; keyI < dictKeys.Count; keyI++)
        {
            string key = dictKeys[keyI];

            result += " "+ key + ": " + dict[key];
            if(keyI < dictKeys.Count - 1)
            {
                result += ";";
            }
            else
            {
                result += " }";

            }
        }
        return result;
    }

    public abstract void InitLogs();
    public abstract IEnumerator WritePlayerToLog(string sessionId, string gameId, string playerId, string playername, string type);
    public abstract IEnumerator WriteGameToLog(string sessionId, string gameId, string condition, string result);
    public abstract IEnumerator UpdateGameResultInLog(string sessionId, string gameId, string condition, string result);
    public abstract IEnumerator WritePlayerResultsToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string money);
    public abstract IEnumerator WriteEventToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string eventType, Dictionary<string,string> descriptionElements);


    public abstract IEnumerator GetLastSessionConditionFromLog(Func<string,int> yieldedReactionToGet);
    public abstract IEnumerator EndLogs();
}
