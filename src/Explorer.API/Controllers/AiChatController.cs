using Explorer.Stakeholders.API.Public;
using Microsoft.AspNetCore.Mvc;

namespace Explorer.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;
    private readonly IElevenLabsService _elevenLabsService;

    public AiChatController(IAiChatService aiChatService, IElevenLabsService elevenLabsService)
    {
        _aiChatService = aiChatService;
        _elevenLabsService = elevenLabsService;
    }

    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var reply = await _aiChatService.AskAsync(request.Message);
        return Ok(new { reply });
    }

    [HttpPost("tts")]
    public async Task<IActionResult> TextToSpeech([FromBody] TtsRequest request)
    {
        var audioBytes = await _elevenLabsService.TextToSpeechAsync(request.Text);

        if (audioBytes.Length == 0)
            return BadRequest("Text-to-Speech failed");

        return File(audioBytes, "audio/mpeg");
    }
}

public record ChatRequest(string Message);
public record TtsRequest(string Text);