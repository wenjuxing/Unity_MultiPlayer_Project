
- 消息分发器对接会话(Connection)，由会话通过AddMessage()方法添加反序列化后的protobuf数据到消息分发器的消息队列中，消息分发器通过开启多线程异步分发给业务处理；

![[Pasted image 20260329104637.png|467]]

#### 核心数据结构

- 消息队列：存储所有客户端发送待处理的消息；
- 订阅字典：Key为消息类型全名，Value为处理的委托；
- 线程信号量：AutoResetEvent类型，无消息时通过WaitOne()让线程休眠,有消息时通过Set()
```csharp
        //通过Set每次可以唤醒1个线程
        AutoResetEvent threadEvent = new AutoResetEvent(true); 
        // 消息队列，所有客户端发来的消息都暂存在这里
        private Queue<Msg> messageQueue = new Queue<Msg>();
        // 消息处理器(委托)
        public delegate void MessageHandler<T>(Connection sender, T msg);
        // 频道字典（订阅记录）
        private Dictionary<string, Delegate> delegateMap = new Dictionary<string, Delegate>();
```

#### 消息的订阅与执行

##### 订阅消息(Subscribe)

```
public void Subscribe<T>(MessageHandler<T> handler) where T : Google.Protobuf.IMessage
        {
            string type = typeof(T).FullName;
            if (!delegateMap.ContainsKey(type))
            {
                delegateMap[type] = null;
            }
            delegateMap[type] = (MessageHandler<T>)delegateMap[type] + handler;
            Log.Debug(type+":"+delegateMap[type].GetInvocationList().Length);
        }
```

- 先通过反射typeof(T)获取消息的全名，再在消息全名作为Key，把传递的函数作为Value存入字典中；

##### 退订消息(Off)

```
ublic void Off<T>(MessageHandler<T> handler) where T : Google.Protobuf.IMessage
        {
            string key = typeof(T).FullName;
            if (!delegateMap.ContainsKey(key))
            {
                delegateMap[key] = null;
            }
            delegateMap[key] = (MessageHandler<T>)delegateMap[key] - handler;
        }
```

- 先通过反射typeof(T)获取消息的全名，再在消息全名作为Key从字典中查找对应的委托，再移除订阅的事件；

##### 触发消息(Fire)

```
private void Fire<T>(Connection sender, T msg)
        {
            string type = typeof(T).FullName;
            if (delegateMap.ContainsKey(type))
            {
                MessageHandler<T> handler = (MessageHandler<T>)delegateMap[type];
                try
                {
                    handler?.Invoke(sender, msg);
                }
                catch(Exception e)
                {
                    Log.Error("MessageRouter.Fire error:" + e.StackTrace);
                }
                
            }
        }
```

先通过反射typeof(T).FullName方法获取消息类型名称，再到字典中通过ContainsKey(type)查找，如果找到则执行事件；

#### 核心功能模块

##### 添加消息(AddMessage)

```
public void AddMessage(Connection sender, Google.Protobuf.IMessage message)
        {
            lock (messageQueue)
            {
                messageQueue.Enqueue(new Msg() { sender = sender, message = message });
            }
            threadEvent.Set(); //唤醒1个worker
        }
```

- 通过会话(Connection)调用AddMessage()方法添加消息进入消息队列，添加消息时通过Lock语句给消息队列加锁，确保同一时间内只有一个线程访问消息队列，并唤醒一个线程去处理收到的消息；

##### 处理消息(MessageWork)

```
         while (_running)
                {
                    if (messageQueue.Count == 0)
                    {
                        threadEvent.WaitOne(); //可以通过Set()唤醒
                        continue;
                    }
                    //从消息队列取出一个元素
                    Msg msg = null;
                    lock (messageQueue)
                    {
                        if (messageQueue.Count==0) continue;
                        msg = messageQueue.Dequeue();
                    }
                    Google.Protobuf.IMessage package = msg.message;
                    if(package != null)
                    {
                        executeMessage(msg.sender, package);
                    }
                }
```

1. 在While循环中先判断消息队列中元素的数量是否为0，是的话则通过WaitOne()方法使得线程进入休眠状态；
2. 通过Lock()语句给消息队列加锁，确保线程安全，再从消息队列中取出一个消息执行Fire()方法触发订阅事件；
3. 继续判断消息队列是否还有元素，有的话继续取出元素；
