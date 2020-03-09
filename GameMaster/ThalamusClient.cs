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

        public void AllConnected(string p0Id, string p0Name, string p1Id, string p1Name, string p2Id, string p2Name)
        {
            this.publisher.AllConnected(p0Id, p0Name, p1Id, p1Name, p2Id, p2Name);
        }
    }

    private GameMasterPublisher _gameMaster;
    private List<TabletPlayer> _tabletsConnected;
    private bool _connectedToMaster;


    public GameMasterThalamusClient(string clientName, string characterName)
        : base(clientName, characterName)
    {
        SetPublisher<IGameMasterPublisher>();
        _gameMaster = new GameMasterPublisher(base.Publisher);
        _tabletsConnected = new List<TabletPlayer>();
        _connectedToMaster = false;
    }

    public override void ConnectedToMaster()
    {
        _connectedToMaster = true;
    }


    public void ConnectToGM(string id, string name)
    {
        _tabletsConnected.Add(new TabletPlayer(id, name));
        Console.WriteLine("Received a connection from Tablet");

        if (_tabletsConnected.Count == 3)
        {
            Console.WriteLine("Sleeping for 3s to make sure the tablets load MainScene and GameManager is not null...");
            Thread.Sleep(3000);
            TabletPlayer p0 = _tabletsConnected[0];
            TabletPlayer p1 = _tabletsConnected[1];
            TabletPlayer p2 = _tabletsConnected[2];
            _gameMaster.AllConnected(p0.ID, p0.Name, p1.ID, p1.Name, p2.ID, p2.Name);
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

            if (input == "q")
            {
                stop = true;
            }
        }
    }
}
