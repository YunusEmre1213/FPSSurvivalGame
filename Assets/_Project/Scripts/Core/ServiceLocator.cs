using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core
{
    public class ServiceLocator
    {
        private static ServiceLocator _instance;
        public static ServiceLocator Instance => _instance ??= new ServiceLocator();

        private readonly Dictionary<Type, IGameService> _services = new Dictionary<Type, IGameService>();

        public void Register<T>(T service) where T : IGameService
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] {type.Name} zaten kayitli, uzerine yaziliyor.");
            }
            _services[type] = service;
            service.Initialize();
        }

        public T Get<T>() where T : IGameService
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new InvalidOperationException(
                $"[ServiceLocator] {type.Name} kayitli degil. Bootstrapper'da Register edildi mi kontrol et.");
        }

        public void ShutdownAll()
        {
            foreach (var service in _services.Values)
            {
                service.Shutdown();
            }
            _services.Clear();
        }
    }
}