using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Debug log manager
public class SilentLogManager : LogManager
{
    private int currI = 0;
    public override void InitLogs(MonoBehaviour monoBehaviourObject)
    {
    }
    public override IEnumerator WriteToLog(string database, string table, Dictionary<string, string> argsNValues) {
        yield return null;
    }


    public override IEnumerator GetFromLog(string database, string table, string query, Func<string, int> yieldedReactionToGet)
    {
        List<SessionParameterization> possibleParams = GameProperties.configurableProperties.possibleParameterizations;
        currI = (currI+1) % possibleParams.Count;
        yield return yieldedReactionToGet("[{ \"condition\" : \""+possibleParams[currI].id+"\" }]");
    }

    public override IEnumerator UpdateLog(string database, string table, string query, Dictionary<string, string> argsNValues)
    {
        yield return null;
    }

    public override IEnumerator EndLogs()
    {
        yield return null;
    }
}

