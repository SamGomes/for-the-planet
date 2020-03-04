using UnityEngine;

public class TabletPlayer : Player
{
    private TabletThalamusConnector thalamusConnector;


    public TabletPlayer(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
        : base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        thalamusConnector = new TabletThalamusConnector(7000);
    }

    public void ConnectToGameMaster()
    {
        thalamusConnector.SendA();
    }

}

public class RemotePlayer : Player
{
    public RemotePlayer(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
        : base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {

    }

}
