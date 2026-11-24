using Askart.Services;
using System.Collections.ObjectModel;

namespace Askart.ViewModels.Administration;

public sealed record AdminUserItem(AdminUserDto User)
{
    public string Initial => string.IsNullOrEmpty(User.Name) ? "?" : User.Name[..1].ToUpper();
    public string Contacts => User.Login +
        (!string.IsNullOrEmpty(User.Email) && User.Email != User.Login ? " • " + User.Email : "") +
        " • " + User.Number;
}

public sealed class AdminUsersViewModel : WindowViewModel
{
    private readonly AdminApiService api;
    private int version;
    private string search = "";
    private string count = "";
    public AdminNavigationViewModel Navigation { get; }
    public string Search { get => search; set { if (Set(ref search, value)) version++; } }
    public string Count { get => count; private set => Set(ref count, value); }
    public ObservableCollection<AdminUserItem> Users { get; } = new();
    public AsyncCommand Find { get; }

    public AdminUsersViewModel(AdminApiService api, AdminNavigationViewModel navigation)
    {
        this.api = api;
        Navigation = navigation;
        Find = new(_ => LoadAsync(), Error, allowConcurrent: true);
    }

    public async Task LoadAsync()
    {
        if (IsClosed) return;
        int request = ++version;
        Users.Clear();
        Status = "Загрузка…";
        try
        {
            var result = await api.GetUsersAsync(Search.Trim(), 0, 200, Lifetime);
            if (IsClosed || request != version) return;
            foreach (var user in result.Items) Users.Add(new(user));
            Count = "Показано " + Users.Count + " из " + result.Total;
            Status = Users.Count == 0 ? "Пользователи не найдены." : "";
        }
        catch (OperationCanceledException) when (IsClosed) { }
        catch (Exception ex) { if (request == version) Error(ex); }
    }
}
