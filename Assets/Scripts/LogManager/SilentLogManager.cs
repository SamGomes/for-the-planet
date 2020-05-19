using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Debug log manager
public class SilentLogManager : LogManager
{
    public override void InitLogs(MonoBehaviour monoBehaviourObject)
    {
    }
    public override IEnumerator WriteToLog(string database, string table, Dictionary<string, string> argsNValues) {
        yield return null;
    }


    public override IEnumerator GetFromLog(string database, string table, string query, Func<string, int> yieldedReactionToGet) {
        yield return yieldedReactionToGet("[]");
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

