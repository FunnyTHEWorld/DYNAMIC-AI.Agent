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
            },
            generationConfig = new
            {
                thinkingConfig = new
                {
                    includeThoughts = true
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
                var responseContentBuilder = new StringBuilder();
                string? thinkingContent = null;

                foreach (var part in parts.EnumerateArray())
                {
                    var text = part.GetProperty("text").GetString() ?? "";
                    // This is a guess. I need to confirm the actual structure of the "thinking" content.
                    if (part.TryGetProperty("role", out var role) && role.GetString() == "tool")
                    {
                        thinkingContent = text;
                    }
                    else
                    {
                        responseContentBuilder.Append(text);
                    }
                }

                var responseContent = responseContentBuilder.ToString();

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
                    ThinkingContent = thinkingContent,
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

    public async IAsyncEnumerable<ChatResponse> GetChatResponseStreamAsync(string prompt, GeminiSettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.ApiKey) || string.IsNullOrEmpty(settings.Model))
        {
            yield return new ChatResponse { Content = "API Key or Model not set. Please configure them in the settings page." };
            yield break;
        }

        var baseUrl = settings.BaseUrl ?? "https://generativelanguage.googleapis.com";
        var url = $"{baseUrl}/v1beta/models/{settings.Model}:streamGenerateContent?key={settings.ApiKey}";

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
            },
            generationConfig = new
            {
                thinkingConfig = new
                {
                    includeThoughts = true
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new System.IO.StreamReader(stream);

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (line != null && line.StartsWith("data: "))
                {
                    var jsonData = line.Substring(6);
                    using (JsonDocument doc = JsonDocument.Parse(jsonData))
                    {
                        JsonElement root = doc.RootElement;
                        JsonElement candidates = root.GetProperty("candidates");
                        JsonElement firstCandidate = candidates[0];
                        JsonElement contentElement = firstCandidate.GetProperty("content");
                        JsonElement parts = contentElement.GetProperty("parts");
                        var responseContentBuilder = new StringBuilder();
                        string? thinkingContent = null;

                        foreach (var part in parts.EnumerateArray())
                        {
                            var text = part.GetProperty("text").GetString() ?? "";
                            // This is a guess. I need to confirm the actual structure of the "thinking" content.
                            if (part.TryGetProperty("role", out var role) && role.GetString() == "tool")
                            {
                                thinkingContent = text;
                            }
                            else
                            {
                                responseContentBuilder.Append(text);
                            }
                        }

                        var responseContent = responseContentBuilder.ToString();

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

                        yield return new ChatResponse
                        {
                            Content = responseContent,
                            ThinkingContent = thinkingContent,
                            PromptTokenCount = promptTokenCount,
                            CandidatesTokenCount = candidatesTokenCount
                        };
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            yield return new ChatResponse { Content = $"Error calling Gemini API: {ex.Message}" };
        }
    }
}
