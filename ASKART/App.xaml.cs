using Askart.Models;
using System.Windows;

namespace Askart
{
    public partial class App : Application
    {
        
        public static Services.DialogService Dialogs { get; } = new();
        public static Services.ClientServices Client { get; } = new();
        public static Services.SessionService Session { get; } = new();
        public static Services.AuthenticationService Authentication { get; } = new(
            new System.Net.Http.HttpClient { BaseAddress = Services.ApiConfiguration.BaseAddress,
                Timeout = TimeSpan.FromSeconds(10) }, Session);
        public static Services.AdminApiService Administration { get; } = new(
            new System.Net.Http.HttpClient { BaseAddress = Services.ApiConfiguration.BaseAddress,
                Timeout = TimeSpan.FromSeconds(15) }, Session);
        public static Services.WindowNavigationService Navigation { get; } = new(Session, Administration, Dialogs);
        public static User? CurrentUser
        {
            get => Session.CurrentUser;
            set
            {
                if (value == null) Session.Clear();
                else Session.SignIn(value);
            }
        }

        public static Windows.MainWindow CreateMainWindow()
        {
            Windows.MainWindow? window = null;
            var viewModel = new ViewModels.MainWindowViewModel(Client.Messages, Client.Chats,
                Session.CurrentUser ?? throw new InvalidOperationException("Нет текущего пользователя"),
                Dialogs,
                text =>
                {
                    var dialog = new Windows.EditMessageDialog(text) { Owner = window };
                    return dialog.ShowDialog() == true ? dialog.NewText : null;
                },
                message => window?.ReplyToMessage(message),
                text => Clipboard.SetText(text));
            window = new(viewModel);
            return window;
        }

        public static Windows.ChatStatisticsWindow CreateChatStatistics(Models.Chat chat) =>
            new(new ViewModels.ChatStatisticsViewModel(chat, Session.CurrentUser?.Id ?? 0,
                Client.Chats, Client.Messages, Client.Tasks, Client.Users));

        protected override void OnStartup(StartupEventArgs e)
        {
            
            Session.Cleared += Client.ClearSessionCache;
            Client.Transport.ErrorOccurred += message => MessageBox.Show(message, "Ошибка API",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            ThemeHelper.Start();
            base.OnStartup(e);
        }
    }
}
