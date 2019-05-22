using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

//Debug log manager
public class MongoDBLogManager : LogManager
{
    private bool isGameRunning;

    private string databaseName;
    private string myApiKey;

    Hashtable postHeader;
    private List<PendingCall> pendingCalls;
    private UnityWebRequestAsyncOperation currRequest;

    [Serializable]
    public class DataEntryPlayerLog
    {
        public string sessionId;
        public string currGameId;
        public string playerId;
        public string playerName;
        public string type;
    }
    [Serializable]
    public class DataEntryGameLog
    {
        public string sessionId;
        public string currGameId;
        public string playerId;
        public string playerName;
        public string type;
    }
    [Serializable]
    public class DataEntryGameResultLog
    {
        public string sessionId;
        public string currGameId;
        public string condition;
        public string result;
    }
    [Serializable]
    public class DataEntryAlbumResultLog
    {
        public string sessionId;
        public string currGameId;
        public string currGameRoundId;
        public string currAlbumId;
        public string currAlbumName;
        public string marktingState;
    }
    [Serializable]
    public class DataEntryPlayerResultLog
    {
        public string sessionId;
        public string currGameId;
        public string currGameRoundId;
        public string playerId;
        public string playerName;
        public string money;
    }
    [Serializable]
    public class DataEntryEventLog
    {
        public string sessionId;
        public string currGameId;
        public string currGameRoundId;
        public string playerId;
        public string playerName;
        public string eventType;
        public string description;
    }

    [Serializable]
    public class DataEntryGameResultLogQueryResult
    {
        public List<DataEntryGameResultLog> results;
    }


    private struct PendingCall
    {
        public UnityWebRequest www;
        public Func<string, int> yieldedReaction;
        public PendingCall(UnityWebRequest www, Func<string, int> yieldedReaction)
        {
            this.yieldedReaction = yieldedReaction;
            this.www = www;
            www.SetRequestHeader("Content-Type", "application/json"); //in order to be recognized by the mongo server
        }
    }
    private IEnumerator ExecuteCall(UnityWebRequestAsyncOperation currConnection, PendingCall call)
    {
        //Debug.Log("number of pending calls: " + pendingCalls.Count);
        yield return currConnection;
        currConnection = call.www.SendWebRequest();
        yield return currConnection;
        Debug.Log("remote call error code returned (no return means no error): "+ call.www.error);
        if (call.yieldedReaction != null)
        {
            call.yieldedReaction(call.www.downloadHandler.text);
        }
        pendingCalls.Remove(call);
        yield return currConnection;
    }

    public override void InitLogs()
    {
        databaseName = "fortheplanetlogs";
        myApiKey = "skgyQ8WGQIP6tfmjytmcjzlgZDU2jWBD";

        pendingCalls = new List<PendingCall>();
        isGameRunning = true;
    }

    private UnityWebRequest ConvertEntityToPostRequest(System.Object entity, string database, string collection)
    {
        string url = "https://api.mlab.com/api/1/databases/" + databaseName + "/collections/" + collection + "?apiKey=" + myApiKey;

        string entityJson = JsonUtility.ToJson(entity);
        byte[] formData = System.Text.Encoding.UTF8.GetBytes(entityJson);
        UnityWebRequest www = UnityWebRequest.Post(url, entityJson);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(formData);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        return www;
    }
    private UnityWebRequest ConvertEntityToGetRequest(string database, string collection, string query)
    {
        string url = "https://api.mlab.com/api/1/databases/" + databaseName + "/collections/" + collection + "?apiKey=" + myApiKey + query;
        UnityWebRequest www = UnityWebRequest.Get(url);
        return www;
    }
    private UnityWebRequest ConvertEntityToPutRequest(System.Object entity, string database, string collection, string query)
    {
        string url = "https://api.mlab.com/api/1/databases/" + databaseName + "/collections/" + collection + "?apiKey=" + myApiKey + query;

        string entityJson = JsonUtility.ToJson(entity);
        UnityWebRequest www = UnityWebRequest.Put(url, entityJson);
        return www;
    }


