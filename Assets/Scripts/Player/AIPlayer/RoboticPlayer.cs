using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoboticPlayer : EmotionalAIPlayer
{

    private ThalamusConnector thalamusConnector = null;

    public RoboticPlayer(IInteractionModule interactionModule, GameObject playerCanvas, MonoBehaviourFunctionalities playerMonoBehaviourFunctionalities, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name) :
        base(interactionModule, playerCanvas, playerMonoBehaviourFunctionalities, warningScreenRef, UIAvatar, id, name)
    {
        this.playerSelfDisablerUI.SetActive(true);
        this.InitThalamusConnectorOnPort(7000, name);
    }

    public void InitThalamusConnectorOnPort(int port, string name)
    {
        thalamusConnector = new ThalamusConnector(port);
        this.name = name;
    }

    public void PerformUtterance(string text, string[] tags, string[] values)
    {
        thalamusConnector.PerformUtterance(text, tags, values);
    }

    public void Speak(string text)
    {
       PerformUtterance(text, new string[] { }, new string[] { });
    }

    public void GazeAt(string target)
    {
        thalamusConnector.GazeAt(target);
    }

}