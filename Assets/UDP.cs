using System;
using UnityEngine;

#if WINDOWS_UWP
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
using Windows.Storage.Streams;
#else
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
#endif

public class ReceivedEventArgs : EventArgs
{
    public string msg;
}

public delegate void ReceivedEventHandler(object sender, ReceivedEventArgs e);

public class UDP : MonoBehaviour
{

    //Parameter
    #region
#if WINDOWS_UWP
    int listenPort = 3333;
    DatagramSocket socket;
    string msg = "NO DATA";
    public event ReceivedEventHandler OnReceivedEventHandler;
    ReceivedEventArgs rcv_args = new ReceivedEventArgs();
#else
    int listenPort = 3333;
    bool isFinished = false;
    Thread thread;
    UdpClient udpClient_;
    IPEndPoint endPoint_;
    string msg = "NO DATA";
    public event ReceivedEventHandler OnReceivedEventHandler;
    ReceivedEventArgs rcv_args = new ReceivedEventArgs();
#endif
    #endregion

    //Initialize UDP Client
    #region
#if WINDOWS_UWP
    public void Init(){    
            socket = new DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
        }

        public void Init(int _listenPort){
            socket = new DatagramSocket();
            socket.MessageReceived += Socket_MessageReceived;
            listenPort = _listenPort;
        }
#else
    public void Init()
    {
        udpClient_ = new UdpClient(listenPort);
        udpClient_.Client.Blocking = false;
    }

    public void Init(int _listenPort)
    {
        listenPort = _listenPort;
        udpClient_ = new UdpClient(listenPort);
        udpClient_.Client.Blocking = false;
    }
#endif
    #endregion

    //Start UDP Client
    #region
#if WINDOWS_UWP
        public async void Connect(){
            try
            {
                await socket.BindEndpointAsync(null, listenPort.ToString());
                isConnected = true;            
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                Debug.Log(SocketError.GetStatus(e.HResult).ToString());

                isConnected = false;
                return;
            }
        }
#else
    public void Connect()
    {
        thread = new Thread(new ThreadStart(ReceiveMessage));
        thread.Start();
        Debug.Log("Start Connection");
    }
#endif

    #endregion

    //Stop UDP Client
    #region
#if WINDOWS_UWP
    public void Disconnect(){
        if(socket != null){
                socket.Dispose();
            }
    } 
#else
    public void Disconnect()
    {
        if (udpClient_ != null)
        {
            udpClient_.Close();
            udpClient_ = null;
        }

        isFinished = true;

        if (thread != null)
        {
            thread.Abort();
        }
    }
#endif

    #endregion

    //Receive Message
    #region
#if WINDOWS_UWP
    public async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
        {
            Stream streamIn = args.GetDataStream().AsStreamForRead();
            StreamReader reader = new StreamReader(streamIn);
            string message = await reader.ReadLineAsync();
            msg = message;

            rcv_args.msg = msg;

            if (OnReceivedEventHandler !=null) {
               OnReceivedEventHandler(this, rcv_args);
            }
        }
#else
    private void ReceiveMessage()
    {
        while (true)
        {
            if (isFinished) break;
            udpClient_.Client.Blocking = true;

            try
            {
                endPoint_ = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] data = udpClient_.Receive(ref endPoint_);
                string text = Encoding.ASCII.GetString(data);

                if (text.Length > 0)
                {
                    msg = text;
                    rcv_args.msg = msg;
                    if (OnReceivedEventHandler != null) OnReceivedEventHandler(this, rcv_args);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
#endif
    #endregion

    //Get Data
    #region
    public string GetData()
    {
#if WINDOWS_UWP
        return msg;
#else
        return msg;
#endif
    }
    #endregion

    void OnApplicationQuit()
    {
        Disconnect();
    }
}