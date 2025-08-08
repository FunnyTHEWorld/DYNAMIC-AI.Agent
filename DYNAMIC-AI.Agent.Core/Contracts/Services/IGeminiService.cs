using DYNAMIC_AI.Agent.Core.Models;
using System.Threading.Tasks;

namespace DYNAMIC_AI.Agent.Core.Contracts.Services;

public interface IGeminiService
{
    Task<string> GetChatResponseAsync(string prompt, GeminiSettings settings);
}
