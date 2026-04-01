using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using Network;
using Proto;

namespace GameServer.Network
{
    /// <summary>
    /// 客户端网络连接
    /// 职责：发送消息 接收消息 关闭连接 断开通知
    /// </summary>
    public class NetConnection
    {
        public delegate void DataReceivedCallback(NetConnection sender,byte[] data);
        public delegate void DisconnectedCallback(NetConnection sender);

        public Socket socket;

        private DataReceivedCallback dataReceivedCallback;
        private DisconnectedCallback disconnectedCallback;
        public NetConnection(Socket socket,DataReceivedCallback cb1,DisconnectedCallback cb2)
        {
            this.socket = socket;
            this.dataReceivedCallback = cb1;
            this.disconnectedCallback = cb2;

            //创建解码器
            LengthFieldDecoder lfd = new LengthFieldDecoder(socket, 64 * 1024, 0, 4, 0, 4);
            //解码信息后的回调事件
            lfd.dataReceivedHandler += OnDataReceived;
            //解码中断开连接的回调
            lfd.disconnectedHandler += ()=> disconnectedCallback?.Invoke(this);
            //启动解码器
            lfd.Start();
        }

        /// <summary>
        /// 接收客户端信息后的回调函数
        /// </summary>
        /// <param name="data"></param>
        private void OnDataReceived(object sender, byte[] data)
        {
            dataReceivedCallback?.Invoke(this,data);
        }
        /// <summary>
        /// 主动关闭连接
        /// </summary>
        public void Close()
        {
            try
            {
                //断开发送和接收信息
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {

            }
            //关闭
            socket.Close();
            socket = null;
            disconnectedCallback?.Invoke(this);
        }
    }

}
