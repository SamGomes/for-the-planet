using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CallMongoLogServer : MonoBehaviour{

    private MonoBehaviour monoBehaviourObject;

    public void InitLogs(MonoBehaviour monoBehaviourObject)
    {
        this.monoBehaviourObject = monoBehaviourObject;
    }

    public void SentLog(Dictionary<string, string> argsNValues)
    {
        StartCoroutine(WriteLog(argsNValues["currSessionId"], argsNValues["currGameId"], argsNValues["generation"], argsNValues["playerName"],argsNValues["playerType"], argsNValues["playerTookFromCP"], argsNValues["playerGain"], argsNValues["envState"]));
    }

    public IEnumerator WriteLog(string currSessionId, string currGameId, string generation, string playerName, string playerType, string playerTookFromCP,
        string playerGain, string envState)
    {
            WWWForm form = new WWWForm();
            form.AddField("currSessionId", currSessionId);
            form.AddField("currGameId", currGameId);
            form.AddField("generation", generation);
            form.AddField("playerName", playerName);
            form.AddField("playerType", playerType);
            form.AddField("playerTookFromCP", playerTookFromCP);
            form.AddField("playerGain", playerGain);
            form.AddField("envState", envState);

            UnityWebRequest www = UnityWebRequest.Post("https://mongodbserverlog.herokuapp.com/fortheplanet", form);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }

    }
