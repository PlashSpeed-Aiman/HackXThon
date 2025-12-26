using System;
using System.Collections.Generic;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Services;

/// <summary>
/// Simple service locator for dependency injection.
/// Provides a centralized place to register and resolve service instances.
/// </summary>
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> Services = new();

    /// <summary>
    /// Initializes the service locator and registers all application services.
    /// Call this once at application startup.
    /// </summary>
    public static void Initialize()
    {
        // Register ViewModels
        RegisterSingleton(new MainWindowViewModel());

        // Register additional services here as your application grows
        // Example: RegisterSingleton<IMyService>(new MyService());
    }

    /// <summary>
    /// Registers a singleton service instance.
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <param name="instance">The service instance</param>
    public static void RegisterSingleton<T>(T instance) where T : class
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));

        Services[typeof(T)] = instance;
    }

    /// <summary>
    /// Gets a registered service instance.
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not registered</exception>
    public static T GetService<T>() where T : class
    {
        var type = typeof(T);
        if (Services.TryGetValue(type, out var service))
        {
            return (T)service;
        }

        throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
    }

    /// <summary>
    /// Checks if a service is registered.
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>True if the service is registered, false otherwise</returns>
    public static bool IsRegistered<T>() where T : class
    {
        return Services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// Clears all registered services. Primarily used for testing.
    /// </summary>
    public static void Clear()
    {
        Services.Clear();
    }
}
