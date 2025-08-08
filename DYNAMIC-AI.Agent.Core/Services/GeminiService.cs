using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DYNAMIC_AI.Agent.Core.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Models;

namespace DYNAMIC_AI.Agent.Core.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;

    public GeminiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<ChatResponse> GetChatResponseAsync(string prompt, GeminiSettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.ApiKey) || string.IsNullOrEmpty(settings.Model))
        {
            return new ChatResponse { Content = "API Key or Model not set. Please configure them in the settings page." };
        }

        var baseUrl = settings.BaseUrl ?? "https://generativelanguage.googleapis.com";
        var url = $"{baseUrl}/v1beta/models/{settings.Model}:generateContent?key={settings.ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            using (JsonDocument doc = JsonDocument.Parse(responseJson))
            {
                JsonElement root = doc.RootElement;
                JsonElement candidates = root.GetProperty("candidates");
                JsonElement firstCandidate = candidates[0];
                JsonElement contentElement = firstCandidate.GetProperty("content");
                JsonElement parts = contentElement.GetProperty("parts");
                JsonElement firstPart = parts[0];
                JsonElement text = firstPart.GetProperty("text");
                var responseContent = text.GetString() ?? "No response from API.";

                var promptTokenCount = 0;
                var candidatesTokenCount = 0;
                if (root.TryGetProperty("usageMetadata", out var usageMetadata))
                {
                    if (usageMetadata.TryGetProperty("promptTokenCount", out var promptTokenCountElement))
                    {
                        promptTokenCount = promptTokenCountElement.GetInt32();
                    }
                    if (usageMetadata.TryGetProperty("candidatesTokenCount", out var candidatesTokenCountElement))
                    {
                        candidatesTokenCount = candidatesTokenCountElement.GetInt32();
                    }
                }

                return new ChatResponse
                {
                    Content = responseContent,
                    PromptTokenCount = promptTokenCount,
                    CandidatesTokenCount = candidatesTokenCount
                };
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            return new ChatResponse { Content = $"Error calling Gemini API: {ex.Message}" };
        }
    }
}
