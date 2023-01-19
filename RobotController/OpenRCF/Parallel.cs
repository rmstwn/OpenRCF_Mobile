using System;
using System.Reflection;
using System.Windows.Threading;
using System.Threading;

namespace OpenRCF
{       
    public static class Parallel
    {       
        private static Task[] task = new Task[10];

        static Parallel()
        {
            for(int i = 0; i < task.Length; i++)
            {
                task[i] = new Task();
            }
        }

        public static void Run(Action timerEvent, uint timeOutMs, uint timeSpanMs)
        {
            for (int i = 0; i < task.Length; i++)
            {
                if (task[i].IsRunning == false)
                {
                    task[i].TimeOutUs = 1000 * timeOutMs;
                    task[i].IsStop = () => false;
                    task[i].TimeSpanUs = 1000 * timeSpanMs;                    
                    task[i].Run(timerEvent);
                    return;
                }                
            }
           
            Console.WriteLine("Error : Maximum number of Parallel.Run() is " + task.Length);
        }

        public static void Run(Action timerEvent, Func<bool> isStop, uint timeSpanMs)
        {
            for (int i = 0; i < task.Length; i++)
            {
                if (task[i].IsRunning == false)
                {
                    task[i].TimeOutUs = long.MaxValue;
                    task[i].IsStop = isStop;
                    task[i].TimeSpanUs = 1000 * timeSpanMs;
                    task[i].Run(timerEvent);
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of Parallel.Run() is " + task.Length);
        }

        public static void Run(Func<bool> timerEvent, uint timeSpanMs)
        {
            for (int i = 0; i < task.Length; i++)
            {
                if (task[i].IsRunning == false)
                {
                    task[i].TimeOutUs = long.MaxValue;
                    task[i].IsStop = () => false;
                    task[i].TimeSpanUs = 1000 * timeSpanMs;
                    task[i].Run(timerEvent);
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of Parallel.Run() is " + task.Length);
        }

        public static void RunEndless(Action timerEvent, uint timeSpanMs)
        {
            for (int i = 0; i < task.Length; i++)
            {
                if (task[i].IsRunning == false)
                {
                    task[i].TimeOutUs = long.MaxValue;
                    task[i].IsStop = () => false;
                    task[i].TimeSpanUs = 1000 * timeSpanMs;
                    task[i].Run(timerEvent);
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of Parallel.Run() is " + task.Length);
        }

        public static int RunningLoopNum
        {
            get 
            {
                int result = 0;
  
                for (int i = 0; i < task.Length; i++)
                {
                    if (task[i].IsRunning) result++;
                }

                return result; 
            }
        }

        private class Task
        {
            private TimeStanp timeStanp = new TimeStanp();
            public bool IsRunning = false;
            public long TimeOutUs = 5000;
            public long TimeSpanUs = 1000;            
            public Func<bool> IsStop = () => false;
            private int sleepTimeMs;

            public void Run(Action timerEvent)
            {
                IsRunning = true;
                uint count = 0;                
                timeStanp.Set();
                
                System.Threading.Tasks.Task.Run(() =>
                {                    
                    while (true)
                    {
                        if (count * TimeSpanUs <= timeStanp.ElapsedTimeUs)
                        {
                            if (TimeOutUs <= timeStanp.ElapsedTimeUs)
                            {
                                break;
                            }
                            else if (IsStop())
                            {
                                break;
                            }
                            else
                            {
                                timerEvent();
                                count++;
                                sleepTimeMs = (int)((count * TimeSpanUs - timeStanp.ElapsedTimeUs) / 1000);
                                if (0 < sleepTimeMs) Thread.Sleep(sleepTimeMs - 1);
                            }
                        }                                        
                    }

                    IsRunning = false;
                });
            }

            public void Run(Func<bool> timerEvent)
            {
                IsRunning = true;
                uint count = 0;
                timeStanp.Set();

                System.Threading.Tasks.Task.Run(() =>
                {
                    while (true)
                    {
                        if (count * TimeSpanUs <= timeStanp.ElapsedTimeUs)
                        {
                            if (timerEvent())
                            {
                                count++;
                                sleepTimeMs = (int)((count * TimeSpanUs - timeStanp.ElapsedTimeUs) / 1000);
                                if (0 < sleepTimeMs) Thread.Sleep(sleepTimeMs - 1);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    IsRunning = false;
                });
            }

            private class TimeStanp
            {
                private long timeStanp = DateTime.Now.Ticks;

                public void Set()
                {
                    timeStanp = DateTime.Now.Ticks;
                }

                public long ElapsedTimeUs
                {          
                    get { return (long)((DateTime.Now.Ticks - timeStanp) / 10); }
                }
            }
        }
        
    }

    public static class ParallelUI
    {
        private static EventTimer[] timer = new EventTimer[10];
       
        static ParallelUI()
        {
            for (int i = 0; i < timer.Length; i++)
            {
                timer[i] = new EventTimer();
            }
        }

        public static void Run(Action timerEvent, uint timeOutMs, uint timeSpanMs)
        {
            for (int i = 0; i < timer.Length; i++)
            {
                if (timer[i].IsEnabled == false)
                {
                    timer[i].EventHandler = (sender, e) =>
                    {
                        if (timer[i].TimeStanp.ElapsedTimeMs < timeOutMs) timerEvent();
                        else timer[i].Stop();
                    };
                    timer[i].Interval = TimeSpan.FromMilliseconds(timeSpanMs);
                    timer[i].TimeStanp.Set();
                    timer[i].Start();
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of ParallelUI.Run() is " + timer.Length);
        }

        public static void Run(Action timerEvent, Func<bool> isStop, uint timeSpanMs)
        {
            for (int i = 0; i < timer.Length; i++)
            {
                if (timer[i].IsEnabled == false)
                {
                    timer[i].EventHandler = (sender, e) =>
                    {
                        if (isStop()) timer[i].Stop(); 
                        else timerEvent();
                    };
                    timer[i].Interval = TimeSpan.FromMilliseconds(timeSpanMs);
                    timer[i].Start();
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of ParallelUI.Run() is " + timer.Length);
        }

        public static void Run(Func<bool> timerEvent, uint timeSpanMs)
        {
            for (int i = 0; i < timer.Length; i++)
            {
                if (timer[i].IsEnabled == false)
                {
                    timer[i].EventHandler = (sender, e) => { if (timerEvent() == false) timer[i].Stop(); };
                    timer[i].Interval = TimeSpan.FromMilliseconds(timeSpanMs);
                    timer[i].Start();
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of ParallelUI.Run() is " + timer.Length);
        }

        public static void RunEndless(Action timerEvent, uint timeSpanMs)
        {
            for (int i = 0; i < timer.Length; i++)
            {
                if (timer[i].IsEnabled == false)
                {
                    timer[i].EventHandler = (sender, e) => { timerEvent(); };
                    timer[i].Interval = TimeSpan.FromMilliseconds(timeSpanMs);
                    timer[i].Start();
                    return;
                }
            }

            Console.WriteLine("Error : Maximum number of ParallelUI.Run() is " + timer.Length);
        }

        private class TimeStanp
        {
            private long timeStanp = DateTime.Now.Ticks;

            public void Set()
            {
                timeStanp = DateTime.Now.Ticks;
            }

            public long ElapsedTimeMs
            {
                get { return (long)((DateTime.Now.Ticks - timeStanp) / 10000); }
            }
        }

        private class EventTimer : DispatcherTimer
        {
            private EventHandler eventHandler;           
            public TimeStanp TimeStanp = new TimeStanp();

            public EventTimer() : base(DispatcherPriority.Normal)
            {
                Tick += eventHandler;
            }

            public EventHandler EventHandler
            {
                set
                {
                    Tick -= eventHandler;
                    eventHandler = value;
                    Tick += eventHandler;
                }
            }
        }
        
    }
   
    public static class Timer
    {        
        private static long[] timeLabel = new long[20];
     
        static Timer()
        {
            long timeStanp = DateTime.Now.Ticks;

            for (int i = 0; i < timeLabel.Length; i++)
            {
                timeLabel[i] = timeStanp;
            }
        }

        public static void SetTimeLabel(uint labelNum)
        {
            if (labelNum < timeLabel.Length) timeLabel[labelNum] = DateTime.Now.Ticks;
            else
            {
                Console.WriteLine("Error : " + MethodBase.GetCurrentMethod().Name);
                Console.WriteLine("The maximum number of Time Labels is " + timeLabel.Length);
            }
        }

        public static long GetElapsedTimeMs(uint labelNum)
        {
            return (DateTime.Now.Ticks - timeLabel[labelNum]) / 10000;
        }

        public static float GetElapsedTimeSec(uint labelNum)
        {
            return (DateTime.Now.Ticks - timeLabel[labelNum]) / 10000000f;
        }

    }
}
