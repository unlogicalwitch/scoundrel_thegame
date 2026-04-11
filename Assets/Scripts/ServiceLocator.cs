using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Minimal service locator. Register services in Bootstrap,
/// retrieve them anywhere with ServiceLocator.Get<T>().
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    public static void Register<T>(T service) where T : class
    {
        services[typeof(T)] = service;
        Debug.Log($"[ServiceLocator] Registered {typeof(T).Name}");
    }

    public static T Get<T>() where T : class
    {
        if (services.TryGetValue(typeof(T), out var service))
            return (T)service;

        Debug.LogError($"[ServiceLocator] Service {typeof(T).Name} not found. Back to bootstrap...");
        SceneManager.LoadScene("Bootstrap");
        return null;
    }

    public static void Clear() => services.Clear();
}

