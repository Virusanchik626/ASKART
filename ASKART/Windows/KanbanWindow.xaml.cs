using Askart.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Askart.Windows
{
    public partial class KanbanWindow : Window
    {
        private Chat currentChat;

        public KanbanWindow(Chat chat)
        {
            InitializeComponent();
            currentChat = chat;
            ChatNameText.Text = chat.Name;
            LoadTasks();
        }

        private void LoadTasks()
        {
            NewTasksPanel.Children.Clear();
            InProgressTasksPanel.Children.Clear();
            CompletedTasksPanel.Children.Clear();

            foreach (var task in currentChat.Tasks)
            {
                var taskCard = CreateTaskCard(task);

                switch (task.Status)
                {
                    case "Новая":
                        NewTasksPanel.Children.Add(taskCard);
                        break;
                    case "В работе":
                        InProgressTasksPanel.Children.Add(taskCard);
                        break;
                    case "Выполнена":
                        CompletedTasksPanel.Children.Add(taskCard);
                        break;
                }
            }
        }

        private Border CreateTaskCard(TaskItem task)
        {
            var border = new Border
            {
                Background = (Brush)FindResource("CardBackgroundBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 0, 0, 10),
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.1
                }
            };

            var stackPanel = new StackPanel();

            var nameText = new TextBlock
            {
                Text = task.Name,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 5)
            };

            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                var descText = new TextBlock
                {
                    Text = task.Description,
                    FontFamily = (FontFamily)FindResource("Roboto"),
                    FontSize = 12,
                    Foreground = (Brush)FindResource("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 8)
                };
                stackPanel.Children.Add(descText);
            }

            var priorityText = new TextBlock
            {
                Text = $"Приоритет: {task.Priority}",
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 11,
                Foreground = task.Priority == "Высокий" ? (Brush)FindResource("ErrorBrush") :
                            task.Priority == "Средний" ? Brushes.Orange : (Brush)FindResource("SuccessBrush"),
                Margin = new Thickness(0, 0, 0, 3)
            };

            var assigneeText = new TextBlock
            {
                Text = $"Исполнитель: {task.AssigneeName}",
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 11,
                Foreground = (Brush)FindResource("SecondaryTextBrush"),
                Margin = new Thickness(0, 0, 0, 8)
            };

            var deadlineText = new TextBlock
            {
                Text = $"Срок: {task.Deadline:dd.MM.yyyy}",
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 11,
                Foreground = (Brush)FindResource("InactiveTextBrush"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var buttonPanel = new StackPanel();

            if (task.Status == "Новая")
            {
                var startButton = new Button
                {
                    Content = "Начать работу",
                    Style = (Style)FindResource("PrimaryButtonStyle"),
                    Padding = new Thickness(10, 5, 10, 5),
                    FontSize = 12
                };
                startButton.Click += (s, e) => ChangeTaskStatus(task, "В работе");
                buttonPanel.Children.Add(startButton);
            }
            else if (task.Status == "В работе")
            {
                var completeButton = new Button
                {
                    Content = "Завершить",
                    Background = (Brush)FindResource("SuccessBrush"),
                    Foreground = Brushes.White,
                    Padding = new Thickness(10, 5, 10, 5),
                    BorderThickness = new Thickness(0),
                    FontFamily = (FontFamily)FindResource("Roboto"),
                    FontSize = 12,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                completeButton.Click += (s, e) => ChangeTaskStatus(task, "Выполнена");
                buttonPanel.Children.Add(completeButton);
            }

            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(priorityText);
            stackPanel.Children.Add(assigneeText);
            stackPanel.Children.Add(deadlineText);
            stackPanel.Children.Add(buttonPanel);

            border.Child = stackPanel;

            return border;
        }

        private void ChangeTaskStatus(TaskItem task, string newStatus)
        {
            task.Status = newStatus;
            LoadTasks();
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateTaskDialog();
            if (dialog.ShowDialog() == true)
            {
                var newTask = new TaskItem
                {
                    Id = currentChat.Tasks.Count > 0 ? currentChat.Tasks.Max(t => t.Id) + 1 : 1,
                    ChatId = currentChat.Id,
                    Name = dialog.TaskName,
                    Description = dialog.Description,
                    Status = "Новая",
                    Deadline = dialog.Deadline,
                    Priority = dialog.Priority,
                    CreatorId = App.CurrentUser.Id,
                    AssigneeUserId = App.CurrentUser.Id,
                    AssigneeName = App.CurrentUser.FullName
                };

                currentChat.Tasks.Add(newTask);
                LoadTasks();
            }
        }
    }

    public class CreateTaskDialog : Window
    {
        private TextBox nameTextBox;
        private TextBox descTextBox;
        private ComboBox priorityComboBox;
        private DatePicker deadlinePicker;

        public string TaskName { get; private set; }
        public string Description { get; private set; }
        public string Priority { get; private set; }
        public DateTime Deadline { get; private set; }

        public CreateTaskDialog()
        {
            Title = "Новая задача";
            Width = 450;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = (Brush)FindResource("BackgroundBrush");

            var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var stackPanel = new StackPanel { Margin = new Thickness(20) };

            // Название
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Название задачи",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            nameTextBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(nameTextBox);

            // Описание
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Описание",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            descTextBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Height = 80,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(descTextBox);

            // Приоритет
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Приоритет",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            priorityComboBox = new ComboBox
            {
                Style = (Style)FindResource("ComboBoxStyle"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            priorityComboBox.Items.Add("Низкий");
            priorityComboBox.Items.Add("Средний");
            priorityComboBox.Items.Add("Высокий");
            priorityComboBox.SelectedIndex = 1;
            stackPanel.Children.Add(priorityComboBox);

            // Срок
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Срок выполнения",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            deadlinePicker = new DatePicker
            {
                FontFamily = (FontFamily)FindResource("Roboto"),
                Padding = new Thickness(15, 12, 15, 12),
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 20)
            };
            deadlinePicker.SelectedDate = DateTime.Now.AddDays(7);
            stackPanel.Children.Add(deadlinePicker);

            // Кнопки
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            var cancelButton = new Button
            {
                Content = "Отмена",
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 10, 0),
                Background = Brushes.Transparent,
                BorderBrush = (Brush)FindResource("SecondaryTextBrush"),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand
            };
            cancelButton.Click += (s, ev) => DialogResult = false;

            var createButton = new Button
            {
                Content = "Создать",
                Style = (Style)FindResource("PrimaryButtonStyle"),
                Padding = new Thickness(15, 8, 15, 8)
            };
            createButton.Click += CreateButton_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);
            stackPanel.Children.Add(buttonPanel);

            scrollViewer.Content = stackPanel;
            Content = scrollViewer;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Введите название задачи", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (deadlinePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите срок выполнения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaskName = nameTextBox.Text;
            Description = descTextBox.Text;
            Priority = priorityComboBox.SelectedItem.ToString();
            Deadline = deadlinePicker.SelectedDate.Value;
            DialogResult = true;
        }
    }
}