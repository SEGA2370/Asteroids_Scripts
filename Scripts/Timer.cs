using System;

public abstract class Timer
{
    protected float InitialTime;

    protected float Time { get; set; }

    public bool IsRunning;

    public float Progress => InitialTime > 0 ? Time / InitialTime : 0;

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };

    protected Timer()
    {
        IsRunning = false;
    }

    public void SetInitialTime(float value)
    {
        if (value < 0)
            throw new ArgumentException("InitialTime cannot be negative.");

        InitialTime = value;
    }

    public void Start(float? initialTime = null)
    {
        if (initialTime.HasValue)
        {
            SetInitialTime(initialTime.Value);
        }

        Time = InitialTime;
        if (IsRunning) return;

        IsRunning = true;
        OnTimerStart.Invoke();
    }

    public void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        Time = 0;
        OnTimerStop.Invoke();
    }

    public void Resume()
    {
        if (!IsRunning && Time > 0)
        {
            IsRunning = true;
        }
    }

    public void Pause()
    {
        if (IsRunning)
        {
            IsRunning = false;
        }
    }

    public virtual void Reset()
    {
        Time = InitialTime;
        IsRunning = false;
    }

    public abstract void Tick(float deltaTime);
}

public class CountdownTimer : Timer
{
    public override void Tick(float deltaTime)
    {
        if (!IsRunning) return;

        Time = Math.Max(Time - deltaTime, 0);
        if (Time <= 0)
        {
            Stop();
        }
    }

    public bool IsFinished => Time <= 0;

    public override void Reset()
    {
        base.Reset();
    }

    public void Reset(float newTime)
    {
        SetInitialTime(newTime);
        Reset();
    }
}

public class StopwatchTimer : Timer
{
    public override void Tick(float deltaTime)
    {
        if (IsRunning)
        {
            Time += deltaTime;
        }
    }

    public override void Reset()
    {
        Time = 0;
        IsRunning = false;
    }

    public float GetTime() => Time;
}
