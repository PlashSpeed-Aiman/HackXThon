using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;

namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Service for generating haikus using Google's Gemini API.
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Generates a haiku based on the provided frustration text.
    /// </summary>
    /// <param name="frustration">The developer's frustration to convert into a haiku</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A response containing the generated haiku or error information</returns>
    Task<GeminiResponse> GenerateHaikuAsync(string frustration, CancellationToken cancellationToken = default);
}
