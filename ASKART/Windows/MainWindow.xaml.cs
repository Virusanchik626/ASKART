using Askart.Models;
using Askart.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Askart.Windows
{
    public partial class MainWindow : Window
    {
        private Chat currentChat;
        private Message replyingTo = null;
        private bool showArchived = false;
        private KanbanWindow kanbanWindow = null;

        public MainWindow()
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            LoadChats();
            UpdateUserInfo();
        }

        
        
        

        
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    return;
                }
                e.Handled = true;
                SendMessage();
            }
        }

        
        
        

        private void LoadChats()
        {
            ChatListPanel.Children.Clear();

            string searchText = "";
            if (SearchTextBox != null)
            {
                searchText = SearchTextBox.Text.Trim();
            }

            
            List<Chat> chats = DataService.GetMyChats(App.CurrentUser.Id, showArchived, searchText);

            foreach (Chat chat in chats)
            {
                Border chatCard = CreateChatCard(chat);
                ChatListPanel.Children.Add(chatCard);
            }

            UpdateChatSelection();
        }

        private Border CreateChatCard(Chat chat)
        {
            Border border = new Border();
            border.Style = (Style)FindResource("ChatCardStyle");
            border.Cursor = Cursors.Hand;
            border.Tag = chat; 

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            
            Border avatarBorder = new Border();
            avatarBorder.Width = 45;
            avatarBorder.Height = 45;
            avatarBorder.CornerRadius = new CornerRadius(22.5);
            avatarBorder.Background = new SolidColorBrush(Color.FromRgb(100, 150, 200));
            avatarBorder.VerticalAlignment = VerticalAlignment.Center;
            avatarBorder.HorizontalAlignment = HorizontalAlignment.Center;

            TextBlock avatarText = new TextBlock();
            avatarText.Text = chat.Name.Substring(0, 1).ToUpper();
            avatarText.FontSize = 20;
            avatarText.FontWeight = FontWeights.Bold;
            avatarText.Foreground = Brushes.White;
            avatarText.VerticalAlignment = VerticalAlignment.Center;
            avatarText.HorizontalAlignment = HorizontalAlignment.Center;
            avatarBorder.Child = avatarText;
            Grid.SetColumn(avatarBorder, 0);

            StackPanel stackPanel = new StackPanel();
            stackPanel.VerticalAlignment = VerticalAlignment.Center;
            stackPanel.Margin = new Thickness(15, 0, 0, 0);

            string chatName = chat.Name;
            if (chat.IsArchived)
            {
                chatName = chatName + " 📦";
            }
            TextBlock nameText = UiHelper.MakeText(chatName, 14, "PrimaryTextBrush");
            nameText.FontWeight = FontWeights.SemiBold;

            string typeName = chat.TypeChat;
            if (chat.TypeGroup != null)
            {
                typeName = chat.TypeGroup;
            }
            TextBlock typeText = UiHelper.MakeText(typeName, 12, "SecondaryTextBrush");

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(typeText);
            Grid.SetColumn(stackPanel, 1);

            grid.Children.Add(avatarBorder);
            grid.Children.Add(stackPanel);
            border.Child = grid;

            border.MouseLeftButtonDown += ChatCard_MouseDown;
            return border;
        }

        
        private void ChatCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            Chat chat = border.Tag as Chat;
            if (chat != null)
            {
                SelectChat(chat);
            }
        }

        
        private void UpdateChatSelection()
        {
            foreach (UIElement child in ChatListPanel.Children)
            {
                Border border = child as Border;
                if (border == null)
                {
                    continue;
                }

                Chat chat = border.Tag as Chat;
                if (currentChat != null && chat != null && chat.Id == currentChat.Id)
                {
                    border.SetResourceReference(Border.BackgroundProperty, "ActiveBrush");
                }
                else
                {
                    
                    border.ClearValue(Border.BackgroundProperty);
                }
            }
        }

        private void SelectChat(Chat chat)
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

            UpdatePinnedBar();
            LoadMessages();
            UpdateChatSelection();
        }

        
        
        

        private void UpdatePinnedBar()
        {
            List<Message> pinnedMessages = DataService.GetPinnedMessages(currentChat);

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
            ChatStatisticsWindow window = new ChatStatisticsWindow(currentChat);
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

        private void Archive_Click(object sender, RoutedEventArgs e)
        {
            if (currentChat == null)
            {
                return;
            }

            bool isAdmin = currentChat.IsAdmin(App.CurrentUser.Id);
            if (isAdmin == false)
            {
                MessageBox.Show("Только администратор может архивировать чат", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string action = "архивировать";
            if (currentChat.IsArchived)
            {
                action = "разархивировать";
            }

            MessageBoxResult result = MessageBox.Show(
                "Вы действительно хотите " + action + " чат \"" + currentChat.Name + "\"?",
                "Архивация", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DataService.SetArchived(currentChat, !currentChat.IsArchived);
                LoadChats();
                SelectChat(currentChat);
            }
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

        
        
        

        private void LoadMessages()
        {
            MessagesPanel.Children.Clear();

            List<Message> messages = DataService.GetMessages(currentChat);

            foreach (Message message in messages)
            {
                Border messageBlock = CreateMessageBlock(message);
                MessagesPanel.Children.Add(messageBlock);
            }

            MessagesScroll.ScrollToEnd();
        }

        private Border CreateMessageBlock(Message message)
        {
            bool isOwn = (message.SenderId == App.CurrentUser.Id);
            bool isAdmin = currentChat.IsAdmin(App.CurrentUser.Id);

            Border border = new Border();
            border.Margin = new Thickness(0, 5, 0, 5);
            border.Padding = new Thickness(15, 10, 15, 10);
            border.CornerRadius = new CornerRadius(12);
            border.MaxWidth = 500;

            if (isOwn)
            {
                border.HorizontalAlignment = HorizontalAlignment.Right;
                border.SetResourceReference(Border.BackgroundProperty, "ActiveBrush");
            }
            else
            {
                border.HorizontalAlignment = HorizontalAlignment.Left;
                border.SetResourceReference(Border.BackgroundProperty, "SecondaryBackgroundBrush");
            }

            if (message.IsPinned)
            {
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 193, 7));
                border.BorderThickness = new Thickness(2);
            }

            StackPanel stackPanel = new StackPanel();

            
            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            string senderText = message.SenderName;
            if (message.IsPinned)
            {
                senderText = senderText + " 📌";
            }
            TextBlock nameText = UiHelper.MakeText(senderText, 12, "SecondaryTextBrush");
            nameText.FontWeight = FontWeights.SemiBold;
            nameText.Margin = new Thickness(0, 0, 0, 5);
            nameText.VerticalAlignment = VerticalAlignment.Center;
            if (isOwn)
            {
                nameText.Foreground = Brushes.White;
            }
            Grid.SetColumn(nameText, 0);
            headerGrid.Children.Add(nameText);

            if (isOwn || isAdmin)
            {
                StackPanel actionsPanel = new StackPanel();
                actionsPanel.Orientation = Orientation.Horizontal;
                actionsPanel.VerticalAlignment = VerticalAlignment.Center;

                if (isOwn)
                {
                    Button editButton = CreateActionButton("✏️", "Редактировать", message, isOwn, "SecondaryTextBrush");
                    editButton.Click += EditMessage_Click;
                    actionsPanel.Children.Add(editButton);
                }

                string deleteTip = "Удалить";
                if (isAdmin && isOwn == false)
                {
                    deleteTip = "Удалить (админ)";
                }
                Button deleteButton = CreateActionButton("✕", deleteTip, message, isOwn, "ErrorBrush");
                deleteButton.Click += DeleteMessage_Click;
                actionsPanel.Children.Add(deleteButton);

                if (isAdmin)
                {
                    string pinIcon = "📌";
                    string pinTip = "Закрепить";
                    if (message.IsPinned)
                    {
                        pinIcon = "📍";
                        pinTip = "Открепить";
                    }
                    Button pinButton = CreateActionButton(pinIcon, pinTip, message, isOwn, "SecondaryTextBrush");
                    pinButton.Click += TogglePin_Click;
                    actionsPanel.Children.Add(pinButton);
                }

                Button replyButton = CreateActionButton("↩", "Ответить", message, isOwn, "SecondaryTextBrush");
                replyButton.Click += Reply_Click;
                actionsPanel.Children.Add(replyButton);

                Grid.SetColumn(actionsPanel, 1);
                headerGrid.Children.Add(actionsPanel);
            }

            stackPanel.Children.Add(headerGrid);

            
            if (message.ReplyId.HasValue)
            {
                Message replied = DataService.GetMessageById(currentChat, message.ReplyId.Value);
                if (replied != null)
                {
                    Border replyBorder = new Border();
                    replyBorder.Background = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255));
                    replyBorder.CornerRadius = new CornerRadius(6);
                    replyBorder.Padding = new Thickness(8, 6, 8, 6);
                    replyBorder.Margin = new Thickness(0, 5, 0, 5);
                    replyBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 200, 255));
                    replyBorder.BorderThickness = new Thickness(2, 0, 0, 0);

                    TextBlock replyText = UiHelper.MakeText("↩ " + replied.SenderName + ": " + replied.Text, 11, "SecondaryTextBrush");
                    replyText.FontStyle = FontStyles.Italic;
                    replyText.TextWrapping = TextWrapping.Wrap;
                    replyText.MaxHeight = 40;
                    if (isOwn)
                    {
                        replyText.Foreground = new SolidColorBrush(Color.FromRgb(200, 240, 255));
                    }

                    replyBorder.Child = replyText;
                    stackPanel.Children.Add(replyBorder);
                }
            }

            
            TextBlock messageText = UiHelper.MakeText(message.Text, 14, "PrimaryTextBrush");
            messageText.TextWrapping = TextWrapping.Wrap;
            if (isOwn)
            {
                messageText.Foreground = Brushes.White;
            }
            stackPanel.Children.Add(messageText);

            
            string timeString = message.SendTime.ToString("HH:mm");
            if (message.EditTime.HasValue)
            {
                timeString = timeString + " (ред.)";
            }
            TextBlock timeText = UiHelper.MakeText(timeString, 11, "InactiveTextBrush");
            timeText.HorizontalAlignment = HorizontalAlignment.Right;
            timeText.Margin = new Thickness(0, 5, 0, 0);
            if (isOwn)
            {
                timeText.Foreground = new SolidColorBrush(Color.FromRgb(200, 240, 255));
            }
            stackPanel.Children.Add(timeText);

            border.Child = stackPanel;

            
            ContextMenu contextMenu = new ContextMenu();

            MenuItem copyItem = CreateMenuItem("Копировать", message);
            copyItem.Click += CopyMessage_Click;
            contextMenu.Items.Add(copyItem);

            MenuItem replyItem = CreateMenuItem("↩ Ответить", message);
            replyItem.Click += Reply_Click;
            contextMenu.Items.Add(replyItem);

            if (isOwn)
            {
                MenuItem editItem = CreateMenuItem("✏️ Редактировать", message);
                editItem.Click += EditMessage_Click;
                contextMenu.Items.Add(editItem);
            }

            if (isOwn || isAdmin)
            {
                MenuItem deleteItem = CreateMenuItem("✕ Удалить", message);
                deleteItem.Click += DeleteMessage_Click;
                contextMenu.Items.Add(deleteItem);
            }

            if (isAdmin)
            {
                string pinHeader = "📌 Закрепить";
                if (message.IsPinned)
                {
                    pinHeader = "📍 Открепить";
                }
                MenuItem pinItem = CreateMenuItem(pinHeader, message);
                pinItem.Click += TogglePin_Click;
                contextMenu.Items.Add(pinItem);
            }

            border.ContextMenu = contextMenu;
            return border;
        }

        
        
        private Button CreateActionButton(string content, string tooltip, Message message, bool isOwn, string otherBrushKey)
        {
            Button button = UiHelper.MakeIconButton(content, 12, tooltip);
            button.Tag = message;

            if (isOwn)
            {
                button.Foreground = Brushes.White;
            }
            else
            {
                button.SetResourceReference(Control.ForegroundProperty, otherBrushKey);
            }
            return button;
        }

        private MenuItem CreateMenuItem(string header, Message message)
        {
            MenuItem item = new MenuItem();
            item.Header = header;
            item.Tag = message;
            return item;
        }

        
        private Message GetMessageFromSender(object sender)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return null;
            }
            return element.Tag as Message;
        }

        

        private void EditMessage_Click(object sender, RoutedEventArgs e)
        {
            Message message = GetMessageFromSender(sender);
            if (message != null)
            {
                EditMessage(message);
            }
        }

        private void DeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            Message message = GetMessageFromSender(sender);
            if (message != null)
            {
                DeleteMessage(message);
            }
        }

        private void TogglePin_Click(object sender, RoutedEventArgs e)
        {
            Message message = GetMessageFromSender(sender);
            if (message != null)
            {
                TogglePinMessage(message);
            }
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            Message message = GetMessageFromSender(sender);
            if (message != null)
            {
                ReplyToMessage(message);
            }
        }

        private void CopyMessage_Click(object sender, RoutedEventArgs e)
        {
            Message message = GetMessageFromSender(sender);
            if (message != null)
            {
                Clipboard.SetText(message.Text);
            }
        }

        

        private void EditMessage(Message message)
        {
            EditMessageDialog dialog = new EditMessageDialog(message.Text);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                DataService.EditMessage(message, dialog.NewText);
                LoadMessages();
                UpdatePinnedBar();
            }
        }

        private void DeleteMessage(Message message)
        {
            bool isOwn = (message.SenderId == App.CurrentUser.Id);
            bool isAdmin = currentChat.IsAdmin(App.CurrentUser.Id);

            if (isOwn == false && isAdmin == false)
            {
                return;
            }

            string confirmText = "Удалить ваше сообщение?";
            if (isOwn == false)
            {
                confirmText = "Удалить сообщение пользователя " + message.SenderName + "? (права администратора)";
            }

            MessageBoxResult result = MessageBox.Show(confirmText, "Удаление сообщения",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DataService.DeleteMessage(currentChat, message);
                LoadMessages();
                UpdatePinnedBar();
            }
        }

        private void TogglePinMessage(Message message)
        {
            if (currentChat.IsAdmin(App.CurrentUser.Id) == false)
            {
                return;
            }

            DataService.SetPinned(message, !message.IsPinned);
            LoadMessages();
            UpdatePinnedBar();

            if (message.IsPinned)
            {
                MessageBox.Show("Сообщение закреплено 📌", "Закрепление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Сообщение откреплено", "Закрепление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ReplyToMessage(Message message)
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

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
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

            DataService.SendMessage(currentChat, App.CurrentUser, text, replyingTo);

            MessageTextBox.Clear();
            replyingTo = null;
            ReplyPanel.Visibility = Visibility.Collapsed;

            LoadMessages();
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

        
        
        

        private void CreateChat_Click(object sender, RoutedEventArgs e)
        {
            CreateChatDialog dialog = new CreateChatDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                DataService.CreateChat(dialog.ChatName, dialog.IsGroup, dialog.IsWorkChat, App.CurrentUser);
                LoadChats();
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow profileWindow = new ProfileWindow();
            profileWindow.Owner = this;
            profileWindow.ShowDialog();

            
            
            if (profileWindow.IsLoggedOut)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadChats();
        }

        private void FilterChanged(object sender, RoutedEventArgs e)
        {
            showArchived = (ShowArchivedCheckBox.IsChecked == true);
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
            ThemeHelper.Watch(this);

            Title = "Создать чат";
            Width = 400;
            Height = 320;
            MinWidth = 350;
            MinHeight = 280;
            Icon = UiHelper.GetLogo();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");

            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(20);

            TextBlock nameLabel = UiHelper.MakeText("Название чата", 13, "PrimaryTextBrush");
            nameLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(nameLabel);

            nameTextBox = new TextBox();
            nameTextBox.Style = (Style)FindResource("InputTextBoxStyle");
            nameTextBox.Margin = new Thickness(0, 0, 0, 15);
            stackPanel.Children.Add(nameTextBox);

            TextBlock typeLabel = UiHelper.MakeText("Тип чата", 13, "PrimaryTextBrush");
            typeLabel.Margin = new Thickness(0, 0, 0, 10);
            stackPanel.Children.Add(typeLabel);

            personalRadio = new RadioButton();
            personalRadio.Content = "Личный (2 участника)";
            personalRadio.FontFamily = UiHelper.GetFont();
            personalRadio.Margin = new Thickness(0, 0, 0, 5);
            personalRadio.IsChecked = true;

            groupRadio = new RadioButton();
            groupRadio.Content = "Групповой (от 3 участников)";
            groupRadio.FontFamily = UiHelper.GetFont();
            groupRadio.Margin = new Thickness(0, 0, 0, 5);

            workRadio = new RadioButton();
            workRadio.Content = "Рабочий (с канбан-доской)";
            workRadio.FontFamily = UiHelper.GetFont();
            workRadio.Margin = new Thickness(0, 0, 0, 20);

            stackPanel.Children.Add(personalRadio);
            stackPanel.Children.Add(groupRadio);
            stackPanel.Children.Add(workRadio);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;

            Button cancelButton = UiHelper.MakeCancelButton();
            cancelButton.Click += Cancel_Click;

            Button createButton = UiHelper.MakeMainButton("Создать");
            createButton.Click += CreateButton_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);
            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Введите название чата", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ChatName = nameTextBox.Text.Trim();
            IsGroup = (groupRadio.IsChecked == true || workRadio.IsChecked == true);
            IsWorkChat = (workRadio.IsChecked == true);
            DialogResult = true;
        }
    }

    
    
    
    public class EditMessageDialog : Window
    {
        private TextBox textBox;

        public string NewText { get; private set; }

        public EditMessageDialog(string oldText)
        {
            ThemeHelper.Watch(this);

            Title = "Редактировать сообщение";
            Width = 400;
            Height = 250;
            Icon = UiHelper.GetLogo();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");

            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(20);

            TextBlock label = UiHelper.MakeText("Текст сообщения", 13, "PrimaryTextBrush");
            label.Margin = new Thickness(0, 0, 0, 10);
            stackPanel.Children.Add(label);

            textBox = new TextBox();
            textBox.Style = (Style)FindResource("InputTextBoxStyle");
            textBox.Text = oldText;
            textBox.Height = 80;
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.AcceptsReturn = true;
            textBox.Margin = new Thickness(0, 0, 0, 20);
            stackPanel.Children.Add(textBox);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;

            Button cancelButton = UiHelper.MakeCancelButton();
            cancelButton.Click += Cancel_Click;

            Button saveButton = UiHelper.MakeMainButton("Сохранить");
            saveButton.Click += Save_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(saveButton);
            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show("Сообщение не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            NewText = textBox.Text;
            DialogResult = true;
        }
    }
}
