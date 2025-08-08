using System;
using System.Collections.Generic;
using System.IO;
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

    public async Task<string> GetChatResponseAsync(string prompt, string? attachmentPath, GeminiSettings settings)
    {
        if (settings == null || string.IsNullOrEmpty(settings.ApiKey) || string.IsNullOrEmpty(settings.Model))
        {
            return "API Key or Model not set. Please configure them in the settings page.";
        }

        var baseUrl = settings.BaseUrl ?? "https://generativelanguage.googleapis.com";
        var url = $"{baseUrl}/v1beta/models/{settings.Model}:generateContent?key={settings.ApiKey}";

        var parts = new List<object> { new { text = prompt } };

        if (!string.IsNullOrEmpty(attachmentPath))
        {
            try
            {
                var fileBytes = await File.ReadAllBytesAsync(attachmentPath);
                var base64String = Convert.ToBase64String(fileBytes);
                var mimeType = GetMimeType(attachmentPath);

                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = mimeType,
                        data = base64String
                    }
                });
            }
            catch (Exception ex)
            {
                // Handle file read errors
                return $"Error processing attachment: {ex.Message}";
            }
        }

        var requestBody = new
        {
            contents = new[]
            {
                new { parts = parts.ToArray() }
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
                JsonElement partsElement = contentElement.GetProperty("parts");
                JsonElement firstPart = partsElement[0];
                JsonElement text = firstPart.GetProperty("text");
                return text.GetString() ?? "No response from API.";
            }
        }
        catch (Exception ex)
        {
            return $"Error calling Gemini API: {ex.Message}";
        }
    }

    private string GetMimeType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => "application/octet-stream",// Default MIME type
        };
    }
}
