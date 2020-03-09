using CookComputing.XmlRpc;
using ForThePlanetParallelScreens;


//this file contains the thalamus interfaces necessary for "for the planet"


//this is a dummy interface
public interface IThalamusReceivingInterface
{
    [XmlRpcMethod]
    void ReceiveInformation();
}

public interface IRobotMessagesRpc : IXmlRpcProxy, IRobotMessages { }
public interface IRobotMessages
{
    void Dispose();

    [XmlRpcMethod]
    void PerformUtterance(string utterance, string[] tags, string[] tagsValues);

    [XmlRpcMethod]
    void GazeAt(string target);

    [XmlRpcMethod]
    void GlanceAt(string target);

}


public interface IUnityTabletPublisher : IXmlRpcProxy, ITabletPublisher { }

public interface ITabletPublisher : ITabletsGM
{
    [XmlRpcMethod]
    new void ConnectToGM(string id, string name);
    [XmlRpcMethod]
    new void SendBudgetAllocation(int economyAllocation, int environmentAllocation);
}

public interface IUnityTabletSubscriber : IGMTablets
{
    [XmlRpcMethod]
    new void AllConnected(string p0Id, string p0Name, string p1Id, string p1Name, string p2Id, string p2Name);
    [XmlRpcMethod]
    new void FinishRound(int[] envAllocations);
}