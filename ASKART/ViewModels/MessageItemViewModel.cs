using Askart.Models;

namespace Askart.ViewModels;

public sealed class MessageItemViewModel(Message message, bool isOwn, bool isAdmin, MainWindowViewModel conversation)
{
    public Message Message { get; } = message;
    public MainWindowViewModel Conversation { get; } = conversation;
    public bool IsOwn { get; } = isOwn;
    public bool IsAdmin { get; } = isAdmin;
    public bool CanDelete => IsOwn || IsAdmin;
    public string Sender => Message.SenderName + (Message.IsPinned ? " 📌" : "");
    public string Time => Message.SendTime.ToString("HH:mm") + (Message.EditTime.HasValue ? " (ред.)" : "");
    public string PinIcon => Message.IsPinned ? "📍" : "📌";
    public string PinTip => Message.IsPinned ? "Открепить" : "Закрепить";
    public string PinHeader => PinIcon + " " + PinTip;
    public string DeleteTip => IsAdmin && !IsOwn ? "Удалить (админ)" : "Удалить";
    public bool HasReply => Message.ReplyTo != null;
    public string ReplyText => Message.ReplyTo == null ? "" :
        "↩ " + Message.ReplyTo.SenderName + ": " + Message.ReplyTo.Text;
}
