using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CookComputing.XmlRpc;
using UnityEngine;

public abstract class ThalamusConnector
{
    protected string _remoteAddress = "";

    protected bool _printExceptions = true;
    public string RemoteAddress
    {
        get { return _remoteAddress; }
        set
        {
            _remoteAddress = value;
            _remoteUri = string.Format("http://{0}:{1}/", _remoteAddress, _remotePort);
            //_rpcProxy.Url = _remoteUri;
        }
    }

    protected int _remotePort = 7000;
    public int RemotePort
    {
        get { return _remotePort; }
        set
        {
            _remotePort = value;
            _remoteUri = string.Format("http://{0}:{1}/", _remoteAddress, _remotePort);
            //_rpcProxy.Url = _remoteUri;
        }
    }

    protected HttpListener _listener;
    protected bool _serviceRunning;
    protected int _localPort = 7001;
    protected bool _shutdown;
    List<HttpListenerContext> _httpRequestsQueue = new List<HttpListenerContext>();
    protected Thread _dispatcherThread;
    protected Thread _messageDispatcherThread;


    protected string _remoteUri = "";




    public class ThalamusDummyListener : XmlRpcListenerService, IThalamusReceivingInterface
    {
        public ThalamusDummyListener() { }

        public void ReceiveInformation()
        {
            //may need to receive external information
        }
    }

    public ThalamusConnector(int remotePort = 7000)
    {
        _remoteAddress = GameGlobals.thalamusClientIP;
        _remotePort = remotePort;
        _localPort = _remotePort + 1;
        _remoteUri = String.Format("http://{0}:{1}/", _remoteAddress, _remotePort);
        Debug.Log("Thalamus endpoint set to " + _remoteUri); 


        _dispatcherThread = new Thread(DispatcherThreadThalamus);
        _messageDispatcherThread = new Thread(MessageDispatcherThalamus);
        _dispatcherThread.Start();
        _messageDispatcherThread.Start();
    }

    #region rpc stuff

    public void Dispose()
    {
        _shutdown = true;

        try
        {
            if (_listener != null) _listener.Stop();
        }
        catch { }

        try
        {
            if (_dispatcherThread != null) _dispatcherThread.Join();
        }
        catch { }

        try
        {
            if (_messageDispatcherThread != null) _messageDispatcherThread.Join();
        }
        catch { }
    }

    public void DispatcherThreadThalamus()
    {
        while (!_serviceRunning)
        {
            try
            {
                Debug.Log("Attempt to start service on port '" + _localPort + "'");
                _listener = new HttpListener();
                _listener.Prefixes.Add(string.Format("http://*:{0}/", _localPort));
                _listener.Start();
                Debug.Log("XMLRPC Listening on " + string.Format("http://*:{0}/", _localPort));
                _serviceRunning = true;
            }
            catch (Exception e)
            {
                _localPort++;
                Debug.Log(e.Message);
                Debug.Log("Port unavaliable.");
                _serviceRunning = false;
            }
        }

        while (!_shutdown)
        {
            try
            {
                HttpListenerContext context = _listener.GetContext();
                lock (_httpRequestsQueue)
                {
                    _httpRequestsQueue.Add(context);
                }
            }
            catch (Exception e)
            {
                if (_printExceptions) Debug.Log("Exception: " + e);
                _serviceRunning = false;
                if (_listener != null)
                    _listener.Close();
            }
        }
        Debug.Log("Terminated DispatcherThreadThalamus");
        //_listener.Close();
    }

