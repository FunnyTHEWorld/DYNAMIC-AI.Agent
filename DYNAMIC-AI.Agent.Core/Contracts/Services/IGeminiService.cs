using DYNAMIC_AI.Agent.Core.Models;
using System.Threading.Tasks;

namespace DYNAMIC_AI.Agent.Core.Contracts.Services;

public interface IGeminiService
{
    Task<ChatResponse> GetChatResponseAsync(string prompt, GeminiSettings settings);
}
