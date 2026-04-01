#### 连接服务器（ConnectToServer）

``` csharp
public static void ConnectToServer(string host, int port)
    {
        //服务器终端
        IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(host), port);
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect(ipe);
        Debug.Log("连接到服务端");
        conn = new Connection(socket);
        conn.OnDisconnected += OnDisconnected;
        //启动消息分发器
        MessageRouter.Instance.Start(1);
    }

```

- 先通过IPEndPoint编写IP地址和端口号，然后创建Socket，通过Connect()方法请求连接服务器，最后启动消息分发器开始接收服务端的消息；