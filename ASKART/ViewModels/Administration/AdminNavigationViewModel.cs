namespace Askart.ViewModels.Administration;

public enum AdminSection { Statistics, Users, Chats }

public sealed class AdminNavigationViewModel : ObservableObject
{
    private AdminSection section;
    public string AdminName { get; }
    public AdminSection Section { get => section; private set => Set(ref section, value); }
    public AsyncCommand Statistics { get; }
    public AsyncCommand Users { get; }
    public AsyncCommand Chats { get; }
    public AsyncCommand Logout { get; }

    public AdminNavigationViewModel(string name, Action<AdminSection> navigate,
        Action logout, Action<Exception> error)
    {
        AdminName = name;
        Statistics = Create(() => navigate(AdminSection.Statistics), error);
        Users = Create(() => navigate(AdminSection.Users), error);
        Chats = Create(() => navigate(AdminSection.Chats), error);
        Logout = Create(logout, error);
    }

    internal void Select(AdminSection value) => Section = value;

    private static AsyncCommand Create(Action action, Action<Exception> error) =>
        new(_ => { action(); return Task.CompletedTask; }, error);
}
