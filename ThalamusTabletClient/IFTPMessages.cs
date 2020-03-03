using System;
using CookComputing.XmlRpc;
using Thalamus;

public interface IThalamusTabletPublisher : IThalamusPublisher, ITabletPublisher
{
}

public interface ITabletPublisher : IAction
{
    void SendA();
}

public interface IUnityTabletSubscriber : ITabletPublisher
{
    void Dispose();

    [XmlRpcMethod]
    new void SendA();

}

public interface IThalamusTabletSubscriber: IPerception
{
    void ReceiveZ();
}


public interface IUnityTabletPublisher : IThalamusTabletSubscriber, IXmlRpcProxy
{
    [XmlRpcMethod]
    new void ReceiveZ();
}
