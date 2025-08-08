using DYNAMIC_AI.Agent.Core.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DYNAMIC_AI.Agent.Helpers;

public class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate? UserMessageTemplate { get; set; }
    public DataTemplate? AiMessageTemplate { get; set; }

    protected override DataTemplate? SelectTemplateCore(object item)
    {
        var message = (ChatMessage)item;
        return message.Sender == SenderType.User ? UserMessageTemplate : AiMessageTemplate;
    }
}
