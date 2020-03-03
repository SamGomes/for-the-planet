using CookComputing.XmlRpc;


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

public interface ITabletPublisher
{
    [XmlRpcMethod]
    void SendA();
}

public interface IUnityTabletSubscriber
{
    [XmlRpcMethod]
    void ReceiveZ();
}