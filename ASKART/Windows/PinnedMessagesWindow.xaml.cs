using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Askart.Windows
{
    public partial class PinnedMessagesWindow : Window
    {
        private Chat chat;

        public PinnedMessagesWindow(Chat chat)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            this.chat = chat;
            ChatNameText.Text = chat.Name;
            Loaded += async (_, _) => await App.Dialogs.RunAsync(LoadPinnedMessages);
        }

        private async Task LoadPinnedMessages()
        {
            PinnedPanel.ItemsSource = await App.Client.Messages.GetPinnedMessages(chat);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
