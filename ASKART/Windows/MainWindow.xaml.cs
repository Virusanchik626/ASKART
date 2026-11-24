using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Askart.Windows
{
    public partial class MainWindow : Window
    {
        private int chatsVersion;
        private readonly Askart.ViewModels.MainWindowViewModel viewModel;
        private Chat? currentChat { get => viewModel.SelectedChat; set => viewModel.SelectedChat = value; }
        private Message replyingTo = null;
        private bool showArchived = false;
        private KanbanWindow kanbanWindow = null;

        public MainWindow(Askart.ViewModels.MainWindowViewModel viewModel)
        {
            this.viewModel = viewModel;
            ThemeHelper.Watch(this);
            InitializeComponent();
            DataContext = viewModel;
            viewModel.MessagesChanged += OnMessagesChanged;
            Closed += (_, _) =>
            {
                viewModel.MessagesChanged -= OnMessagesChanged;
                viewModel.Dispose();
            };
            MessageTextBox.MaxLength = 200;
            Loaded += async (_, _) => await App.Dialogs.RunAsync(LoadChats);
            UpdateUserInfo();
        }

        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                if (e.Key == Key.Enter)
                {
                    if (Keyboard.Modifiers == ModifierKeys.Shift)
                    {
                        return;
                    }
                    e.Handled = true;
                    await SendMessage();
                }
            });
        }

        private async Task LoadChats()
        {
            if (!IsLoaded || App.CurrentUser == null) return;
            int version = ++chatsVersion;

            string searchText = "";
            if (SearchTextBox != null)
            {
                searchText = SearchTextBox.Text.Trim();
            }

            List<Chat> chats = await viewModel.LoadChatsAsync(showArchived, searchText);

            if (version != chatsVersion || !IsLoaded) return;
            ChatListPanel.ItemsSource = chats.Select(chat => new Askart.ViewModels.ChatListItem(chat)).ToList();

            UpdateChatSelection();
        }

        private async void ChatCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                Border border = sender as Border;
                Chat chat = border.Tag as Chat;
                if (chat != null)
                {
                    await SelectChat(chat);
                }
            });
        }

        private void UpdateChatSelection()
        {
            if (ChatListPanel.ItemsSource is IEnumerable<Askart.ViewModels.ChatListItem> items)
                foreach (var item in items)
                    item.IsSelected = currentChat?.Id == item.Chat.Id;
        }

        private async Task SelectChat(Chat chat)
        {
            currentChat = chat;
            replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;

            WelcomePanel.Visibility = Visibility.Collapsed;
            ChatHeader.Visibility = Visibility.Visible;
            MessagesScroll.Visibility = Visibility.Visible;
            InputArea.Visibility = Visibility.Visible;

            ChatNameText.Text = chat.Name;
            if (chat.IsArchived)
            {
                ChatNameText.Text = chat.Name + " 📦";
            }

            string statusText = chat.TypeChat;
            if (chat.TypeGroup != null)
            {
                statusText = chat.TypeChat + " • " + chat.TypeGroup;
            }

            if (chat.TypeChat == "Личный")
            {
                foreach (ChatMember member in chat.Members)
                {
                    if (member.UserId != App.CurrentUser.Id)
                    {
                        statusText = statusText + " • Онлайн";
                        break;
                    }
                }
            }
            ChatTypeText.Text = statusText;

            bool isAdmin = chat.IsAdmin(App.CurrentUser.Id);

            if (chat.TypeGroup == "Рабочая")
            {
                KanbanButton.Visibility = Visibility.Visible;
            }
            else
            {
                KanbanButton.Visibility = Visibility.Collapsed;
            }

            MembersButton.Visibility = Visibility.Visible;

            if (isAdmin)
            {
                PinnedButton.Visibility = Visibility.Visible;
                StatsButton.Visibility = Visibility.Visible;
                ArchiveButton.Visibility = Visibility.Visible;
            }
            else
            {
                PinnedButton.Visibility = Visibility.Collapsed;
                StatsButton.Visibility = Visibility.Collapsed;
                ArchiveButton.Visibility = Visibility.Collapsed;
            }

            if (chat.IsArchived)
            {
                ArchiveButton.Content = "📤 Разархивировать";
            }
            else
            {
                ArchiveButton.Content = "📦 Архивировать";
            }

            await UpdatePinnedBar();
            if (!IsLoaded || currentChat != chat) return;
            await LoadMessages();
            UpdateChatSelection();
        }

        private async Task UpdatePinnedBar()
        {
            var chat = currentChat;
            if (chat == null || !IsLoaded) return;
            List<Message> pinnedMessages = await viewModel.LoadPinnedAsync(chat);
            if (!IsLoaded || currentChat != chat) return;

            if (pinnedMessages.Count > 0)
            {
                PinnedBar.Visibility = Visibility.Visible;
                Message lastPinned = pinnedMessages[0]; 
                PinnedBarText.Text = lastPinned.SenderName + ": " + lastPinned.Text +
                                     " (всего закреплённых: " + pinnedMessages.Count + ")";
            }
            else
            {
                PinnedBar.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenPinnedWindow()
        {
            if (currentChat == null)
            {
                return;
            }
            PinnedMessagesWindow window = new PinnedMessagesWindow(currentChat);
            window.Owner = this;
            window.ShowDialog();
        }

        private void PinnedBar_Click(object sender, MouseButtonEventArgs e)
        {
            OpenPinnedWindow();
        }

        private void Pinned_Click(object sender, RoutedEventArgs e)
        {
            OpenPinnedWindow();
        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat == null)
            {
                return;
            }
            ChatStatisticsWindow window = App.CreateChatStatistics(currentChat);
            window.Owner = this;
            window.ShowDialog();
        }

        private void Members_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat == null)
            {
                return;
            }
            MembersWindow membersWindow = new MembersWindow(currentChat);
            membersWindow.Owner = this;
            membersWindow.ShowDialog();
        }

        private async void Archive_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                if (!viewModel.ToggleArchive() || !IsLoaded) return;
                await LoadChats();
                if (currentChat != null) await SelectChat(currentChat);
            });
        }

        private void Kanban_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat == null)
            {
                return;
            }

            if (kanbanWindow != null)
            {
                kanbanWindow.Close();
            }

            kanbanWindow = new KanbanWindow(currentChat);
            kanbanWindow.Owner = this; 
            kanbanWindow.Closed += Kanban_Closed;
            kanbanWindow.Show();
        }

        private void Kanban_Closed(object sender, EventArgs e)
        {
            KanbanWindow closedWindow = sender as KanbanWindow;
            if (closedWindow == kanbanWindow)
            {
                kanbanWindow = null;
            }
        }

        private Task LoadMessages() => viewModel.LoadMessagesAsync();

        private async void OnMessagesChanged(object? sender, EventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                if (!IsLoaded) return;
                MessagesScroll.ScrollToEnd();
                await UpdatePinnedBar();
            });
        }

        internal void ReplyToMessage(Message message)
        {
            replyingTo = message;

            ReplyAuthorText.Text = "↩ Ответ на сообщение: " + message.SenderName;

            string preview = message.Text;
            if (preview.Length > 60)
            {
                preview = preview.Substring(0, 60) + "...";
            }
            ReplyPreviewText.Text = preview;
            ReplyPanel.Visibility = Visibility.Visible;

            MessageTextBox.Focus();
        }

        private void CancelReply_Click(object sender, RoutedEventArgs e)
        {
            replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                await SendMessage();
            });
        }

        private async Task SendMessage()
        {
            if (currentChat == null)
            {
                return;
            }

            string text = MessageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (!await viewModel.SendAsync(text, replyingTo)) return;
            if (!IsLoaded) return;

            if (MessageTextBox.Text.Trim() == text) MessageTextBox.Clear();
            replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;

            await LoadMessages();
        }

        private void Emoji_Click(object sender, RoutedEventArgs e)
        {
            EmojiPickerWindow emojiWindow = new EmojiPickerWindow();
            emojiWindow.Owner = this;

            if (emojiWindow.ShowDialog() == true)
            {
                MessageTextBox.Text = MessageTextBox.Text + emojiWindow.SelectedEmoji;
                MessageTextBox.Focus();
                MessageTextBox.CaretIndex = MessageTextBox.Text.Length;
            }
        }

        private async void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                CreateChatDialog dialog = new CreateChatDialog();
                dialog.Owner = this;
    
                if (dialog.ShowDialog() == true)
                {
                    await App.Client.Chats.CreateChat(dialog.ChatName, dialog.IsGroup, dialog.IsWorkChat, App.CurrentUser);
                    await LoadChats();
                }
            });
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = new ProfileWindow();
            profileWindow.Owner = this;
            profileWindow.ShowDialog();

            if (profileWindow.IsLoggedOut)
            {
                App.Navigation.EndSession();
            }
        }

        private async void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                await LoadChats();
            });
        }

        private async void FilterChanged(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                showArchived = (ShowArchivedCheckBox.IsChecked == true);
                await LoadChats();
            });
        }

        private void UpdateUserInfo()
        {
            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.FullName;
            }
        }
    }

}
