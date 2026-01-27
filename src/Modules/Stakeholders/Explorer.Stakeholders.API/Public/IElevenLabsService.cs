namespace Explorer.Stakeholders.API.Public;

public interface IElevenLabsService
{
    Task<byte[]> TextToSpeechAsync(string text);
}
