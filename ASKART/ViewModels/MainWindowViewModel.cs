using Askart.Models;
using Askart.Services;
using System.Collections.ObjectModel;

namespace Askart.ViewModels;

public sealed class MainWindowViewModel : WindowViewModel
{
    private readonly MessageService messages;
    private readonly ChatService chats;
    private readonly User user;
    private readonly DialogService dialogs;
    private readonly Func<string, string?> edit;
    private int version;
    private Chat? selectedChat;
    private bool sending;
    public bool IsSending { get => sending; private set => Set(ref sending, value); }
    public Chat? SelectedChat
    {
        get => selectedChat;
        set
        {
            if (!Set(ref selectedChat, value)) return;
            version++;
            Messages.Clear();
        }
    }
    public ObservableCollection<MessageItemViewModel> Messages { get; } = new();
    public AsyncCommand Edit { get; }
    public AsyncCommand Delete { get; }
    public AsyncCommand TogglePin { get; }
    public AsyncCommand Reply { get; }
    public AsyncCommand Copy { get; }
    public event EventHandler? MessagesChanged;

    public MainWindowViewModel(MessageService messages, ChatService chats, User user, DialogService dialogs,
        Func<string, string?> edit, Action<Message> reply, Action<string> copy)
    {
        this.messages = messages;
        this.chats = chats;
        this.user = user;
        this.dialogs = dialogs;
        this.edit = edit;
        Edit = new(EditAsync, Error);
        Delete = new(DeleteAsync, Error);
        TogglePin = new(PinAsync, Error);
        Reply = new(value => { if (value is Message message && CanAct(message, SelectedChat)) reply(message); return Task.CompletedTask; }, Error);
        Copy = new(value => { if (value is Message message) copy(message.Text); return Task.CompletedTask; }, Error);
    }

    public Task<List<Chat>> LoadChatsAsync(bool archived, string search) =>
        chats.GetMyChats(user.Id, archived, search, Lifetime);

    public Task<List<Message>> LoadPinnedAsync(Chat chat) => messages.GetPinnedMessages(chat, Lifetime);

    public bool ToggleArchive()
    {
        var chat = SelectedChat;
        if (IsClosed || chat == null) return false;
        if (!chat.IsAdmin(user.Id))
        {
            dialogs.Information("Только администратор может архивировать чат", "Ошибка");
            return false;
        }
        string action = chat.IsArchived ? "разархивировать" : "архивировать";
        if (!dialogs.Confirm("Вы действительно хотите " + action + " чат «" + chat.Name + "»?", "Архивация"))
            return false;
        chats.SetArchived(chat, !chat.IsArchived);
        return true;
    }

    public async Task LoadMessagesAsync()
    {
        var chat = SelectedChat;
        int request = ++version;
        if (chat == null || IsClosed) return;
        try
        {
            var result = await messages.GetMessages(chat, Lifetime);
            if (IsClosed || request != version || SelectedChat != chat) return;
            Messages.Clear();
            foreach (var message in result)
                Messages.Add(new(message, message.SenderId == user.Id, chat.IsAdmin(user.Id), this));
            Status = "";
            MessagesChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { if (request == version) Error(ex); }
    }

    private bool CanAct(Message message, Chat? chat) =>
        !IsClosed && chat != null && message.ChatId == chat.Id;

    private async Task EditAsync(object? value)
    {
        var chat = SelectedChat;
        if (value is not Message message || !CanAct(message, chat) || message.SenderId != user.Id) return;
        var text = edit(message.Text);
        if (text == null) return;
        await messages.EditMessage(message, text, Lifetime);
        if (SelectedChat == chat) await LoadMessagesAsync();
    }

    private async Task DeleteAsync(object? value)
    {
        var chat = SelectedChat;
        if (value is not Message message || !CanAct(message, chat)) return;
        bool own = message.SenderId == user.Id;
        if (!own && !chat!.IsAdmin(user.Id)) return;
        string question = own ? "Удалить ваше сообщение?" :
            "Удалить сообщение пользователя " + message.SenderName + "? (права администратора)";
        if (!dialogs.Confirm(question, "Удаление сообщения")) return;
        await messages.DeleteMessage(chat!, message, Lifetime);
        if (SelectedChat == chat) await LoadMessagesAsync();
    }

    private async Task PinAsync(object? value)
    {
        var chat = SelectedChat;
        if (value is not Message message || !CanAct(message, chat) || !chat!.IsAdmin(user.Id)) return;
        messages.SetPinned(message, !message.IsPinned);
        await LoadMessagesAsync();
        if (!IsClosed) dialogs.Information(message.IsPinned ? "Сообщение закреплено 📌" :
            "Сообщение откреплено", "Закрепление");
    }

    public async Task<bool> SendAsync(string text, Message? reply)
    {
        var chat = SelectedChat;
        if (sending || IsClosed || chat == null || string.IsNullOrWhiteSpace(text)) return false;
        IsSending = true;
        try
        {
            var sent = await messages.SendMessage(chat, user, text.Trim(), reply, Lifetime);
            if (IsClosed || SelectedChat != chat || sent == null) return false;
            await LoadMessagesAsync();
            return true;
        }
        finally { IsSending = false; }
    }
}