    public override IEnumerator WritePlayerToLog(string sessionId, string currGameId, string playerId, string playerName, string type)
    {
        var entity = new DataEntryPlayerLog
        {
            sessionId = sessionId,
            currGameId = currGameId,
            playerId = playerId,
            playerName = playerName,
            type = type
        };
        PendingCall call = new PendingCall(ConvertEntityToPostRequest(entity, databaseName, "playerslog"), null);
        pendingCalls.Add(call);
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(ExecuteCall(currRequest,call));
        pendingCalls.Remove(call);
    }

    public override IEnumerator WriteGameToLog(string sessionId, string currGameId, string condition, string result)
    {
        var entity = new DataEntryGameResultLog
        {
            sessionId = sessionId,
            currGameId = currGameId,
            condition = condition,
            result = result
        };

        PendingCall call = new PendingCall(ConvertEntityToPostRequest(entity, databaseName, "gameresultslog"), null);
        pendingCalls.Add(call);
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(ExecuteCall(currRequest, call));
        pendingCalls.Remove(call);
    }
    public override IEnumerator UpdateGameResultInLog(string sessionId, string currGameId, string condition, string result)
    {
        var entity = new DataEntryGameResultLog
        {
            sessionId = sessionId,
            currGameId = currGameId,
            condition = condition,
            result = result
        };
        string collection = "gameresultslog";
        string query = "&q={\"currGameId\": \"" + currGameId + "\", \"sessionId\":\"" + sessionId + "\"}";

        PendingCall call = new PendingCall(ConvertEntityToPutRequest(entity, databaseName, collection, query), null);
        pendingCalls.Add(call);
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(ExecuteCall(currRequest, call));
        pendingCalls.Remove(call);
    }

    public override IEnumerator WritePlayerResultsToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName, string money)
    {
        var entity = new DataEntryPlayerResultLog
        {
            sessionId = sessionId,
            currGameId = currGameId,
            currGameRoundId = currGameRoundId,
            playerId = playerId,
            playerName = playerName,
            money = money
        };
        PendingCall call = new PendingCall(ConvertEntityToPostRequest(entity, databaseName, "playerresultslog"), null);
        pendingCalls.Add(call);
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(ExecuteCall(currRequest, call));
        pendingCalls.Remove(call);
    }

    public override IEnumerator WriteEventToLog(string sessionId, string currGameId, string currGameRoundId, string playerId, string playerName,
        string eventType, Dictionary<string, string> descriptionElements)
    {
        string description = StringifyDictionaryForLogs(descriptionElements);
        var entity = new DataEntryEventLog
        {
            sessionId = sessionId,
            currGameId = currGameId,
            currGameRoundId = currGameRoundId,
            playerId = playerId,
            playerName = playerName,
            eventType = eventType,
            description = description
        };
        PendingCall call = new PendingCall(ConvertEntityToPostRequest(entity, databaseName, "eventslog"), null);
        pendingCalls.Add(call);
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(ExecuteCall(currRequest, call));
        pendingCalls.Remove(call);
    }

    public override IEnumerator GetLastSessionConditionFromLog(Func<string, int> yieldedReactionToGet)
    {
       string query = "&s={\"_id\": -1}&l=1"; //query which returns the last game result
       PendingCall call = new PendingCall(ConvertEntityToGetRequest(databaseName, "gameresultslog", query), 
            delegate (string lastGameEntry){
                string lastConditionString = "";

                lastGameEntry = "{ \"results\": " + lastGameEntry + "}";
                DataEntryGameResultLogQueryResult lastGameEntriesObject = JsonUtility.FromJson<DataEntryGameResultLogQueryResult>(lastGameEntry);
                if (lastGameEntriesObject.results.Count > 0)
                {
                    lastConditionString = ((DataEntryGameResultLog)(lastGameEntriesObject.results[lastGameEntriesObject.results.Count - 1])).condition;
                }
                yieldedReactionToGet(lastConditionString);
                return 0;
            }
        );
        pendingCalls.Add(call);
        yield return GameGlobals.monoBehaviourFunctionalities.StartCoroutine(ExecuteCall(currRequest, call));
        pendingCalls.Remove(call);
    }

    public override IEnumerator EndLogs()
    {
        while (pendingCalls.Count > 0)
        {
            yield return null;
        }
        Debug.Log("Log Closed.");
    }
}

