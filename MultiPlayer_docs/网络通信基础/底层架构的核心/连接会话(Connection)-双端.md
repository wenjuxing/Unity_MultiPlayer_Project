- Connection相当于一个中间桥梁向下对接底层解码器（处理分包粘包）接受解析后的消息，向上对接消息分发器，把解析后的消息传递给消息分发器处理；

### 添加解析后的protobuf数据到消息分发器

```csharp
private void _received(byte[] data)
        {
            //获取消息序列号
            ushort code = GetUShort(data, 0);
            var msg = ProtoHelper.ParseFrom(code, data, 2, data.Length - 2);

            if (MessageRouter.Instance.Running)
            {
                MessageRouter.Instance.AddMessage(this,msg);
            }

            OnDataReceived?.Invoke(this, msg);
        }
```

- 接收到解码器解析出来的二进制数组后，去掉序列号(2byte)，反序列化protobuf添加到消息分发器中；
### 发送消息

```csharp
 public void Send(IMessage message)
        {
            Log.Debug($"发送消息:{message}");
            using(var ds = DataStream.Allocate())
            {
                int code = ProtoHelper.SeqCode(message.GetType());
                ds.WriteInt(message.CalculateSize()+2);
                ds.WriteUShort((ushort)code);
                message.WriteTo(ds);
                this.SocketSend(ds.ToArray());
            }
            
        }
```

1. 从对象池中获取自定义的数据流；
2. 写入长度（消息体大小+序列号大小）(4byte)；
3. 写入序列号（2byte）；
4. 写入protobuf数据；
5. 检查是否大端模式，然后发送消息；


