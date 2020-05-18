using System;
using System.Collections;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using UnityEngine;
using UnityEngine.Networking;

//Debug log manager
public class MongoAtlasLogManager : LogManager
{
    private MonoBehaviour monoBehaviourObject;
    private MongoClient client;

    public override void InitLogs(MonoBehaviour monoBehaviourObject)
    {
        this.monoBehaviourObject = monoBehaviourObject;
        this.client = new MongoClient("mongodb+srv://studyAC1:studyAC1@cluster0-nfksn.mongodb.net/test?retryWrites=true&w=majority");
   
    }
    

    public override IEnumerator WriteToLog(string database, string table, Dictionary<string,string> argsNValues)
    {
        var databaseObj = client.GetDatabase(database);
        var document = BsonDocument.Parse(StringifyDictionaryForLogs(argsNValues));
        databaseObj.GetCollection<BsonDocument>(table).InsertOne(document);
        return null;
    }

    public override IEnumerator GetFromLog(string database, string table, string query, Func<string, int> yieldedReactionToGet)
    {
        yield return yieldedReactionToGet("[]");
    }

    public override IEnumerator UpdateLog(string database, string table, string query, Dictionary<string, string> argsNValues)
    {
        var databaseObj = client.GetDatabase(database);
        return null;
    }


    public override IEnumerator EndLogs()
    {
        return null;
    }
}

