namespace Askart.ViewModels.Administration;

public sealed class AdminShellViewModel : IDisposable
{
    private readonly Dictionary<AdminSection, Task> initialLoads = new();
    private bool disposed;
    public AdminNavigationViewModel Navigation { get; }
    public AdminStatisticsViewModel Statistics { get; }
    public AdminUsersViewModel Users { get; }
    public AdminChatsViewModel Chats { get; }

    public AdminShellViewModel(AdminNavigationViewModel navigation, AdminStatisticsViewModel statistics,
        AdminUsersViewModel users, AdminChatsViewModel chats)
    {
        Navigation = navigation;
        Statistics = statistics;
        Users = users;
        Chats = chats;
    }

    public bool Select(AdminSection section)
    {
        if (disposed || section == Navigation.Section) return false;
        Navigation.Select(section);
        return true;
    }

    public Task LoadSelectedAsync()
    {
        if (disposed) return Task.CompletedTask;
        var section = Navigation.Section;
        if (initialLoads.TryGetValue(section, out var loading)) return loading;
        loading = section switch
        {
            AdminSection.Users => Users.LoadAsync(),
            AdminSection.Chats => Chats.LoadAsync(),
            _ => Statistics.LoadAsync()
        };
        initialLoads.Add(section, loading);
        return loading;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Statistics.Dispose();
        Users.Dispose();
        Chats.Dispose();
        initialLoads.Clear();
    }
}
