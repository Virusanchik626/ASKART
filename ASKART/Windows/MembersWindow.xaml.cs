using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Askart.Windows
{
    public partial class MembersWindow : Window
    {
        private Chat chat;

        public MembersWindow(Chat chat)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            Loaded += async (_, _) => await App.Dialogs.RunAsync(LoadMembers);
        }

        private async Task LoadMembers()
        {
            var items = new List<Askart.ViewModels.MemberItem>();
            foreach (var member in await App.Client.Chats.GetMembers(chat))
            {
                var user = await App.Client.Users.GetUserById(member.UserId);
                if (user != null) items.Add(new(user, member.Role));
            }
            MembersPanel.ItemsSource = items;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
