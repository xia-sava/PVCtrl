using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace PVCtrl;

public static class TickService
{
    private static readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private static readonly List<Subscriber> _subscribers = [];

    public static void Start()
    {
        _timer.Tick += (_, _) => OnTick();
        _timer.Start();
    }

    public static void Stop() => _timer.Stop();

    private static void OnTick()
    {
        // イテレート中に変更される可能性があるのでコピー
        foreach (var sub in _subscribers.ToArray())
        {
            sub.Counter++;
            if (sub.Counter >= sub.IntervalSeconds)
            {
                sub.Counter = 0;
                sub.Callback();
            }
        }
    }

    public static IDisposable Subscribe(int intervalSeconds, Action callback)
    {
        var subscriber = new Subscriber(intervalSeconds, callback);
        _subscribers.Add(subscriber);
        return new Unsubscriber(() => _subscribers.Remove(subscriber));
    }

    private class Subscriber(int intervalSeconds, Action callback)
    {
        public int IntervalSeconds => intervalSeconds;
        public Action Callback => callback;
        public int Counter { get; set; }
    }

    private class Unsubscriber(Action unsubscribe) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            unsubscribe();
        }
    }
}
