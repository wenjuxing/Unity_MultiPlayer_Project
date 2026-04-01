using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Common;
using Serilog;
using Google.Protobuf;
namespace Summer.Network
{
    /// <summary>
    /// 负责监听TCP网络端口，异步接收Socket连接
    /// </summary>
    public class TcpServer
    {
        private IPEndPoint endPoint;
        private Socket serverSocket;    //服务端监听对象
        private int backlog=100;        //连接最大请求数

        public event EventHandler<Socket> SocketConnected; //客户端接入事件
        public delegate void  ConnectedCallback(Connection conn);
        public delegate void DataReceivedCallback(Connection conn, IMessage data);
        public delegate void DisconnectedCallback(Connection conn);

        public event ConnectedCallback Connected;
        public event DataReceivedCallback DataReceived;
        public event DisconnectedCallback Disconnected;
        public TcpServer(string host, int port)
        {
            endPoint = new IPEndPoint(IPAddress.Parse(host), port);
        }
        //初始化参数列表的作用是在调用这个构造函数的同时调用另一个构造函数实现参数的初始化
        public TcpServer(string host, int port, int backlog) : this(host,port)
        {
            this.backlog = backlog;
        }

        public void Start()
        {
            if (!IsRunning)
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(endPoint);
                serverSocket.Listen(backlog); //设置监听等待队列的最大请求数，如果客户端发起连接请求时，服务器来不及处理，则加入等待队列
                Log.Information("开始监听端口：" + endPoint.Port);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                args.Completed += OnAccept; //当有人连入的时候
                serverSocket.AcceptAsync(args);
                
            }
            else
            {
                Log.Information("TcpServer is working");
            }
        }

        private void OnAccept(object sender, SocketAsyncEventArgs e)
        {
            //连入的人
            Socket client = e.AcceptSocket;
            //继续接收下一位
            e.AcceptSocket = null;
            serverSocket.AcceptAsync(e);

            //真的有人连进来
            if (e.SocketError == SocketError.Success)
            {
                if (client != null)
                {
                    OnSocketConneced(client);
                }
            }
        }
        private void OnSocketConneced(Socket socket)
        {
            SocketConnected?.Invoke(this, socket);
            Connection conn = new Connection(socket);
            conn.OnDataReceived += (cnn, data) => { DataReceived?.Invoke(cnn, data); };
            conn.OnDisconnected += (cnn) => { Disconnected?.Invoke(cnn); };
            Connected?.Invoke(conn);
        }
        public bool IsRunning
        {
            get { return serverSocket != null; }
        }
        public void Stop()
        {
            if (serverSocket == null)
                return;
            serverSocket.Close();
            serverSocket = null;
        }

    }
}
