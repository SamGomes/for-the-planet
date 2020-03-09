using Thalamus;
using ForThePlanetParallelScreens;



public class ThalamusConnector : ThalamusClient, IGMTablets
{
    public IThalamusTabletPublisher TypifiedPublisher {  get;  private set; }
    public UnityConnector UnityConnector { private get; set; }



    public class ThalamusPublisher : IThalamusTabletPublisher
    {
        private readonly dynamic _publisher;
        public ThalamusPublisher(dynamic publisher)
        {
            _publisher = publisher;
        }

        public void Dispose()
        {
            _publisher.Dispose();
        }

        public void ConnectToGM(string id, string name)
        {
            _publisher.ConnectToGM(id, name);
        }

        public void SendBudgetAllocation(int economyAllocation, int environmentAllocation)
        {
            _publisher.SendBudgetAllocation(economyAllocation, environmentAllocation);
        }
    }

    public ThalamusConnector(string clientName, string character)
        : base(clientName, character)
    {
        SetPublisher<IThalamusTabletPublisher>();
        TypifiedPublisher = new ThalamusPublisher(Publisher);
    }

    public override void Dispose()
    {
        UnityConnector.Dispose();
        base.Dispose();
    }

    public void AllConnected(string p0Id, string p0Name, string p1Id, string p1Name, string p2Id, string p2Name)
    {
        UnityConnector.RPCProxy.AllConnected(p0Id, p0Name, p1Id, p1Name, p2Id, p2Name);
    }
}
