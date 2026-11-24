using Askart.Services;
using System.Collections.ObjectModel;

namespace Askart.ViewModels.Administration;

public sealed class AdminChatDetailsViewModel : WindowViewModel
{
    private readonly AdminApiService api;
    private readonly DialogService dialogs;
    private readonly int chatId;
    private readonly Action changed;
    private readonly Action close;
    private int version;
    private bool unavailable;
    private bool mutating;
    public string Metadata => unavailable ? "" : Chat.TypeText + " • Владелец: " + Chat.OwnerName + " • " + Chat.Summary;
    private string note = "";
    public AdminChatDto Chat { get; }
    public string Note { get => note; private set => Set(ref note, value); }
    public ObservableCollection<AdminMessageDto> Messages { get; } = new();
    public AsyncCommand Refresh { get; }
    public AsyncCommand DeleteMessage { get; }
    public AsyncCommand DeleteChat { get; }

    public AdminChatDetailsViewModel(int chatId, AdminChatDto chat, AdminApiService api,
        DialogService dialogs, Action changed, Action close)
    {
        this.chatId = chatId;
        Chat = chat;
        this.api = api;
        this.dialogs = dialogs;
        this.changed = changed;
        this.close = close;
        Refresh = new(_ => LoadAsync(), Error);
        DeleteMessage = new(value => MutateAsync(() => DeleteMessageAsync(value)), Error, _ => !unavailable && !mutating);
        DeleteChat = new(_ => MutateAsync(DeleteChatAsync), Error, _ => !unavailable && !mutating);
    }

    public async Task LoadAsync()
    {
        if (IsClosed) return;
        int request = ++version;
        Messages.Clear();
        Status = "Загрузка сообщений…";
        try
        {
            var result = await api.GetChatMessagesAsync(chatId, Lifetime);
            if (IsClosed || request != version) return;
            unavailable = false;
            DeleteMessage.Refresh();
            DeleteChat.Refresh();
            foreach (var message in result.Items) Messages.Add(message);
            Note = result.Total > Messages.Count
                ? "Показаны последние " + Messages.Count + " из " + result.Total + " сообщений." : "";
            Status = Messages.Count == 0 ? "В этом чате пока нет сообщений." : "";
        }
        catch (OperationCanceledException) when (IsClosed) { }
        catch (Exception ex)
        {
            if (request != version || IsClosed) return;
            if (ex is ApiException { StatusCode: System.Net.HttpStatusCode.NotFound })
            {
                unavailable = true;
                Changed(nameof(Metadata));
                Note = "";
                changed();
                DeleteMessage.Refresh();
                DeleteChat.Refresh();
            }
            Error(ex);
        }
    }

    private async Task MutateAsync(Func<Task> operation)
    {
        if (mutating || IsClosed) return;
        mutating = true;
        DeleteMessage.Refresh();
        DeleteChat.Refresh();
        try { await operation(); }
        finally
        {
            mutating = false;
            DeleteMessage.Refresh();
            DeleteChat.Refresh();
        }
    }

    private async Task DeleteMessageAsync(object? value)
    {
        if (value is not AdminMessageDto message || IsClosed) return;
        string preview = message.Text.Length > 150 ? message.Text[..150] + "…" : message.Text;
        if (!dialogs.Confirm("Удалить сообщение от " + message.SenderName +
            "?\n\n«" + preview + "»\n\nДействие необратимо.", "Удаление сообщения")) return;
        await api.DeleteMessageAsync(message.Id, Lifetime);
        if (IsClosed) return;
        Chat.MessagesCount = Math.Max(0, Chat.MessagesCount - 1);
        Changed(nameof(Chat));
        Changed(nameof(Metadata));
        changed();
        await LoadAsync();
    }

    private async Task DeleteChatAsync()
    {
        if (IsClosed || !dialogs.Confirm("Удалить чат «" + Chat.Name + "» навсегда?\n\n" +
            "Вместе с ним будут удалены: сообщения (" + Chat.MessagesCount + "), задачи (" +
            Chat.TasksCount + "), колонки канбан-доски и список участников.\n\nДействие необратимо.",
            "Удаление чата")) return;
        await api.DeleteChatAsync(chatId, Lifetime);
        if (IsClosed) return;
        changed();
        close();
    }
}
