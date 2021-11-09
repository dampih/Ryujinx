using Ryujinx.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Ryujinx.HLE.HOS.Kernel.Common
{
    class KTimeManager : IDisposable
    {
        public static readonly long DefaultTimeIncrementNanoSeconds = ConvertGuestTicksToNanoSeconds(2);

        private class WaitingObject
        {
            public IKFutureSchedulerObject Object { get; }
            public long TimePoint { get; }

            public WaitingObject(IKFutureSchedulerObject schedulerObj, long timePoint)
            {
                Object    = schedulerObj;
                TimePoint = timePoint;
            }
        }

        private readonly KernelContext _context;
        private readonly List<WaitingObject> _waitingObjects;
        private AutoResetEvent _waitEvent;
        private bool _keepRunning;

        public KTimeManager(KernelContext context)
        {
            _context = context;
            _waitingObjects = new List<WaitingObject>();
            _keepRunning = true;

            Thread work = new Thread(WaitAndCheckScheduledObjects)
            {
                Name = "HLE.TimeManager"
            };

            work.Start();
        }

        public void ScheduleFutureInvocation(IKFutureSchedulerObject schedulerObj, long timeout)
        {
            long timePoint = PerformanceCounter.ElapsedTicks + ConvertNanosecondsToHostTicks(timeout);

            lock (_context.CriticalSection.Lock)
            {
                _waitingObjects.Add(new WaitingObject(schedulerObj, timePoint));
            }

            _waitEvent.Set();
        }

        public void UnscheduleFutureInvocation(IKFutureSchedulerObject schedulerObj)
        {
            lock (_context.CriticalSection.Lock)
            {
                _waitingObjects.RemoveAll(x => x.Object == schedulerObj);
            }
        }

        private void WaitAndCheckScheduledObjects()
        {
            using (_waitEvent = new AutoResetEvent(false))
            {
                while (_keepRunning)
                {
                    WaitingObject next;

                    lock (_context.CriticalSection.Lock)
                    {
                        next = _waitingObjects.OrderBy(x => x.TimePoint).FirstOrDefault();
                    }

                    if (next != null)
                    {
                        long timePoint = PerformanceCounter.ElapsedTicks;

                        if (next.TimePoint > timePoint)
                        {
                            int ms = (int)((next.TimePoint - timePoint) / PerformanceCounter.TicksPerMillisecond);

                            if (ms > 0)
                            {
                                _waitEvent.WaitOne(ms);
                            }
                        }

                        bool timeUp = PerformanceCounter.ElapsedTicks >= next.TimePoint;

                        if (timeUp)
                        {
                            lock (_context.CriticalSection.Lock)
                            {
                                if (_waitingObjects.Remove(next))
                                {
                                    next.Object.TimeUp();
                                }
                            }
                        }
                    }
                    else
                    {
                        _waitEvent.WaitOne();
                    }
                }
            }
        }

        public static long ConvertNanosecondsToMilliseconds(long time)
        {
            time /= 1000000;

            if ((ulong)time > int.MaxValue)
            {
                return int.MaxValue;
            }

            return time;
        }

        public static long ConvertMillisecondsToNanoseconds(long time)
        {
            return time * 1000000;
        }

        public static long ConvertNanosecondsToHostTicks(long ns)
        {
            long nsDiv = ns / 1000000000;
            long nsMod = ns % 1000000000;
            long tickDiv = PerformanceCounter.TicksPerSecond / 1000000000;
            long tickMod = PerformanceCounter.TicksPerSecond % 1000000000;

            long baseTicks = (nsMod * tickMod + PerformanceCounter.TicksPerSecond - 1) / 1000000000;
            return (nsDiv * tickDiv) * 1000000000 + nsDiv * tickMod + nsMod * tickDiv + baseTicks;
        }

        public static long ConvertGuestTicksToNanoSeconds(long ticks)
        {
            return (long)Math.Ceiling(ticks * (1000000000.0 / 19200000.0));
        }

        public static long ConvertHostTicksToTicks(long time)
        {
            return (long)((time / (double)PerformanceCounter.TicksPerSecond) * 19200000.0);
        }

        public void Dispose()
        {
            _keepRunning = false;
            _waitEvent?.Set();
        }
    }
}