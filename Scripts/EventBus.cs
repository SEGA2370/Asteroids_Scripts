using System;
using System.Collections.Generic;

public class EventBus : SingletonMonoBehaviour<EventBus>
{
    private readonly Dictionary<Type, Delegate> _eventDictionary = new();
    private readonly object _lock = new(); // Lock object for thread safety

    /// <summary>
    /// Subscribes a listener to the specified event type.
    /// </summary>
    public void Subscribe<T>(Action<T> listener)
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));

        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType] = null;
            }

            _eventDictionary[eventType] = Delegate.Combine(_eventDictionary[eventType], listener);
        }
    }

    /// <summary>
    /// Unsubscribes a listener from the specified event type.
    /// </summary>
    public void Unsubscribe<T>(Action<T> listener)
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));

        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_eventDictionary.TryGetValue(eventType, out var currentDelegate)) return;

            currentDelegate = Delegate.Remove(currentDelegate, listener);

            if (currentDelegate == null)
            {
                _eventDictionary.Remove(eventType);
            }
            else
            {
                _eventDictionary[eventType] = currentDelegate;
            }
        }
    }

    /// <summary>
    /// Raises an event of the specified type with the given arguments.
    /// </summary>
    public void Raise<T>(T eventArgs)
    {
        var eventType = typeof(T);

        Delegate currentDelegate;
        lock (_lock)
        {
            if (!_eventDictionary.TryGetValue(eventType, out currentDelegate)) return;
        }

        if (currentDelegate is Action<T> action)
        {
            action.Invoke(eventArgs);
        }
        else
        {
            throw new InvalidOperationException($"Event type mismatch for {eventType}. Expected Action<{eventType.Name}>.");
        }
    }

    /// <summary>
    /// Clears all event subscriptions. Use cautiously.
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _eventDictionary.Clear();
        }
    }
}