    public void MessageDispatcherThalamus()
    {
        while (!_shutdown)
        {
            bool performSleep = true;
            try
            {
                if (_httpRequestsQueue.Count > 0)
                {
                    performSleep = false;
                    List<HttpListenerContext> httpRequests;
                    lock (_httpRequestsQueue)
                    {
                        httpRequests = new List<HttpListenerContext>(_httpRequestsQueue);
                        _httpRequestsQueue.Clear();
                    }
                    foreach (HttpListenerContext r in httpRequests)
                    {
                        //ProcessRequest(r);
                        (new Thread(ProcessRequestThalamus)).Start(r);
                        performSleep = false;
                    }
                }
            }
            catch (Exception e)
            {
                if (_printExceptions) Debug.Log("Exception: " + e);
            }
            if (performSleep) Thread.Sleep(10);
        }
        Debug.Log("Terminated MessageDispatcherThalamus");
    }

    //this method should be overriden if you need the derived class to listen to thalamus methods
    public abstract void ProcessRequestThalamus(object oContext);
        /*
    {
        try
        {
            XmlRpcListenerService svc = new ThalamusDummyListener();
            svc.ProcessRequest((HttpListenerContext)oContext);
        }
        catch (Exception e)
        {
            if (_printExceptions) Debug.Log("Exception: " + e);
        }

    }*/

    #endregion

}


public class RobotThalamusConnector : ThalamusConnector, IRobotMessages
{

    protected IRobotMessagesRpc _rpcProxy;

    public RobotThalamusConnector(int remotePort = 7000) : base(remotePort)
    {
        _rpcProxy = XmlRpcProxyGen.Create<IRobotMessagesRpc>();
        _rpcProxy.Timeout = 5000;
        _rpcProxy.Url = _remoteUri;
    }

    public override void ProcessRequestThalamus(object oContext)
    {
        try
        {
            XmlRpcListenerService svc = new ThalamusDummyListener();
            svc.ProcessRequest((HttpListenerContext)oContext);
        }
        catch (Exception e)
        {
            if (_printExceptions) Debug.Log("Exception: " + e);
        }

    }

    public void PerformUtterance(string utterance, string[] tags, string[] tagsValues)
    {
        try
        {
            _rpcProxy.PerformUtterance(utterance, tags, tagsValues);
        }
        catch (Exception e)
        {
            if (_printExceptions) Debug.Log("Exception: " + e.Message + (e.InnerException != null ? ": " + e.InnerException : ""));
        }
    }

    public void GazeAt(string target)
    {
        try
        {
            _rpcProxy.GazeAt(target);
        }
        catch (Exception e)
        {
            if (_printExceptions) Debug.Log("Exception: " + e.Message + (e.InnerException != null ? ": " + e.InnerException : ""));
        }
    }

    public void GlanceAt(string target)
    {
        try
        {
            _rpcProxy.GlanceAt(target);
        }
        catch (Exception e)
        {
            if (_printExceptions) Debug.Log("Exception: " + e.Message + (e.InnerException != null ? ": " + e.InnerException : ""));
        }
    }
}


public class TabletThalamusConnector : ThalamusConnector, ITabletPublisher
{
    protected IUnityTabletPublisher _rpcProxy;

    public class UnityRPCListener : XmlRpcListenerService, IUnityTabletSubscriber
    {
        private TabletThalamusConnector _thalamusConnector;

        public UnityRPCListener(TabletThalamusConnector connectorRef)
        {
            _thalamusConnector = connectorRef;
        }

        public void AllConnected()
        {
            GameGlobals.gameManager.AllConnected();
        }
    }

    public TabletThalamusConnector(int remotePort = 7000) : base(remotePort)
    {
        _rpcProxy = XmlRpcProxyGen.Create<IUnityTabletPublisher>();
        _rpcProxy.Timeout = 5000;
        _rpcProxy.Url = _remoteUri;
    }

    public override void ProcessRequestThalamus(object oContext)
    {
        try
        {
            XmlRpcListenerService svc = new UnityRPCListener(this);
            svc.ProcessRequest((HttpListenerContext)oContext);
        }
        catch (Exception e)
        {
            if (_printExceptions) Debug.Log("Exception: " + e);
        }

    }

    public void ConnectToGM()
    {
        _rpcProxy.ConnectToGM();
    }
}
