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

    public void AllConnected()
    {
        UnityConnector.RPCProxy.AllConnected();
    }
}
