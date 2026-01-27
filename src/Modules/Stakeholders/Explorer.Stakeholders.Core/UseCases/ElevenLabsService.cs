using System.Text;
using System.Text.Json;
using Explorer.Stakeholders.API.Public;
using Microsoft.Extensions.Configuration;

namespace Explorer.Stakeholders.Core.UseCases;

public class ElevenLabsService : IElevenLabsService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ElevenLabsService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _apiKey = config["ElevenLabs:ApiKey"]!;
    }

    public async Task<byte[]> TextToSpeechAsync(string text)
    {
        try
        {
            var voiceId = "21m00Tcm4TlvDq8ikWAM"; // Rachel - engleski glas (najbolji besplatni)
                                                  // Za srpski probaj: "pNInz6obpgDQGcFmaJgB" (Adam)

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}"
            );

            request.Headers.Add("xi-api-key", _apiKey);

            var body = new
            {
                text = text,
                model_id = "eleven_multilingual_v2", // Podržava srpski
                voice_settings = new
                {
                    stability = 0.5,
                    similarity_boost = 0.75,
                    style = 0.0,
                    use_speaker_boost = true
                }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"ElevenLabs error: {response.StatusCode}");
                return Array.Empty<byte>();
            }

            return await response.Content.ReadAsByteArrayAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ElevenLabs exception: {ex}");
            return Array.Empty<byte>();
        }
    }
}
