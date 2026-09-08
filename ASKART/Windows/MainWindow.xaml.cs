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

        public MainWindow()
        {
            InitializeComponent();
            LoadChats();
            UpdateUserInfo();
        }

        private void LoadChats()
        {
            ChatListPanel.Children.Clear();

            foreach (var chat in DataInitializer.Chats)
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

            // Аватар
            var avatarBorder = new Border
            {
                Width = 45,
                Height = 45,
                CornerRadius = new CornerRadius(22.5),
                Background = (Brush)FindResource("ActiveBrush"),
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

            // Информация о чате
            var stackPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(15, 0, 0, 0)
            };

            var nameText = new TextBlock
            {
                Text = chat.Name,
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

            // Обработчик клика
            border.MouseLeftButtonDown += (s, e) => SelectChat(chat);

            return border;
        }

        private void SelectChat(Chat chat)
        {
            currentChat = chat;

            // Показываем область чата
            WelcomePanel.Visibility = Visibility.Collapsed;
            ChatHeader.Visibility = Visibility.Visible;
            MessagesScroll.Visibility = Visibility.Visible;
            InputArea.Visibility = Visibility.Visible;

            // Обновляем заголовок
            ChatNameText.Text = chat.Name;
            ChatTypeText.Text = chat.TypeGroup != null ?
                $"{chat.TypeChat} • {chat.TypeGroup}" :
                chat.TypeChat;

            // Показываем кнопку канбана для рабочих чатов
            KanbanButton.Visibility = chat.TypeGroup == "Рабочая" ?
                Visibility.Visible : Visibility.Collapsed;

            // Загружаем сообщения
            LoadMessages();

            // Подсвечиваем выбранный чат
            UpdateChatSelection();
        }

        private void UpdateChatSelection()
        {
            foreach (var child in ChatListPanel.Children)
            {
                if (child is Border border)
                {
                    // Сбрасываем выделение
                    border.Background = (Brush)FindResource("CardBackgroundBrush");

                    // Проверяем, это ли выбранный чат
                    if (border.Child is Grid grid &&
                        grid.Children.Count > 1 &&
                        grid.Children[1] is StackPanel sp &&
                        sp.Children.Count > 0 &&
                        sp.Children[0] is TextBlock tb &&
                        tb.Text == currentChat?.Name)
                    {
                        border.Background = (Brush)FindResource("ActiveBrush");
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

            if (message.SenderId == App.CurrentUser.Id)
            {
                border.Background = (Brush)FindResource("ActiveBrush");
            }
            else
            {
                border.Background = (Brush)FindResource("SecondaryBackgroundBrush");
            }

            var stackPanel = new StackPanel();

            var nameText = new TextBlock
            {
                Text = message.SenderName,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = message.SenderId == App.CurrentUser.Id ? Brushes.White : (Brush)FindResource("SecondaryTextBrush"),
                Margin = new Thickness(0, 0, 0, 5)
            };

            var messageText = new TextBlock
            {
                Text = message.Text,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 14,
                Foreground = message.SenderId == App.CurrentUser.Id ? Brushes.White : (Brush)FindResource("PrimaryTextBrush"),
                TextWrapping = TextWrapping.Wrap
            };

            var timeText = new TextBlock
            {
                Text = message.SendTime.ToString("HH:mm"),
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 11,
                Foreground = message.SenderId == App.CurrentUser.Id ?
                    new SolidColorBrush(Color.FromRgb(200, 240, 255)) : (Brush)FindResource("InactiveTextBrush"),
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(timeText);

            border.Child = stackPanel;

            return border;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat == null)
                return;

            string text = MessageTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(text))
                return;

            var message = new Message
            {
                Id = currentChat.Messages.Count > 0 ? currentChat.Messages.Max(m => m.Id) + 1 : 1,
                Text = text,
                ChatId = currentChat.Id,
                SenderId = App.CurrentUser.Id,
                SenderName = App.CurrentUser.FullName,
                SendTime = DateTime.Now
            };

            currentChat.Messages.Add(message);
            MessageTextBox.Clear();

            LoadMessages();
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
                    OwnerId = App.CurrentUser.Id,
                    Members = new List<ChatMember>
                    {
                        new ChatMember { ChatId = DataInitializer.Chats.Count + 1, UserId = App.CurrentUser.Id, Role = "Управляющий" }
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

        private void UpdateUserInfo()
        {
            if (App.CurrentUser != null)
            {
                UserNameText.Text = App.CurrentUser.FullName;
            }
        }
    }

    // Диалог создания чата
    public class CreateChatDialog : Window
    {
        private TextBox nameTextBox;
        private CheckBox isGroupCheckBox;
        private CheckBox isWorkCheckBox;

        public string ChatName { get; private set; }
        public bool IsGroup { get; private set; }
        public bool IsWorkChat { get; private set; }

        public CreateChatDialog()
        {
            Title = "Создать чат";
            Width = 400;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = (Brush)FindResource("BackgroundBrush");

            var stackPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            var nameLabel = new TextBlock
            {
                Text = "Название чата",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            };

            nameTextBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Margin = new Thickness(0, 0, 0, 15)
            };

            isGroupCheckBox = new CheckBox
            {
                Content = "Групповой чат",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            isWorkCheckBox = new CheckBox
            {
                Content = "Рабочий чат (с канбан-доской)",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 20)
            };

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
                Style = (Style)FindResource("PrimaryButtonStyle"),
                Padding = new Thickness(15, 8, 15, 8)
            };
            createButton.Click += CreateButton_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);

            stackPanel.Children.Add(nameLabel);
            stackPanel.Children.Add(nameTextBox);
            stackPanel.Children.Add(isGroupCheckBox);
            stackPanel.Children.Add(isWorkCheckBox);
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
            IsGroup = isGroupCheckBox.IsChecked == true;
            IsWorkChat = isWorkCheckBox.IsChecked == true;
            DialogResult = true;
        }
    }
}