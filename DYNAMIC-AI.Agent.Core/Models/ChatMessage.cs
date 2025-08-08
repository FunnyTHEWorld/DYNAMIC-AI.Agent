using Microsoft.UI.Xaml.Media.Imaging;

namespace DYNAMIC_AI.Agent.Core.Models;

public enum SenderType
{
    User,
    AI
}

public class ChatMessage
{
    public string? Content
    {
        get; set;
    }
    public SenderType Sender
    {
        get; set;
    }
    public DateTime Timestamp
    {
        get; set;
    }
    public string? AttachmentPath
    {
        get; set;
    }
    public BitmapImage? AttachmentThumbnail
    {
        get; set;
    }
}
