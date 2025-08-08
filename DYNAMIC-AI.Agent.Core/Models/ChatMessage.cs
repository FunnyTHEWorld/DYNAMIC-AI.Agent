using CommunityToolkit.Mvvm.ComponentModel;

namespace DYNAMIC_AI.Agent.Core.Models;

public enum SenderType
{
    User,
    AI
}

using System.Collections.Generic;
using DYNAMIC_AI.Agent.Helpers;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    private string? _content;

    [ObservableProperty]
    private string? _markdownContent;

    [ObservableProperty]
    private List<RenderedContent> _renderedContent;

    [ObservableProperty]
    private string? _thinkingContent;

    [ObservableProperty]
    private SenderType _sender;

    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private int _promptTokenCount;

    [ObservableProperty]
    private int _candidatesTokenCount;

    [ObservableProperty]
    private bool _isThinkingExpanded;
}
