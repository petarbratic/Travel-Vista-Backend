namespace Explorer.Stakeholders.API.Public;

public interface IAiChatService
{
    Task<string> AskAsync(string userMessage);
}
