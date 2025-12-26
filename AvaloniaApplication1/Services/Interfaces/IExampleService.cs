using System.Threading.Tasks;

namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Example service interface demonstrating the service contract pattern.
/// This serves as a template for creating new service interfaces in your application.
/// </summary>
/// <remarks>
/// Service interfaces define the contract that implementations must follow.
/// This enables:
/// - Loose coupling between components
/// - Easy testing with mock implementations
/// - Flexibility to swap implementations
///
/// To use this pattern:
/// 1. Define your service interface in Services/Interfaces/
/// 2. Create implementation in Services/Implementation/
/// 3. Register in ServiceLocator.Initialize()
/// 4. Inject via constructor or resolve via ServiceLocator.GetService()
/// </remarks>
public interface IExampleService
{
    /// <summary>
    /// Example synchronous method.
    /// </summary>
    /// <param name="input">Input parameter</param>
    /// <returns>Result string</returns>
    string DoSomething(string input);

    /// <summary>
    /// Example asynchronous method.
    /// Use async methods for I/O operations, network calls, or long-running tasks.
    /// </summary>
    /// <param name="input">Input parameter</param>
    /// <returns>A task representing the asynchronous operation with a string result</returns>
    Task<string> DoSomethingAsync(string input);

    /// <summary>
    /// Example property showing read-only state.
    /// </summary>
    bool IsReady { get; }
}
