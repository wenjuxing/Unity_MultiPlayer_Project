using System;
using System.Net.Sockets;
using System.Net;
using Proto;
using System.Text;
using Google.Protobuf;
using System.IO;
using Summer.Network;
using System.Threading;
using Common;
using Serilog;
using Summer;

namespace NetClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //初始化日志环境
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File("logs\\NetClient-log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();


            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 32510);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(iPEndPoint);
            //Log.Information("成功连接服务器");
           
            //用户登录消息包
            //Connection cnn=new Connection(socket);
            ///快捷发送
            

            //var msg2 = new GameEnterRequest();
            //cnn.Send(msg2);
            //Console.ReadLine();
        }
        private static void SendMessage(Socket socket, byte[] body)
        {
            int len = body.Length;
            byte[] bodylen = BitConverter.GetBytes(len);
            socket.Send(bodylen);
            socket.Send(body);
        }
    }
}
