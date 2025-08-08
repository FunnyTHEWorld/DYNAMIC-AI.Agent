using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DYNAMIC_AI.Agent.Core.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Models;

namespace DYNAMIC_AI.Agent.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IGeminiService _geminiService;

    public ObservableCollection<ChatMessage> ChatMessages { get; } = new ObservableCollection<ChatMessage>();

    [ObservableProperty]
    private string? _userInput;

    [ObservableProperty]
    private double _temperature = 0.5;

    [ObservableProperty]
    private double _topP = 0.9;

    public MainViewModel(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput))
        {
            return;
        }

        var userMessage = new ChatMessage
        {
            Content = UserInput,
            Sender = SenderType.User,
            Timestamp = System.DateTime.Now
        };
        ChatMessages.Add(userMessage);

        var prompt = UserInput;
        UserInput = string.Empty;

        var response = await _geminiService.GetChatResponseAsync(prompt);

        var aiMessage = new ChatMessage
        {
            Content = response,
            Sender = SenderType.AI,
            Timestamp = System.DateTime.Now
        };
        ChatMessages.Add(aiMessage);
    }
}
