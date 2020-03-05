using System;
using Thalamus;
using ForThePlanetParallelScreens;
using System.Collections.Generic;
using System.Threading;

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

    private GameMasterPublisher _gameMaster;
    private List<int> _tabletsConnected;
    private bool _connectedToMaster;


    public GameMasterThalamusClient()
        : base("GameMastrer", "filipa")
    {
        SetPublisher<IGameMasterPublisher>();
        _gameMaster = new GameMasterPublisher(base.Publisher);
        _tabletsConnected = new List<int>();
        _connectedToMaster = false;
    }

    public override void ConnectedToMaster()
    {
        _connectedToMaster = true;
    }


    public void SendA()
    {
        _tabletsConnected.Add(1);
        Console.WriteLine("Received A from Tablet");

        if (_tabletsConnected.Count == 3)
        {
            Console.WriteLine("Sleeping for 3s to make sure the tablets load MainScene and GameManager is not null...");
            Thread.Sleep(3000);
            _gameMaster.ReceiveZ();
        }
    }


    internal void SessionWithTablets()
    {
        while (!_connectedToMaster) { }

        Console.WriteLine("Waiting for 3 tablets...");

        bool stop = false;
        while (!stop)
        {
            string input = Console.ReadLine();

            if (input == "z")
            {
                _gameMaster.ReceiveZ();
            }
            else if (input == "q")
            {
                stop = true;
            }
        }
    }
}
