using Askart.Services;
using System.Collections.ObjectModel;

namespace Askart.ViewModels.Administration;

public sealed class AdminChatItem(AdminChatDto chat) : ObservableObject
{
    public AdminChatDto Chat { get; } = chat;
    private bool selected;
    public bool IsSelected { get => selected; set => Set(ref selected, value); }
}

public sealed class AdminChatsViewModel : WindowViewModel
{
    private readonly AdminApiService api;
    private int version;
    private string search = "";
    private int filter;
    private string count = "";
    private AdminChatDto? selected;
    public AdminNavigationViewModel Navigation { get; }
    public string Search { get => search; set { if (Set(ref search, value)) version++; } }
    public int Filter { get => filter; set { if (Set(ref filter, value)) { version++; Find.Execute(null); } } }
    public string Count { get => count; private set => Set(ref count, value); }
    public AdminChatDto? Selected { get => selected; set => Set(ref selected, value); }
    public ObservableCollection<AdminChatItem> Chats { get; } = new();
    public AsyncCommand Find { get; }
    public AsyncCommand OpenChat { get; }

    public AdminChatsViewModel(AdminApiService api, AdminNavigationViewModel navigation, Action<AdminChatDto> openChat)
    {
        this.api = api;
        Navigation = navigation;
        Find = new(_ => LoadAsync(), Error, allowConcurrent: true);
        OpenChat = new(value => {
            if (value is AdminChatItem item)
            {
                foreach (var row in Chats) row.IsSelected = row == item;
                Selected = item.Chat;
                openChat(item.Chat);
            }
            return Task.CompletedTask;
        }, Error);
    }

    public async Task LoadAsync()
    {
        if (IsClosed) return;
        int request = ++version;
        Chats.Clear();
        Status = "Загрузка…";
        try
        {
            string? type = Filter == 1 ? "Личный" : Filter >= 2 ? "Группа" : null;
            string? group = Filter == 2 ? "Личная" : Filter == 3 ? "Рабочая" : null;
            var result = await api.GetChatsAsync(Search.Trim(), type, group, 0, 200, Lifetime);
            if (IsClosed || request != version) return;
            foreach (var chat in result.Items)
                Chats.Add(new(chat) { IsSelected = Selected?.Id == chat.Id });
            Count = "Показано " + Chats.Count + " из " + result.Total;
            Status = Chats.Count == 0 ? "Чаты не найдены." : "";
        }
        catch (OperationCanceledException) when (IsClosed) { }
        catch (Exception ex) { if (request == version) Error(ex); }
    }
}
