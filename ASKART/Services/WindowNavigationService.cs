using Askart.ViewModels.Administration;
using Askart.Windows;
using System.Windows;

namespace Askart.Services;

public sealed class WindowNavigationService
{
    private readonly SessionService session;
    private readonly AdminApiService administration;
    private readonly DialogService dialogs;
    private readonly Dictionary<int, AdminChatDetailsWindow> details = new();

    public WindowNavigationService(SessionService session, AdminApiService administration, DialogService dialogs)
    {
        this.session = session;
        this.administration = administration;
        this.dialogs = dialogs;
    }

    public void OpenSession(Window previous, Models.User user)
    {
        Window next = SessionService.DestinationFor(user) == StartDestination.Administration
            ? CreateAdmin() : App.CreateMainWindow();
        Replace(previous, next);
    }

    private AdminWindow CreateAdmin()
    {
        var navigation = new AdminNavigationViewModel(session.CurrentUser?.FullName ?? "Администратор",
            NavigateAdmin, Logout, dialogs.Error);
        var shell = new AdminShellViewModel(navigation,
            new(administration, navigation),
            new(administration, navigation),
            new(administration, navigation, OpenChatDetails));
        var window = new AdminWindow(shell);
        window.Closed += (_, _) =>
        {
            if (Application.Current.MainWindow == window) session.Clear();
        };
        return window;
    }

    private void NavigateAdmin(AdminSection section)
    {
        if (session.CurrentUser?.IsAdmin != true ||
            Application.Current.MainWindow is not AdminWindow window ||
            window.DataContext is not AdminShellViewModel shell ||
            shell.Navigation.Section == section) return;
        CloseDetails();
        shell.Select(section);
    }

    private void CloseDetails()
    {
        foreach (var window in details.Values.ToArray()) window.Close();
        details.Clear();
    }

    private void OpenChatDetails(AdminChatDto chat)
    {
        if (session.CurrentUser?.IsAdmin != true) return;
        if (details.TryGetValue(chat.Id, out var existing))
        {
            if (existing.WindowState == WindowState.Minimized) existing.WindowState = WindowState.Normal;
            existing.Activate();
            return;
        }
        var owner = Application.Current.MainWindow;
        AdminChatDetailsWindow? window = null;
        var viewModel = new AdminChatDetailsViewModel(chat.Id, chat, administration, dialogs,
            () =>
            {
                if (owner.IsLoaded && owner.DataContext is AdminShellViewModel shell)
                    shell.Chats.Find.Execute(null);
            },
            () => window?.Close());
        window = new(viewModel) { Owner = owner };
        details.Add(chat.Id, window);
        window.Closed += (_, _) => details.Remove(chat.Id);
        window.Show();
    }

    public void Logout()
    {
        if (!dialogs.Confirm("Вы действительно хотите выйти?", "Выход")) return;
        EndSession();
    }

    public void EndSession()
    {
        var login = new LoginWindow();
        Application.Current.MainWindow = login;
        login.Show();
        session.Clear();
        foreach (var window in Application.Current.Windows.Cast<Window>().ToArray())
            if (window != login && window.IsLoaded) window.Close();
    }

    public void Replace(Window previous, Window next)
    {
        Application.Current.MainWindow = next;
        next.Show();
        previous.Close();
    }
}
