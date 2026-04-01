### 监听连接

```csharp
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
```

- 通过Start()方法创建套接字Socket，通过Bind()绑定IP地址和端口号，并使用Listen()方法开始监听；
- 在Listen中设置最大连接请求等待数，当客户端请求连接时，服务器来不及处理，则会把连接请求放入等待队列；
### 持续监听连接

```csharp
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
```

1. 调用AcceptAsync()异步接受连接方法后立即返回，由操作系统内核等待连接；

2. 当有连接时会保存客户端的Socket，然后继续监听下一个连接，如果连接成功的话回调连接成功事件并返回Connection回话；
### 工作流程

![[Pasted image 20260328112303.png]]