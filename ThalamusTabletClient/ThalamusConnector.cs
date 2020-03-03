using System.Threading;
using Thalamus;



public class ThalamusConnector : ThalamusClient, IThalamusTabletSubscriber
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

        public void SendA()
        {
            _publisher.SendA();
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

    public void ReceiveZ()
    {
        UnityConnector.RPCProxy.ReceiveZ();
    }
}
