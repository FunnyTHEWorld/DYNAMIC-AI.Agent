namespace DYNAMIC_AI.Agent.Core.Models;

public class ChatResponse
{
    public string? Content { get; set; }
    public int PromptTokenCount { get; set; }
    public int CandidatesTokenCount { get; set; }
}
