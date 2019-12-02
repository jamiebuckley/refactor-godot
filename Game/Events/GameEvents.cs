using System;
using System.Collections.Generic;
using System.Linq;

namespace Refactor1.Game.Events
{
    public class GameEvents
    {
        public static readonly GameEvents Instance = new GameEvents();
        
        private int id = 0;
        
        private readonly Dictionary<Type, List<Action<GameEvent>>> _listeners = new Dictionary<Type, List<Action<GameEvent>>>();
        
        public Action SubscribeTo(Type eventName, Action<GameEvent> listener)
        {
            if (!_listeners.ContainsKey(eventName)) _listeners[eventName] = new List<Action<GameEvent>>();
            _listeners[eventName].Add(listener);
            return () => _listeners[eventName].Remove(listener);
        }

        public void Emit<T>(T gameEvent) where T : GameEvent
        {
            if (!_listeners.ContainsKey(typeof(T))) return;
            var listeners = _listeners[typeof(T)];
            listeners?.ForEach(l => l.Invoke(gameEvent));
        }
    }
}