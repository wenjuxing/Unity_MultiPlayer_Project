- 通常情况下在多线程中是无法执行Unity相关的API，但是可以通过在多线程中添加需要执行Unity脚本装进委托当中，然后在一个继承自MonoBehavior的脚本中调用这个委托；

```csharp
    //定义一个只读队列用于存储委托
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

	public void Update() {
		lock(_executionQueue) {
			while (_executionQueue.Count > 0) {
				_executionQueue.Dequeue().Invoke();
			}
		}
	}
```
- 通过队列存储委托，而委托中又存储着需要执行的Unity脚本，然后在Update函数中取出存在队列中委托，执行回调事件；
- 从队列中取出委托时需要加锁保证线程安全；