using Askart.Models;
using System.ComponentModel;

namespace Askart.ViewModels;

public sealed class ChatListItem(Chat chat) : INotifyPropertyChanged
{
    public Chat Chat { get; } = chat;
    public string Initial => string.IsNullOrEmpty(Chat.Name) ? "" : Chat.Name[..1].ToUpper();
    public string Name => Chat.Name + (Chat.IsArchived ? " 📦" : "");
    public string TypeName => Chat.TypeGroup ?? Chat.TypeChat;
    private bool isSelected;
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (isSelected == value) return;
            isSelected = value;
            PropertyChanged?.Invoke(this, new(nameof(IsSelected)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
}
