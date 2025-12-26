using System;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services.Interfaces;
using Google.GenAI;

namespace AvaloniaApplication1.Services;

public class ConfessionService : IConfessionService
{
    private readonly Client? _client;
    private readonly string? _apiKey;

    public ConfessionService()
    {
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _client = new Client(apiKey: _apiKey);
        }
    }

    public async Task<ConfessionResponse> GetAbsolutionAsync(string confession, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return ConfessionResponse.CreateError("API key not found. Please set the GEMINI_API_KEY environment variable.");
        }

        if (string.IsNullOrWhiteSpace(confession))
        {
            return ConfessionResponse.CreateError("Please confess your coding sin.");
        }

        if (_client == null)
        {
            return ConfessionResponse.CreateError("Failed to initialize Gemini client.");
        }

        try
        {
            var prompt = $@"You are a wise, humorous zen coding monk who absolves developers of their programming sins.
A developer has confessed: ""{confession}""

Respond with a short, witty, monk-style absolution (2-3 sentences). Be understanding, slightly humorous, and wise.
Use phrases like 'My child', 'Fear not', 'The path to enlightenment', etc. Offer gentle absolution and encouragement.";

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: prompt
            ).ConfigureAwait(false);

            if (response?.Candidates != null &&
                response.Candidates.Count > 0 &&
                response.Candidates[0].Content?.Parts != null &&
                response.Candidates[0].Content.Parts.Count > 0)
            {
                var absolution = response.Candidates[0].Content.Parts[0].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(absolution))
                {
                    return ConfessionResponse.CreateSuccess(absolution);
                }
            }

            return ConfessionResponse.CreateError("The monk is meditating. Please try again.");
        }
        catch (UnauthorizedAccessException)
        {
            return ConfessionResponse.CreateError("Invalid API key. Please check your GEMINI_API_KEY environment variable.");
        }
        catch (TaskCanceledException)
        {
            return ConfessionResponse.CreateError("Request timed out. Please try again.");
        }
        catch (OperationCanceledException)
        {
            return ConfessionResponse.CreateError("Operation cancelled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting absolution: {ex.Message}");
            return ConfessionResponse.CreateError($"Error: {ex.Message}");
        }
    }
}
