using CommunityToolkit.Mvvm.ComponentModel;
using DYNAMIC_AI.Agent.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

namespace DYNAMIC_AI.Agent.ViewModels;

public partial class ChatMessageViewModel : ObservableObject
{
    private readonly ChatMessage _message;

    public ChatMessageViewModel(ChatMessage message)
    {
        _message = message;
    }

    public string? Content => _message.Content;
    public SenderType Sender => _message.Sender;
    public DateTime Timestamp => _message.Timestamp;
    public string? AttachmentPath => _message.AttachmentPath;

    private BitmapImage? _attachmentThumbnail;
    public BitmapImage? AttachmentThumbnail
    {
        get => _attachmentThumbnail;
        set => SetProperty(ref _attachmentThumbnail, value);
    }
}
