using Serilog;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Summer
{
    /// <summary>
    /// 中心计时器
    /// </summary>
    public class Scheduler : Singleton<Scheduler>
    {
        //任务列表
        private List<Task> tasks = new List<Task>();
        //
        private ConcurrentQueue<Task> _addQueue = new ConcurrentQueue<Task>();
        private ConcurrentQueue<Action> _removeQueue = new ConcurrentQueue<Action>();

        private int fps = 50; // 每秒帧数

        public Scheduler()
        {

        }


        Timer timer;
        public void Start()
        {
            if (timer != null) return;
            timer = new Timer(new TimerCallback(Execute), null, 0, 1);
        }

        public void Stop()
        {
            timer.Dispose();
            timer = null;
        }


        public void AddTask(Action taskMethod, float seconds, int repeatCount = 0)
        {
            this.AddTask(taskMethod, (int)(seconds * 1000), TimeUnit.Milliseconds, repeatCount);
        }

        public void AddTask(Action taskMethod, int timeValue, TimeUnit timeUnit, int repeatCount = 0)
        {
            int interval = GetInterval(timeValue, timeUnit);
            long startTime = UnixTime + interval;
            Task task = new Task(taskMethod, startTime, interval, repeatCount);
            _addQueue.Enqueue(task);
        }

        public void AddTask(Action taskMethod, float delay, float interval, int repeatCount = 0)
        {
            int _interval = (int)(interval * 1000);
            long startTime = UnixTime + (long)(delay * 1000);
            Task task = new Task(taskMethod, startTime, _interval, repeatCount);
            _addQueue.Enqueue(task);
        }

        public void RemoveTask(Action taskMethod)
        {
            _removeQueue.Enqueue(taskMethod);
        }
        /// <summary>
        /// 每帧都会执行
        /// </summary>
        /// <param name="action"></param>
        public void Update(Action action)
        {
            Task task = new Task(action, 0, 0, 0);
            _addQueue.Enqueue(task);
        }

        //下一帧执行的时间
        private long _next = 0;

        /// <summary>
        /// 计时器主循环
        /// </summary>
        private void Execute(object state)
        {
            // tick间隔
            int interval = 1000 / fps;
            long time = UnixTime;
            if (time < _next) return;
            _next = time + interval;

            Time.Tick();

            //处理逻辑帧
            lock (tasks)
            {
                //移除队列
                while (_removeQueue.TryDequeue(out var item))
                {
                    tasks.RemoveAll(task => task.TaskMethod == item);
                }
                // 移除完毕的任务
                tasks.RemoveAll(task => task.Completed);
                // 添加队列任务
                while (_addQueue.TryDequeue(out var item))
                {
                    tasks.Add(item);
                }
                // 执行任务
                foreach (Task task in tasks)
                {
                    if (task.ShouldRun())
                    {
                        task.Run();
                    }
                }
            }

        }


        // 获取从1970年1月1日午夜（也称为UNIX纪元）到现在的毫秒数
        public static long UnixTime { get => DateTimeOffset.Now.ToUnixTimeMilliseconds(); }

        private int GetInterval(int timeValue, TimeUnit timeUnit)
        {
            switch (timeUnit)
            {
                case TimeUnit.Milliseconds:
                    return timeValue;
                case TimeUnit.Seconds:
                    return timeValue * 1000;
                case TimeUnit.Minutes:
                    return timeValue * 1000 * 60;
                case TimeUnit.Hours:
                    return timeValue * 1000 * 60 * 60;
                case TimeUnit.Days:
                    return timeValue * 1000 * 60 * 60 * 24;
                default:
                    throw new ArgumentException("Invalid time unit.");
            }
        }

        private class Task
        {
            public Action TaskMethod { get; }
            public long StartTime { get; }
            public long Interval { get; }
            public int RepeatCount { get; }

            private int currentCount;

            private long lastTick = 0; //上一次执行开始的时间

            public bool Completed = false; //是否已经执行完毕

            public Task(Action taskMethod, long startTime, long interval, int repeatCount)
            {
                TaskMethod = taskMethod;
                StartTime = startTime;
                Interval = interval;
                RepeatCount = repeatCount;
                currentCount = 0;
            }

            public bool ShouldRun()
            {
                if (currentCount == RepeatCount && RepeatCount != 0)
                {
                    Log.Information("RepeatCount={0}", RepeatCount);
                    return false;
                }

                long now = UnixTime;
                if (now >= StartTime && (now - lastTick) >= Interval)
                {
                    return true;
                }

                return false;
            }

            public void Run()
            {
                lastTick = UnixTime;
                try
                {
                    TaskMethod.Invoke();
                }
                catch (Exception ex)
                {
                    Log.Error("Scheduler has Error:{0}", ex.Message);
                    //return;
                }

                currentCount++;

                if (currentCount == RepeatCount && RepeatCount != 0)
                {
                    Console.WriteLine("Task completed.");
                    Completed = true;
                }
            }
        }
    }

    public enum TimeUnit
    {
        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days
    }

    public class Time
    {
        //游戏开始的时间戳
        private static long startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        /// <summary>
        /// 游戏的运行时间（秒）
        /// </summary>
        public static float time { get; private set; }
        /// <summary>
        /// 获取上一帧运行所用的时间（秒）
        /// </summary>
        public static float deltaTime { get; private set; }

        // 记录最后一次tick的时间
        private static long lastTick = 0;

        /// <summary>
        /// 由Schedule调用，请不要自行调用，除非你知道自己在做什么！！！
        /// </summary>
        public static void Tick()
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            time = (now - startTime) * 0.001f;
            if (lastTick == 0) lastTick = now;
            deltaTime = (now - lastTick) * 0.001f;
            lastTick = now;
        }
    }
}