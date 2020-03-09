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
    [XmlRpcMethod]
    new void SendBudgetAllocation(int economyAllocation, int environmentAllocation);

}

public interface IUnityTabletPublisher : IGMTablets, IXmlRpcProxy
{
    [XmlRpcMethod]
    new void AllConnected(string p0Id, string p0Name, string p1Id, string p1Name, string p2Id, string p2Name);
}
