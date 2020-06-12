using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.Networking;


[Serializable]
public class BsonDocument
{
    public string currSessionId;
    public string currGameId;
    public string generation;
    public string playerName;
    public string playerType;
    public string playerTookFromCP;
    public string playerGain;
    public string envState;

}

//Debug log manager
public class MongoAtlasLogManager : MonoBehaviour {

    private MonoBehaviour monoBehaviourObject;

#if UNITY_WEBGL && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void insertOneDB(string currSessionId, string currGameId , string generation ,string playerName ,string playerType, string playerTookFromCP, 
        string playerGain, string envState);

    public void InitLogs(MonoBehaviour monoBehaviourObject)
    {
        this.monoBehaviourObject = monoBehaviourObject;
    }


    public void Start(Dictionary<string, string> argsNValues)
    {
        insertOneDB(argsNValues["currSessionId"], argsNValues["currGameId"], argsNValues["generation"], argsNValues["playerName"],
                argsNValues["playerType"], argsNValues["playerTookFromCP"], argsNValues["playerGain"], argsNValues["envState"]);
    }


    public IEnumerator WriteToLog(Dictionary<string, string> argsNValues){
        insertOneDB(argsNValues["currSessionId"], argsNValues["currGameId"], argsNValues["generation"], argsNValues["playerName"],
            argsNValues["playerType"], argsNValues["playerTookFromCP"], argsNValues["playerGain"], argsNValues["envState"]);
        yield return null;
    }

    public IEnumerator EndLogs(){
        return null;
    }

#else

    public void InitLogs(MonoBehaviour monoBehaviourObject)
    {
        this.monoBehaviourObject = monoBehaviourObject;
    }

    public void Start(Dictionary<string, string> argsNValues)
    {
        Debug.Log("Only works in WebGL");
    }

    public IEnumerator WriteToLog(Dictionary<string, string> argsNValues)
    {
        Debug.Log("Only works in WebGL");
        yield return null;
    }

    public IEnumerator EndLogs()
    {
        return null;
    }
#endif
}


//C# MongoDBLogManager
/*
    public override void InitLogs(MonoBehaviour monoBehaviourObject)
    {
        this.monoBehaviourObject = monoBehaviourObject;
        //this.client = new MongoClient(GameProperties.configurableProperties.mongoConnector);
    }
    

    public override IEnumerator WriteToLog(string database, string table, Dictionary<string,string> argsNValues)
    {
        var databaseObj = client.GetDatabase(database);
        var document = BsonDocument.Parse(StringifyDictionaryForLogs(argsNValues));
        databaseObj.GetCollection<BsonDocument>(table).InsertOne(document);
        yield return null;
    }

    public override IEnumerator GetFromLog(string database, string table, string query, Func<string, int> yieldedReactionToGet)
    {
        var databaseObj = client.GetDatabase(database);
        var collection = databaseObj.GetCollection<BsonDocument>(table);

        QueryObject queryObj = JsonUtility.FromJson<QueryObject>(query);
        
        var queryRes = collection.Find(queryObj.find).Sort(queryObj.sort).Limit(queryObj.limit).ToList();
        //remove _id attributes
        foreach(var queryItem in queryRes)
        {
            queryItem.RemoveAt(0);
        }
        //yield return yieldedReactionToGet(queryRes.ToJson());
        return null;
    }

    public override IEnumerator UpdateLog(string database, string table, string query, Dictionary<string, string> argsNValues)
    {
        var databaseObj = client.GetDatabase(database);
        var collection = databaseObj.GetCollection<BsonDocument>(table);
//        collection.FindOneAndReplace(user => user._id == newModelUser._id, newModelUser);
        return null;
    }


    public override IEnumerator EndLogs()
    {
        return null;
    }
}
*/
