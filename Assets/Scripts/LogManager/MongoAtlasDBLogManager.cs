using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using UnityEngine;
using UnityEngine.Networking;





//Debug log manager
public class MongoAtlasLogManager : LogManager
{
    private MonoBehaviour monoBehaviourObject;
    private MongoClient client;

    
//    public static List<T> GetItems<T>(MongoCollection collection, string queryString, string orderString) where T : class 
//    {
//        var queryDoc = BsonSerializer.Deserialize<BsonDocument>(queryString);
//        var orderDoc = BsonSerializer.Deserialize<BsonDocument>(orderString);
//
//        var cursor = collection.Find(query);
//        cursor.SetSortOrder(order);
//
//        return cursor.ToList();
//    }
    
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
        yield return null;
    }

    public override IEnumerator GetFromLog(string database, string table, List<string> query, Func<string, int> yieldedReactionToGet)
    {
        var databaseObj = client.GetDatabase(database);
        var collection = databaseObj.GetCollection<BsonDocument>(table);
        var allElements = collection.Find(query[0]).ToList();

        if (query[1] == "desc")
        {
            allElements.Reverse();
        }

        string[] cropLims = query[2].Split(',');
        allElements = allElements.GetRange(Int32.Parse(cropLims[0]), Int32.Parse(cropLims[1]));
        
//        yield return yieldedReactionToGet(allElements.ToJson());
        yield return yieldedReactionToGet("[]");
    }

    public override IEnumerator UpdateLog(string database, string table, List<string> query, Dictionary<string, string> argsNValues)
    {
        var databaseObj = client.GetDatabase(database);
        return null;
    }


    public override IEnumerator EndLogs()
    {
        return null;
    }
}

