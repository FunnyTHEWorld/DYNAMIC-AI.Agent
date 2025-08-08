using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DYNAMIC_AI.Agent.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Models;

namespace DYNAMIC_AI.Agent.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IGeminiService _geminiService;
    private readonly ILocalSettingsService _localSettingsService;

    public ObservableCollection<ChatMessage> ChatMessages { get; } = new ObservableCollection<ChatMessage>();

    [ObservableProperty]
    private string? _userInput;

    [ObservableProperty]
    private double _temperature = 0.5;

    [ObservableProperty]
    private double _topP = 0.9;

    [ObservableProperty]
    private bool _isThinking;

    [ObservableProperty]
    private bool _isStreamingEnabled;

    public MainViewModel(IGeminiService geminiService, ILocalSettingsService localSettingsService)
    {
        _geminiService = geminiService;
        _localSettingsService = localSettingsService;
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

        IsThinking = true;

        var settings = await _localSettingsService.ReadSettingAsync<GeminiSettings>("GeminiSettings");
        if (settings == null)
        {
            // Handle case where settings are not found
            var errorMessage = new ChatMessage
            {
                Content = "Gemini settings not configured. Please go to the settings page.",
                Sender = SenderType.AI,
                Timestamp = System.DateTime.Now
            };
            ChatMessages.Add(errorMessage);
            IsThinking = false;
            return;
        }

        var response = await _geminiService.GetChatResponseAsync(prompt, settings);

        userMessage.PromptTokenCount = response.PromptTokenCount;

        var aiMessage = new ChatMessage
        {
            Content = response.Content,
            Sender = SenderType.AI,
            Timestamp = System.DateTime.Now,
            CandidatesTokenCount = response.CandidatesTokenCount
        };
        ChatMessages.Add(aiMessage);

        IsThinking = false;
    }
}
