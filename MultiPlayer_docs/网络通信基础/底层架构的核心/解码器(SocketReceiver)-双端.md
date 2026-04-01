- 解码器主要通过Buffer缓冲区接受数据，读取原始的TCP字节流处理分包粘包，并把解析后的包体（序列号+消息体）传递给Connection会话，让回话层去解析出具体的protobuf数据；

### 接收数据

通过BeginReceive()非阻塞方法接收数据并存储在缓冲区中(字节数组)并获取本次收到数据的长度，然后立刻返回继续接收数据，并把开始解析数据；

### 解析数据(处理分包/粘包)

```csharp
 private void doReceive(int len)
        {
            //解析数据
            int remain = startIndex + len;
            int offset = 0;
            while (remain > 4)
            {
                int msgLen = GetInt32BE(buffer, offset);
                if (remain < msgLen + 4)
                {
                    break;
                }
                //解析消息
                byte[] data = new byte[msgLen];
                Array.Copy(buffer, offset + 4, data, 0, msgLen);
                //解析消息
                try { DataReceived?.Invoke(data); } catch { Log.Debug("消息解析异常"); }
                
                offset += msgLen + 4;
                remain -= msgLen + 4;
            }      
            if (remain > 0)
            {
                Array.Copy(buffer, offset, buffer, 0, remain);
            }

            startIndex = remain;
        }
```

1. 通过开始索引加上数据长度开始在缓冲区读取数据；
2. 先读取前4个字节获取包体长度，如果分包的话，就会出现整个数据包的长度都没有包体长度大，剩下的数据还没送到；如果粘包的话，读取完包体长度数据包还会剩下没有读完；
3. 每次读取完都会判断剩余部分是否大于四个字节（包体长度），如果大于的话则继续读取；