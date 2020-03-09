using UnityEngine;

public class TabletPlayer : Player
{
    private TabletThalamusConnector thalamusConnector;


    public TabletPlayer(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
        : base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {
        thalamusConnector = new TabletThalamusConnector(int.Parse(GameGlobals.thalamusClientPort));
    }



    public void ConnectToGameMaster()
    {
        thalamusConnector.ConnectToGM(GameGlobals.tabletID, GameGlobals.participantName);
    }

    public void Dispose()
    {
        thalamusConnector.Dispose();
    }

}

public class RemotePlayer : Player
{
    public RemotePlayer(string type, GameObject playerCanvas, PopupScreenFunctionalities warningScreenRef, Sprite UIAvatar, int id, string name)
        : base(type, playerCanvas, warningScreenRef, UIAvatar, id, name)
    {

    }

}
