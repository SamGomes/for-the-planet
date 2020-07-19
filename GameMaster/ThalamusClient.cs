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

        public void AllConnected(int p0Id, string p0Name, int p1Id, string p1Name, int p2Id, string p2Name)
        {
            this.publisher.AllConnected(p0Id, p0Name, p1Id, p1Name, p2Id, p2Name);
        }

        public void FinishRound(int[] envAllocations)
        {
            this.publisher.FinishRound(envAllocations);
        }
    }

    private GameMasterPublisher _gameMaster;
    private TabletPlayer[] _tabletsConnected;
    private bool _connectedToMaster;
    private int _currentRound;
    private int _totalInvestmentsCurrentRound;


    public GameMasterThalamusClient(string clientName, string characterName)
        : base(clientName, characterName)
    {
        SetPublisher<IGameMasterPublisher>();
        _gameMaster = new GameMasterPublisher(base.Publisher);
        TabletPlayer p0 = new TabletPlayer(0);
        TabletPlayer p1 = new TabletPlayer(1);
        TabletPlayer p2 = new TabletPlayer(2);
        _tabletsConnected = new TabletPlayer[] { p0, p1, p2};
        _connectedToMaster = false;
        _currentRound = 0;
        _totalInvestmentsCurrentRound = 0;
    }

    public override void ConnectedToMaster()
    {
        _connectedToMaster = true;
    }


    public void ConnectToGM(int id, string name)
    {
        _tabletsConnected[id].Name = name;
        _tabletsConnected[id].Connected = true;
        Console.WriteLine("Received a connection from Tablet");

        if (_tabletsConnected[0].Connected & _tabletsConnected[1].Connected & _tabletsConnected[2].Connected)
        {
            Console.WriteLine("Sleeping for 3s to make sure the tablets load MainScene and GameManager is not null...");
            Thread.Sleep(3000);
            TabletPlayer p0 = _tabletsConnected[0];
            TabletPlayer p1 = _tabletsConnected[1];
            TabletPlayer p2 = _tabletsConnected[2];
            _gameMaster.AllConnected(p0.ID, p0.Name, p1.ID, p1.Name, p2.ID, p2.Name);
        }
    }


    public void SendBudgetAllocation(int tabletID, int envAllocation)
    {
        _tabletsConnected[tabletID].EnvInvestments.Add(envAllocation);
        _totalInvestmentsCurrentRound++;

        if (_totalInvestmentsCurrentRound == 3)
        {
            _totalInvestmentsCurrentRound = 0;

            int[] currentRoundInvestments = new int[3] { 0, 0, 0};
            for (int i = 0; i <_tabletsConnected.Length; i++)
            {
                TabletPlayer p = _tabletsConnected[i];
                int roundInvestment = p.EnvInvestments[_currentRound];
                currentRoundInvestments[i] = roundInvestment;
            }
            _currentRound++;
            _gameMaster.FinishRound(currentRoundInvestments);
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
                Console.WriteLine("Quiting...");
                stop = true;
            }
            if (input == "r")
            {
                Console.WriteLine("RESET! Waiting for 3 tablets...");
                TabletPlayer p0 = new TabletPlayer(0);
                TabletPlayer p1 = new TabletPlayer(1);
                TabletPlayer p2 = new TabletPlayer(2);
                _tabletsConnected = new TabletPlayer[] { p0, p1, p2 };
                _currentRound = 0;
                _totalInvestmentsCurrentRound = 0;
            }
        }
    }

    public void Disconnect(int id)
    {
        Console.WriteLine("RESET! Waiting for 3 tablets...");
        TabletPlayer p0 = new TabletPlayer(0);
        TabletPlayer p1 = new TabletPlayer(1);
        TabletPlayer p2 = new TabletPlayer(2);
        _tabletsConnected = new TabletPlayer[] { p0, p1, p2 };
        _currentRound = 0;
        _totalInvestmentsCurrentRound = 0;
    }
}
