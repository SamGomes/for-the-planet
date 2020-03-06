using Thalamus;
using CookComputing.XmlRpc;
using ForThePlanetParallelScreens;

public interface IThalamusTabletPublisher : IThalamusPublisher, ITabletsGM
{
}


public interface IUnityTabletSubscriber : ITabletsGM
{
    void Dispose();

    [XmlRpcMethod]
    new void ConnectToGM(string id, string name);

}

public interface IUnityTabletPublisher : IGMTablets, IXmlRpcProxy
{
    [XmlRpcMethod]
    new void AllConnected();
}
