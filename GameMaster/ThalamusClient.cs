using System;
using Thalamus;
using ForThePlanetParallelScreens;


public interface IGameMasterPublisher : IThalamusPublisher, IGMTablets { }


class GameMasterThalamusClient : ThalamusClient, ITabletsGM
{
    public class GameMasterPublisher : IGameMasterPublisher
    {
        dynamic publisher;
        public GameMasterPublisher(dynamic publisher)
        {
            this.publisher = publisher;
        }

        public void ReceiveZ()
        {
            this.publisher.ReceiveZ();
        }
    }

    public GameMasterPublisher GameMaster;


    public GameMasterThalamusClient()
        : base("GameMastrer", "filipa")
    {
        SetPublisher<IGameMasterPublisher>();
        GameMaster = new GameMasterPublisher(base.Publisher);
    }


    public void SendA()
    {
        //Console.WriteLine("Received A from Tablet");
    }
}
