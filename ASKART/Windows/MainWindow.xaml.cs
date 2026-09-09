using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Askart.Models;
using Askart.MockData;

namespace Askart.Windows
{
    public partial class MainWindow : Window
    {
        private Chat currentChat;
        private Message _replyingTo = null;
        private bool showArchived = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadChats();
            UpdateUserInfo();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                Send_Click(sender, e);
            }
            else if (e.Key == Key.Enter && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                e.Handled = true;
                Send_Click(sender, e);
            }
        }

        private void LoadChats()
        {
            ChatListPanel.Children.Clear();

            var chatsToShow = DataInitializer.Chats
                .Where(c => showArchived ? true : !c.IsArchived)
                .ToList();

            string searchText = SearchTextBox?.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                chatsToShow = chatsToShow.Where(c => c.Name.ToLower().Contains(searchText)).ToList();
            }

            foreach (var chat in chatsToShow)
            {
                var chatCard = CreateChatCard(chat);
                ChatListPanel.Children.Add(chatCard);
            }
        }

        private Border CreateChatCard(Chat chat)
        {
            var border = new Border
            {
                Style = (Style)FindResource("ChatCardStyle"),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var avatarBorder = new Border
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(22.5),
                Background = new SolidColorBrush(Color.FromRgb(100, 150, 200)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var avatarText = new TextBlock
            {
                Text = chat.Name.Substring(0, 1).ToUpper(),
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            avatarBorder.Child = avatarText;
            Grid.SetColumn(avatarBorder, 0);

            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var nameText = new TextBlock
            {
                Text = chat.Name + (chat.IsArchived ? " 📦" : ""),
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                FontSize = 14
            };

            var typeText = new TextBlock
            {
                Text = chat.TypeGroup != null ? chat.TypeGroup : chat.TypeChat,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 12,
                Foreground = (Brush)FindResource("SecondaryTextBrush")
            };

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(typeText);
            Grid.SetColumn(stackPanel, 1);

            grid.Children.Add(avatarBorder);
            grid.Children.Add(stackPanel);
            border.Child = grid;

            border.MouseLeftButtonDown += (s, e) => SelectChat(chat);
            return border;
        }

        private void SelectChat(Chat chat)
        {
            currentChat = chat;
            _replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;

            WelcomePanel.Visibility = Visibility.Collapsed;
            ChatHeader.Visibility = Visibility.Visible;
            MessagesScroll.Visibility = Visibility.Visible;
            InputArea.Visibility = Visibility.Visible;

            ChatNameText.Text = chat.Name + (chat.IsArchived ? " " : "");

            string statusText = chat.TypeGroup != null ?
                $"{chat.TypeChat} • {chat.TypeGroup}" : chat.TypeChat;

            if (chat.TypeChat == "Личный" && chat.Members.Count > 0)
            {
                var otherMember = chat.Members.FirstOrDefault(m => m.UserId != App.CurrentUser.Id);
                if (otherMember != null)
                    statusText += " • Онлайн";
            }

            ChatTypeText.Text = statusText;

            bool isAdmin = chat.IsAdmin(App.CurrentUser.Id);

            KanbanButton.Visibility = chat.TypeGroup == "Рабочая" ? Visibility.Visible : Visibility.Collapsed;
            MembersButton.Visibility = Visibility.Visible;
            PinnedButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            StatsButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            ArchiveButton.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            ArchiveButton.Content = chat.IsArchived ? "📤 Разархивировать" : "📦 Архивировать";

            UpdatePinnedBar();
            LoadMessages();
            UpdateChatSelection();
        }

        private void UpdatePinnedBar()
        {
            var pinnedMessages = currentChat.Messages.Where(m => m.IsPinned).OrderByDescending(m => m.SendTime).ToList();

            if (pinnedMessages.Count > 0)
            {
                PinnedBar.Visibility = Visibility.Visible;
                var lastPinned = pinnedMessages.First();
                PinnedBarText.Text = $"{lastPinned.SenderName}: {lastPinned.Text} (всего закреплённых: {pinnedMessages.Count})";
            }
            else
            {
                PinnedBar.Visibility = Visibility.Collapsed;
            }
        }

        private void PinnedBar_Click(object sender, MouseButtonEventArgs e)
        {
            if (currentChat != null)
            {
                var window = new PinnedMessagesWindow(currentChat);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        private void Pinned_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat != null)
            {
                var window = new PinnedMessagesWindow(currentChat);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        private void Stats_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat != null)
            {
                var window = new ChatStatisticsWindow(currentChat);
                window.Owner = this;
                window.ShowDialog();
            }
        }

        private void Archive_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat == null) return;

            bool isAdmin = currentChat.IsAdmin(App.CurrentUser.Id);
            if (!isAdmin)
            {
                MessageBox.Show("Только администратор может архивировать чат", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string action = currentChat.IsArchived ? "разархивировать" : "архивировать";
            var result = MessageBox.Show($"Вы действительно хотите {action} чат \"{currentChat.Name}\"?",
                "Архивация", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentChat.IsArchived = !currentChat.IsArchived;
                LoadChats();
                SelectChat(currentChat);
            }
        }

        private void UpdateChatSelection()
        {
            foreach (var child in ChatListPanel.Children)
            {
                if (child is Border border)
                {
                    border.Background = (Brush)FindResource("CardBackgroundBrush");

                    if (border.Child is Grid grid &&
                        grid.Children.Count > 1 &&
                        grid.Children[1] is StackPanel sp &&
                        sp.Children.Count > 0 &&
                        sp.Children[0] is TextBlock tb)
                    {
                        string chatName = tb.Text.Replace(" 📦", "");
                        if (chatName == currentChat?.Name)
                        {
                            border.Background = (Brush)FindResource("ActiveBrush");
                        }
                    }
                }
            }
        }

        private void LoadMessages()
        {
            MessagesPanel.Children.Clear();

            foreach (var message in currentChat.Messages)
            {
                var messageBlock = CreateMessageBlock(message);
                MessagesPanel.Children.Add(messageBlock);
            }

            MessagesScroll.ScrollToEnd();
        }

        private Border CreateMessageBlock(Message message)
        {
            var border = new Border
            {
                Margin = new Thickness(0, 5, 0, 5),
                Padding = new Thickness(15, 10, 15, 10),
                CornerRadius = new CornerRadius(12),
                MaxWidth = 500,
                HorizontalAlignment = message.SenderId == App.CurrentUser.Id ?
                    HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            bool isOwn = message.SenderId == App.CurrentUser.Id;
            bool isAdmin = currentChat.IsAdmin(App.CurrentUser.Id);

            border.Background = isOwn ? (Brush)FindResource("ActiveBrush") : (Brush)FindResource("SecondaryBackgroundBrush");

            if (message.IsPinned)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                border.BorderThickness = new Thickness(2, 2, 2, 2);
            }

            var stackPanel = new StackPanel();

            // Заголовок с кнопками действий
            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = message.SenderName + (message.IsPinned ? " 📌" : ""),
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = isOwn ? Brushes.White : (Brush)FindResource("SecondaryTextBrush"),
                Margin = new Thickness(0, 0, 0, 5),
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);

            if (isOwn || isAdmin)
            {
                var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

                if (isOwn)
                {
                    var editBtn = CreateActionButton("", isOwn ? Brushes.White : (Brush)FindResource("SecondaryTextBrush"), "Редактировать");
                    editBtn.Click += (s, ev) => EditMessage(message);
                    actionsPanel.Children.Add(editBtn);
                }

                var deleteBtn = CreateActionButton("✕", isOwn ? Brushes.White : (Brush)FindResource("ErrorBrush"),
                    isAdmin && !isOwn ? "Удалить (админ)" : "Удалить");
                deleteBtn.Click += (s, ev) => DeleteMessage(message);
                actionsPanel.Children.Add(deleteBtn);

                if (isAdmin)
                {
                    var pinBtn = CreateActionButton(message.IsPinned ? "📍" : "",
                        isOwn ? Brushes.White : (Brush)FindResource("SecondaryTextBrush"),
                        message.IsPinned ? "Открепить" : "Закрепить");
                    pinBtn.Click += (s, ev) => TogglePinMessage(message);
                    actionsPanel.Children.Add(pinBtn);
                }

                var replyBtn = CreateActionButton("↩", isOwn ? Brushes.White : (Brush)FindResource("SecondaryTextBrush"), "Ответить");
                replyBtn.Click += (s, ev) => ReplyToMessage(message);
                actionsPanel.Children.Add(replyBtn);

                Grid.SetColumn(actionsPanel, 1);
                headerGrid.Children.Add(actionsPanel);
            }

            headerGrid.Children.Add(nameText);
            stackPanel.Children.Add(headerGrid);

            // Пересланное сообщение
            if (message.ReplyId.HasValue)
            {
                var replied = currentChat.Messages.FirstOrDefault(m => m.Id == message.ReplyId.Value);
                if (replied != null)
                {
                    var replyBorder = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                        CornerRadius = new CornerRadius(6),
                        Padding = new Thickness(8, 6, 8, 6),
                        Margin = new Thickness(0, 5, 0, 5),
                        BorderBrush = new SolidColorBrush(Color.FromRgb(100, 200, 255)),
                        BorderThickness = new Thickness(2, 0, 0, 0)
                    };

                    var replyText = new TextBlock
                    {
                        Text = $"↩ {replied.SenderName}: {replied.Text}",
                        FontFamily = (FontFamily)FindResource("Roboto"),
                        FontSize = 11,
                        FontStyle = FontStyles.Italic,
                        Foreground = isOwn ? new SolidColorBrush(Color.FromRgb(200, 240, 255)) : (Brush)FindResource("SecondaryTextBrush"),
                        TextWrapping = TextWrapping.Wrap,
                        MaxHeight = 40
                    };
                    replyBorder.Child = replyText;
                    stackPanel.Children.Add(replyBorder);
                }
            }

            var messageText = new TextBlock
            {
                Text = message.Text,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 14,
                Foreground = isOwn ? Brushes.White : (Brush)FindResource("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };
            stackPanel.Children.Add(messageText);

            var timeText = new TextBlock
            {
                Text = message.SendTime.ToString("HH:mm") + (message.EditTime.HasValue ? " (ред.)" : ""),
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 11,
                Foreground = isOwn ? new SolidColorBrush(Color.FromRgb(200, 240, 255)) : (Brush)FindResource("InactiveTextBrush"),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };
            stackPanel.Children.Add(timeText);

            border.Child = stackPanel;

            // Контекстное меню
            var contextMenu = new ContextMenu();
            var copyItem = new MenuItem { Header = "Копировать" };
            copyItem.Click += (s, ev) => Clipboard.SetText(message.Text);
            contextMenu.Items.Add(copyItem);

            var replyItem = new MenuItem { Header = "↩ Ответить" };
            replyItem.Click += (s, ev) => ReplyToMessage(message);
            contextMenu.Items.Add(replyItem);

            if (isOwn)
            {
                var editItem = new MenuItem { Header = "️ Редактировать" };
                editItem.Click += (s, ev) => EditMessage(message);
                contextMenu.Items.Add(editItem);
            }

            if (isOwn || isAdmin)
            {
                var deleteItem = new MenuItem { Header = "✕ Удалить" };
                deleteItem.Click += (s, ev) => DeleteMessage(message);
                contextMenu.Items.Add(deleteItem);
            }

            if (isAdmin)
            {
                var pinItem = new MenuItem { Header = message.IsPinned ? "📍 Открепить" : " Закрепить" };
                pinItem.Click += (s, ev) => TogglePinMessage(message);
                contextMenu.Items.Add(pinItem);
            }

            border.ContextMenu = contextMenu;

            return border;
        }

        private Button CreateActionButton(string content, Brush foreground, string tooltip)
        {
            return new Button
            {
                Content = content,
                Background = Brushes.Transparent,
                Foreground = foreground,
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Cursor = Cursors.Hand,
                Padding = new Thickness(4, 0, 4, 0),
                ToolTip = tooltip
            };
        }

        private void EditMessage(Message message)
        {
            var editWindow = new Window
            {
                Title = "Редактировать сообщение",
                Width = 400,
                Height = 250,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Icon = new System.Windows.Media.Imaging.BitmapImage(
                    new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute)),
                Background = (Brush)FindResource("BackgroundBrush")
            };

            var stackPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Текст сообщения",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 10)
            });

            var textBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Text = message.Text,
                Height = 80,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(textBox);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var cancelBtn = new Button
            {
                Content = "Отмена",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.Transparent,
                BorderBrush = (Brush)FindResource("SecondaryTextBrush"),
                BorderThickness = new Thickness(1, 1, 1, 1),
                Cursor = Cursors.Hand
            };
            cancelBtn.Click += (s, ev) => editWindow.Close();

            var saveBtn = new Button
            {
                Content = "Сохранить",
                Style = (Style)FindResource("StandardButtonStyle"),
                Padding = new Thickness(15, 8, 15, 8)
            };
            saveBtn.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Сообщение не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                message.Text = textBox.Text;
                message.EditTime = DateTime.Now;
                editWindow.Close();
                LoadMessages();
            };

            buttonPanel.Children.Add(cancelBtn);
            buttonPanel.Children.Add(saveBtn);
            stackPanel.Children.Add(buttonPanel);

            editWindow.Content = stackPanel;
            editWindow.ShowDialog();
        }

        private void DeleteMessage(Message message)
        {
            bool isOwn = message.SenderId == App.CurrentUser.Id;
            bool isAdmin = currentChat.IsAdmin(App.CurrentUser.Id);

            if (!isOwn && !isAdmin) return;

            string confirmText = isOwn
                ? "Удалить ваше сообщение?"
                : $"Удалить сообщение пользователя {message.SenderName}? (права администратора)";

            var result = MessageBox.Show(confirmText, "Удаление сообщения",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentChat.Messages.Remove(message);
                LoadMessages();
                UpdatePinnedBar();
            }
        }

        private void TogglePinMessage(Message message)
        {
            if (!currentChat.IsAdmin(App.CurrentUser.Id)) return;

            message.IsPinned = !message.IsPinned;
            LoadMessages();
            UpdatePinnedBar();

            MessageBox.Show(
                message.IsPinned ? "Сообщение закреплено 📌" : "Сообщение откреплено",
                "Закрепление", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReplyToMessage(Message message)
        {
            _replyingTo = message;

            // Показываем панель ответа
            ReplyAuthorText.Text = "↩ Ответ на сообщение: " + message.SenderName;
            string preview = message.Text.Length > 60 ? message.Text.Substring(0, 60) + "..." : message.Text;
            ReplyPreviewText.Text = preview;
            ReplyPanel.Visibility = Visibility.Visible;

            MessageTextBox.Focus();
        }

        private void CancelReply_Click(object sender, RoutedEventArgs e)
        {
            _replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            if (currentChat == null) return;

            string text = MessageTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            var message = new Message
            {
                Id = currentChat.Messages.Count > 0 ? currentChat.Messages.Max(m => m.Id) + 1 : 1,
                Text = text,
                ChatId = currentChat.Id,
                SenderId = App.CurrentUser.Id,
                SenderName = App.CurrentUser.FullName,
                SendTime = DateTime.Now,
                ReplyId = _replyingTo?.Id,
                ReplyTo = _replyingTo
            };

            currentChat.Messages.Add(message);
            MessageTextBox.Clear();
            _replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;

            LoadMessages();
        }

        private void Emoji_Click(object sender, RoutedEventArgs e)
        {
            var emojiWindow = new EmojiPickerWindow();
            emojiWindow.Owner = this;
            if (emojiWindow.ShowDialog() == true)
            {
                MessageTextBox.Text += emojiWindow.SelectedEmoji;
                MessageTextBox.Focus();
            }
        }

        private void Members_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat != null)
            {
                var membersWindow = new MembersWindow(currentChat);
                membersWindow.Owner = this;
                membersWindow.ShowDialog();
            }
        }

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateChatDialog();
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                var newChat = new Chat
                {
                    Id = DataInitializer.Chats.Max(c => c.Id) + 1,
                    Name = dialog.ChatName,
                    TypeChat = dialog.IsGroup ? "Группа" : "Личный",
                    TypeGroup = dialog.IsWorkChat ? "Рабочая" : "Личная",
                    CreatedAt = DateTime.Now,
                    OwnerId = App.CurrentUser.Id,
                    Members = new System.Collections.Generic.List<ChatMember>
                    {
                        new ChatMember { ChatId = DataInitializer.Chats.Count + 1, UserId = App.CurrentUser.Id, Role = "Владелец" }
                    }
                };

                DataInitializer.Chats.Add(newChat);
                LoadChats();
            }
        }

        private void Kanban_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat != null)
            {
                var kanbanWindow = new KanbanWindow(currentChat);
                kanbanWindow.Owner = this;
                kanbanWindow.Show();
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            var profileWindow = new ProfileWindow();
            profileWindow.Owner = this;
            profileWindow.ShowDialog();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadChats();
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            showArchived = ShowArchivedCheckBox.IsChecked == true;
            LoadChats();
        }

        private void UpdateUserInfo()
        {
            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.FullName;
            }
        }
    }

    public class CreateChatDialog : Window
    {
        private TextBox nameTextBox;
        private RadioButton personalRadio;
        private RadioButton groupRadio;
        private RadioButton workRadio;

        public string ChatName { get; private set; }
        public bool IsGroup { get; private set; }
        public bool IsWorkChat { get; private set; }

        public CreateChatDialog()
        {
            Title = "Создать чат";
            Width = 400; Height = 320;
            MinWidth = 350; MinHeight = 280;
            Icon = new System.Windows.Media.Imaging.BitmapImage(
                new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute));
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = (Brush)FindResource("BackgroundBrush");

            var stackPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Название чата",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            nameTextBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(nameTextBox);

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Тип чата",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 10)
            });

            personalRadio = new RadioButton
            {
                Content = "Личный (2 участника)",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5),
                IsChecked = true
            };
            groupRadio = new RadioButton
            {
                Content = "Групповой (от 3 участников)",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            };
            workRadio = new RadioButton
            {
                Content = "Рабочий (с канбан-доской)",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 20)
            };

            stackPanel.Children.Add(personalRadio);
            stackPanel.Children.Add(groupRadio);
            stackPanel.Children.Add(workRadio);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.Transparent,
                BorderBrush = (Brush)FindResource("SecondaryTextBrush"),
                BorderThickness = new Thickness(1, 1, 1, 1),
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (s, e) => DialogResult = false;

            var createButton = new Button
            {
                Content = "Создать",
                Style = (Style)FindResource("StandardButtonStyle"),
                Padding = new Thickness(15, 8, 15, 8)
            };
            createButton.Click += CreateButton_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);
            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
        }



        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Введите название чата", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChatName = nameTextBox.Text;
            IsGroup = groupRadio.IsChecked == true || workRadio.IsChecked == true;
            IsWorkChat = workRadio.IsChecked == true;
            DialogResult = true;
        }

        
    }


}