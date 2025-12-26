using System;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services.Interfaces;
using Google.GenAI;

namespace AvaloniaApplication1.Services;

public class GeminiService : IGeminiService
{
    private readonly Client? _client;
    private readonly string? _apiKey;

    public GeminiService()
    {
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            _client = new Client(apiKey: _apiKey);
        }
    }

    public async Task<GeminiResponse> GenerateHaikuAsync(string frustration, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return GeminiResponse.CreateError("API key not found. Please set the GEMINI_API_KEY environment variable.");
        }

        if (string.IsNullOrWhiteSpace(frustration))
        {
            return GeminiResponse.CreateError("Please enter your frustration.");
        }

        if (_client == null)
        {
            return GeminiResponse.CreateError("Failed to initialize Gemini client.");
        }

        try
        {
            var prompt = $"You are a haiku poet specializing in developer frustrations. Convert the following frustration into a thoughtful, creative haiku (5-7-5 syllable format). Return only the haiku text, nothing else:\n\n{frustration}";

            var response = await _client.Models.GenerateContentAsync(
                model: "gemini-2.5-flash",
                contents: prompt
            ).ConfigureAwait(false);

            if (response?.Candidates != null &&
                response.Candidates.Count > 0 &&
                response.Candidates[0].Content?.Parts != null &&
                response.Candidates[0].Content.Parts.Count > 0)
            {
                var haiku = response.Candidates[0].Content.Parts[0].Text?.Trim();
                if (!string.IsNullOrWhiteSpace(haiku))
                {
                    return GeminiResponse.CreateSuccess(haiku);
                }
            }

            return GeminiResponse.CreateError("Failed to generate haiku. Please try again.");
        }
        catch (UnauthorizedAccessException)
        {
            return GeminiResponse.CreateError("Invalid API key. Please check your GEMINI_API_KEY environment variable.");
        }
        catch (TaskCanceledException)
        {
            return GeminiResponse.CreateError("Request timed out. Please try again.");
        }
        catch (OperationCanceledException)
        {
            return GeminiResponse.CreateError("Operation cancelled.");
        }
        catch (Exception ex)
        {
            // Log the detailed error for debugging
            Console.WriteLine($"Error generating haiku: {ex.Message}");
            return GeminiResponse.CreateError($"Error: {ex.Message}");
        }
    }
}
