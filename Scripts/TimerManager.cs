using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class TimerManager : SingletonMonoBehaviour<TimerManager>
{
    readonly Dictionary<Type, ObjectPool<Timer>> _pools = new();
    readonly List<Timer> _timers = new();

    public Timer CreateTimer<T>(float value = 0f)
    {
        var pool = GetTimerPool<T>();
        var timer = pool.Get();
        timer.SetInitialTime(value);
        _timers.Add(timer);
        return timer;
    }

    public void ReleaseTimer<T>(Timer timer)
    {
        if (timer == null) return;

        var pool = GetTimerPool<T>();
        if (_timers.Contains(timer)) _timers.Remove(timer);
        timer.Stop(); // Ensure timer is stopped before releasing
        pool.Release(timer);
    }

    IObjectPool<Timer> GetTimerPool<T>()
    {
        var type = typeof(T);
        if (!_pools.ContainsKey(type))
        {
            _pools[type] = new ObjectPool<Timer>(() =>
            {
                return (Timer)Activator.CreateInstance(typeof(T));
            });
        }

        return _pools[type];
    }

    void Update()
    {
        var timers = _timers.ToArray();
        foreach (var timer in timers)
        {
            timer.Tick(Time.deltaTime);
        }
    }
}