using System;
using System.Collections.Generic;

namespace Project.Core
{
    public interface IGameEvent { }

    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static void Subscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                _subscribers[type] = list;
            }
            list.Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler) where T : IGameEvent
        {
            var type = typeof(T);
            if (_subscribers.TryGetValue(type, out var list))
            {
                list.Remove(handler);
            }
        }

        public static void Publish<T>(T gameEvent) where T : IGameEvent
        {
            var type = typeof(T);
            if (!_subscribers.TryGetValue(type, out var list)) return;

            var snapshot = list.ToArray();
            foreach (var del in snapshot)
            {
                (del as Action<T>)?.Invoke(gameEvent);
            }
        }
        public static void Clear()
        {
            _subscribers.Clear();
        }
    }
}