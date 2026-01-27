using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Microsoft.Extensions.Configuration;

namespace Explorer.Stakeholders.Core.UseCases;

public class AiChatService : IAiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AiChatService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["Groq:ApiKey"]!;

        if (string.IsNullOrEmpty(_apiKey))
            Console.WriteLine("⚠️ Groq API key nije postavljen u appsettings.json ili env!");
    }

    public async Task<string> AskAsync(string userMessage)
    {
        try
        {
            // Kreiramo chat-style request sa system + user porukama
            var messages = new object[]
            {
                new
                {
                    role = "system",
                    content = AppKnowledge.SystemPrompt
                },
                new
                {
                    role = "user",
                    content = userMessage
                }
            };

            var requestBody = new
            {
                model = "openai/gpt-oss-20b",
                messages = messages
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true }),
                Encoding.UTF8,
                "application/json"
            );

            Console.WriteLine($"[AiChatService] Sending to Groq chat/completions:\n{JsonSerializer.Serialize(requestBody, new JsonSerializerOptions { WriteIndented = true })}");

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[AiChatService] Groq response: {json}");

            if (!response.IsSuccessStatusCode)
            {
                return $"Greška od Groq API: {response.StatusCode}";
            }

            // Parsiranje direktnog odgovora iz chat/completions
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("choices", out var choices))
                return "AI nije vratio tekstualni odgovor.";

            var firstChoice = choices.EnumerateArray().FirstOrDefault();
            if (firstChoice.ValueKind == JsonValueKind.Undefined)
                return "AI nije vratio tekstualni odgovor.";

            if (!firstChoice.TryGetProperty("message", out var message))
                return "AI nije vratio tekstualni odgovor.";

            if (!message.TryGetProperty("content", out var content))
                return "AI nije vratio tekstualni odgovor.";

            return content.GetString()!;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AiChatService] Exception: {ex}");
            return "Došlo je do greške pri komunikaciji s AI. Pokušaj ponovo.";
        }
    }
}
