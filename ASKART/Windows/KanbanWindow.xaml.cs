using Askart.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Askart.Windows
{
    public partial class KanbanWindow : Window
    {
        private Chat currentChat;
        private TaskItem draggedTask;
        private const double ColumnWidth = 300;

        public KanbanWindow(Chat chat)
        {
            InitializeComponent();
            currentChat = chat;
            ChatNameText.Text = chat.Name;
            LoadTasks();
        }

        private void LoadTasks()
        {
            ColumnsPanel.Children.Clear();

            foreach (var column in currentChat.KanbanColumns)
            {
                var columnBorder = CreateColumn(column);
                ColumnsPanel.Children.Add(columnBorder);

                var tasksInStatus = currentChat.Tasks.Where(t => t.Status == column.Name).ToList();
                var stackPanel = FindStackPanelInColumn(columnBorder);

                foreach (var task in tasksInStatus)
                {
                    var taskCard = CreateTaskCard(task);
                    stackPanel.Children.Add(taskCard);
                }
            }
        }

        private Border CreateColumn(KanbanColumn column)
        {
            var border = new Border
            {
                Background = (Brush)FindResource("SecondaryBackgroundBrush"),
                CornerRadius = new CornerRadius(12),
                Width = ColumnWidth,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Stretch
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Заголовок колонки с настраиваемым цветом
            var header = new Border
            {
                Padding = new Thickness(15, 15, 15, 15),
                Background = HexToBrush(column.Color),
                CornerRadius = new CornerRadius(12, 12, 0, 0)
            };

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var headerText = new TextBlock
            {
                Text = column.Name,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(headerText, 0);

            // Кнопка быстрой задачи (+)
            var quickAddButton = new Button
            {
                Content = "+",
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Tag = column.Name,
                Width = 28,
                Height = 28,
                Margin = new Thickness(0, 0, 5, 0)
            };
            quickAddButton.Click += QuickAddTask_Click;
            Grid.SetColumn(quickAddButton, 1);

            // Кнопка настроек (⚙)
            var settingsButton = new Button
            {
                Content = "⚙",
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                Cursor = Cursors.Hand,
                Tag = column,
                Width = 28,
                Height = 28,
                Margin = new Thickness(0, 0, 5, 0)
            };
            settingsButton.Click += ColumnSettings_Click;
            Grid.SetColumn(settingsButton, 2);

            // Кнопка перемещения влево (←)
            var moveLeftButton = new Button
            {
                Content = "←",
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Tag = column.Name,
                Width = 28,
                Height = 28,
                Margin = new Thickness(0, 0, 5, 0)
            };
            moveLeftButton.Click += MoveColumnLeft_Click;
            Grid.SetColumn(moveLeftButton, 3);

            // Кнопка перемещения вправо (→)
            var moveRightButton = new Button
            {
                Content = "→",
                Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                Tag = column.Name,
                Width = 28,
                Height = 28,
                Margin = new Thickness(0, 0, 0, 0)
            };
            moveRightButton.Click += MoveColumnRight_Click;
            Grid.SetColumn(moveRightButton, 4);

            headerGrid.Children.Add(headerText);
            headerGrid.Children.Add(quickAddButton);
            headerGrid.Children.Add(settingsButton);
            headerGrid.Children.Add(moveLeftButton);
            headerGrid.Children.Add(moveRightButton);
            header.Child = headerGrid;

            Grid.SetRow(header, 0);

            // Кнопка удаления колонки
            var deleteBorder = new Border
            {
                Background = (Brush)FindResource("SecondaryBackgroundBrush"),
                Padding = new Thickness(15, 10, 15, 10),
                BorderBrush = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };

            var deleteButton = new Button
            {
                Content = "✕ Удалить колонку",
                Background = Brushes.Transparent,
                Foreground = (Brush)FindResource("ErrorBrush"),
                BorderThickness = new Thickness(0),
                FontSize = 12,
                Cursor = Cursors.Hand,
                Tag = column.Name,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            deleteButton.Click += DeleteColumn_Click;
            deleteBorder.Child = deleteButton;

            Grid.SetRow(deleteBorder, 1);

            // Область для задач с drag-and-drop
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                AllowDrop = true,
                Tag = column.Name
            };
            scrollViewer.Drop += TaskDrop;
            scrollViewer.DragOver += TaskDragOver;

            var panel = new StackPanel { Margin = new Thickness(10, 10, 10, 10) };
            scrollViewer.Content = panel;

            Grid.SetRow(scrollViewer, 2);

            grid.Children.Add(header);
            grid.Children.Add(deleteBorder);
            grid.Children.Add(scrollViewer);
            border.Child = grid;

            return border;
        }

        // Преобразование HEX в Brush
        private Brush HexToBrush(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                return new SolidColorBrush(Color.FromRgb(150, 150, 150));

            hex = hex.Replace("#", "");

            byte a = 255;
            byte r = 150, g = 150, b = 150;

            if (hex.Length == 6)
            {
                r = Convert.ToByte(hex.Substring(0, 2), 16);
                g = Convert.ToByte(hex.Substring(2, 2), 16);
                b = Convert.ToByte(hex.Substring(4, 2), 16);
            }
            else if (hex.Length == 8)
            {
                a = Convert.ToByte(hex.Substring(0, 2), 16);
                r = Convert.ToByte(hex.Substring(2, 2), 16);
                g = Convert.ToByte(hex.Substring(4, 2), 16);
                b = Convert.ToByte(hex.Substring(6, 2), 16);
            }

            return new SolidColorBrush(Color.FromArgb(a, r, g, b));
        }

        private string BrushToHex(Brush brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                var color = solidBrush.Color;
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }
            return "#9E9E9E";
        }

        private StackPanel FindStackPanelInColumn(Border column)
        {
            if (column.Child is Grid grid && grid.Children.Count > 2)
            {
                if (grid.Children[2] is ScrollViewer scrollViewer)
                {
                    return scrollViewer.Content as StackPanel;
                }
            }
            return null;
        }

        private Border CreateTaskCard(TaskItem task)
        {
            var border = new Border
            {
                Background = (Brush)FindResource("CardBackgroundBrush"),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12, 12, 12, 12),
                Margin = new Thickness(0, 0, 0, 10),
                Cursor = Cursors.Hand,
                Effect = new DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.1
                }
            };

            var stackPanel = new StackPanel();

            var headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = task.Name,
                FontFamily = (FontFamily)FindResource("Roboto"),
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryTextBrush"),
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameText, 0);

            var deleteButton = new Button
            {
                Content = "✕",
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = (Brush)FindResource("ErrorBrush"),
                FontSize = 16,
                Cursor = Cursors.Hand,
                Padding = new Thickness(5, 0, 5, 0)
            };
            deleteButton.Click += (s, e) => DeleteTask(task);
            Grid.SetColumn(deleteButton, 1);

            headerGrid.Children.Add(nameText);
            headerGrid.Children.Add(deleteButton);

            stackPanel.Children.Add(headerGrid);

            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                var descText = new TextBlock
                {
                    Text = task.Description,
                    FontFamily = (FontFamily)FindResource("Roboto"),
                    FontSize = 12,
                    Foreground = (Brush)FindResource("SecondaryTextBrush"),
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 5, 0, 8)
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

            stackPanel.Children.Add(priorityText);
            stackPanel.Children.Add(assigneeText);
            stackPanel.Children.Add(deadlineText);

            border.Child = stackPanel;
            border.Tag = task;

            border.MouseLeftButtonDown += (s, e) =>
            {
                draggedTask = task;
                DragDrop.DoDragDrop(border, task, DragDropEffects.Move);
            };

            return border;
        }

        private void DeleteTask(TaskItem task)
        {
            var result = MessageBox.Show($"Удалить задачу \"{task.Name}\"?", "Удаление задачи",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                currentChat.Tasks.Remove(task);
                LoadTasks();
            }
        }

        private void DeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string columnName)
            {
                var result = MessageBox.Show($"Удалить колонку \"{columnName}\" и все её задачи?", "Удаление колонки",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var tasksToRemove = currentChat.Tasks.Where(t => t.Status == columnName).ToList();
                    foreach (var task in tasksToRemove)
                    {
                        currentChat.Tasks.Remove(task);
                    }

                    var columnToRemove = currentChat.KanbanColumns.FirstOrDefault(c => c.Name == columnName);
                    if (columnToRemove != null)
                    {
                        currentChat.KanbanColumns.Remove(columnToRemove);
                    }

                    LoadTasks();
                }
            }
        }

        private void MoveColumnLeft_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string columnName)
            {
                int index = currentChat.KanbanColumns.FindIndex(c => c.Name == columnName);
                if (index > 0)
                {
                    var column = currentChat.KanbanColumns[index];
                    currentChat.KanbanColumns.RemoveAt(index);
                    currentChat.KanbanColumns.Insert(index - 1, column);
                    LoadTasks();
                }
            }
        }

        private void MoveColumnRight_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string columnName)
            {
                int index = currentChat.KanbanColumns.FindIndex(c => c.Name == columnName);
                if (index < currentChat.KanbanColumns.Count - 1)
                {
                    var column = currentChat.KanbanColumns[index];
                    currentChat.KanbanColumns.RemoveAt(index);
                    currentChat.KanbanColumns.Insert(index + 1, column);
                    LoadTasks();
                }
            }
        }

        // Настройки колонки (цвет и название)
        private void ColumnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is KanbanColumn column)
            {
                var settingsWindow = new ColumnSettingsWindow(column);
                settingsWindow.Owner = this;
                if (settingsWindow.ShowDialog() == true)
                {
                    LoadTasks();
                }
            }
        }

        // Быстрое создание задачи в конкретной колонке
        private void QuickAddTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string columnName)
            {
                var dialog = new CreateTaskDialog(columnName);
                dialog.Owner = this;
                if (dialog.ShowDialog() == true)
                {
                    var newTask = new TaskItem
                    {
                        Id = currentChat.Tasks.Count > 0 ? currentChat.Tasks.Max(t => t.Id) + 1 : 1,
                        ChatId = currentChat.Id,
                        Name = dialog.TaskName,
                        Description = dialog.Description,
                        Status = dialog.SelectedColumn,
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

        private void TaskDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TaskItem)))
            {
                e.Effects = DragDropEffects.Move;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void TaskDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TaskItem)))
            {
                var task = e.Data.GetData(typeof(TaskItem)) as TaskItem;
                if (task == null) return;

                var scrollViewer = sender as ScrollViewer;
                if (scrollViewer == null) return;

                string newStatus = scrollViewer.Tag as string;

                if (newStatus != null && task.Status != newStatus)
                {
                    task.Status = newStatus;
                    LoadTasks();
                }
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateTaskDialog(null);
            dialog.Owner = this;
            if (dialog.ShowDialog() == true)
            {
                var newTask = new TaskItem
                {
                    Id = currentChat.Tasks.Count > 0 ? currentChat.Tasks.Max(t => t.Id) + 1 : 1,
                    ChatId = currentChat.Id,
                    Name = dialog.TaskName,
                    Description = dialog.Description,
                    Status = dialog.SelectedColumn,
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

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            var inputWindow = new Window
            {
                Title = "Новая колонка",
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Background = (Brush)FindResource("BackgroundBrush")
            };
            inputWindow.Icon = new System.Windows.Media.Imaging.BitmapImage(
    new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute));

            var stackPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            var label = new TextBlock
            {
                Text = "Название колонки",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 10)
            };

            var textBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
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
            cancelButton.Click += (s, ev) => inputWindow.Close();

            var createButton = new Button
            {
                Content = "Создать",
                Style = (Style)FindResource("StandardButtonStyle"),
                Padding = new Thickness(15, 8, 15, 8)
            };
            createButton.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    MessageBox.Show("Введите название колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (currentChat.KanbanColumns.Any(c => c.Name == textBox.Text))
                {
                    MessageBox.Show("Колонка с таким названием уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                currentChat.KanbanColumns.Add(new KanbanColumn
                {
                    Name = textBox.Text,
                    Color = "#9E9E9E"
                });
                LoadTasks();

                inputWindow.Close();
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);

            inputWindow.Content = stackPanel;
            inputWindow.ShowDialog();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Диалог создания задачи с выбором колонки
    public class CreateTaskDialog : Window
    {
        private TextBox nameTextBox;
        private TextBox descTextBox;
        private ComboBox priorityComboBox;
        private ComboBox columnComboBox;
        private DatePicker deadlinePicker;

        public string TaskName { get; private set; }
        public string Description { get; private set; }
        public string Priority { get; private set; }
        public string SelectedColumn { get; private set; }
        public DateTime Deadline { get; private set; }

        public CreateTaskDialog(string defaultColumn)
        {
            Title = "Новая задача";
            this.Icon = new System.Windows.Media.Imaging.BitmapImage(
                                    new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute));
            Width = 450;
            Height = 450;
            MinWidth = 400;
            MinHeight = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = (Brush)FindResource("BackgroundBrush");

            var scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var stackPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

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

            stackPanel.Children.Add(new TextBlock
            {
                Text = "Описание",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            descTextBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Height = 60,
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Margin = new Thickness(0, 0, 0, 15)
            };
            stackPanel.Children.Add(descTextBox);

            // Выбор колонки
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Колонка",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            columnComboBox = new ComboBox
            {
                Style = (Style)FindResource("ComboBoxStyle"),
                Margin = new Thickness(0, 0, 0, 15)
            };

            // Заполняем список колонок
            var kanbanWindow = Owner as KanbanWindow;
            if (kanbanWindow != null)
            {
                // Получаем доступ к currentChat через поле
                // Но проще — использовать App.CurrentUser и найти чат
                // Используем статический доступ через DataInitializer
                foreach (var chat in MockData.DataInitializer.Chats)
                {
                    if (chat.TypeGroup == "Рабочая")
                    {
                        foreach (var col in chat.KanbanColumns)
                        {
                            columnComboBox.Items.Add(col.Name);
                        }
                        break;
                    }
                }
            }

            if (columnComboBox.Items.Count > 0)
            {
                // Выбираем колонку по умолчанию
                if (defaultColumn != null && columnComboBox.Items.Contains(defaultColumn))
                {
                    columnComboBox.SelectedItem = defaultColumn;
                }
                else
                {
                    columnComboBox.SelectedIndex = 0;
                }
            }
            else
            {
                // Заглушка, если не удалось получить колонки
                columnComboBox.Items.Add("Новая");
                columnComboBox.Items.Add("В работе");
                columnComboBox.Items.Add("Выполнена");
                columnComboBox.SelectedIndex = 0;
            }

            stackPanel.Children.Add(columnComboBox);

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
                BorderThickness = new Thickness(1, 1, 1, 1),
                BorderBrush = Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 20)
            };
            deadlinePicker.SelectedDate = DateTime.Now.AddDays(7);
            stackPanel.Children.Add(deadlinePicker);

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
            cancelButton.Click += (s, ev) => DialogResult = false;

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

            if (columnComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите колонку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TaskName = nameTextBox.Text;
            Description = descTextBox.Text;
            Priority = priorityComboBox.SelectedItem?.ToString() ?? "Средний";
            SelectedColumn = columnComboBox.SelectedItem.ToString();
            Deadline = deadlinePicker.SelectedDate.Value;
            DialogResult = true;
        }
    }

    // Окно настроек колонки
    public class ColumnSettingsWindow : Window
    {
        private KanbanColumn column;
        private TextBox nameTextBox;
        private ComboBox colorComboBox;

        public ColumnSettingsWindow(KanbanColumn column)
        {
            this.column = column;

            Title = "Настройки колонки";

            this.Icon = new System.Windows.Media.Imaging.BitmapImage(
                            new Uri("pack://application:,,,/Resources/logo.png", UriKind.Absolute));
            Width = 400;
            Height = 350;
            MinWidth = 350;
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = (Brush)FindResource("BackgroundBrush");

            var stackPanel = new StackPanel { Margin = new Thickness(20, 20, 20, 20) };

            // Название
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Название колонки",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            nameTextBox = new TextBox
            {
                Style = (Style)FindResource("InputTextBoxStyle"),
                Text = column.Name,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(nameTextBox);

            // Цвет
            stackPanel.Children.Add(new TextBlock
            {
                Text = "Цвет колонки",
                FontFamily = (FontFamily)FindResource("Roboto"),
                Margin = new Thickness(0, 0, 0, 5)
            });

            colorComboBox = new ComboBox
            {
                Style = (Style)FindResource("ComboBoxStyle"),
                Margin = new Thickness(0, 0, 0, 20)
            };

            // Предопределённые цвета
            var colors = new (string Name, string Hex)[]
            {
                ("Серый", "#9E9E9E"),
                ("Голубой", "#5DD4FF"),
                ("Зелёный", "#4CAF50"),
                ("Красный", "#F44336"),
                ("Оранжевый", "#FF9800"),
                ("Жёлтый", "#FFEB3B"),
                ("Фиолетовый", "#9C27B0"),
                ("Розовый", "#E91E63"),
                ("Синий", "#2196F3"),
                ("Бирюзовый", "#00BCD4"),
                ("Тёмно-синий", "#3F51B5"),
                ("Коричневый", "#795548")
            };

            int selectedIndex = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                var item = new ComboBoxItem
                {
                    Content = colors[i].Name,
                    Tag = colors[i].Hex
                };

                // Добавляем цветной кружок
                var stack = new StackPanel { Orientation = Orientation.Horizontal };
                var circle = new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = new SolidColorBrush(ParseHexColor(colors[i].Hex)),
                    Margin = new Thickness(0, 0, 10, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var text = new TextBlock
                {
                    Text = colors[i].Name,
                    VerticalAlignment = VerticalAlignment.Center
                };
                stack.Children.Add(circle);
                stack.Children.Add(text);
                item.Content = stack;

                colorComboBox.Items.Add(item);

                if (colors[i].Hex.ToUpper() == column.Color?.ToUpper())
                {
                    selectedIndex = i;
                }
            }

            colorComboBox.SelectedIndex = selectedIndex;
            stackPanel.Children.Add(colorComboBox);

            // Предпросмотр цвета
            var previewBorder = new Border
            {
                Height = 40,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(ParseHexColor(column.Color)),
                Margin = new Thickness(0, 0, 0, 20)
            };
            previewBorder.Name = "PreviewBorder";
            stackPanel.Children.Add(previewBorder);

            colorComboBox.SelectionChanged += (s, ev) =>
            {
                if (colorComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string hex)
                {
                    previewBorder.Background = new SolidColorBrush(ParseHexColor(hex));
                }
            };

            // Кнопки
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
            cancelButton.Click += (s, ev) => DialogResult = false;

            var saveButton = new Button
            {
                Content = "Сохранить",
                Style = (Style)FindResource("StandardButtonStyle"),
                Padding = new Thickness(15, 8, 15, 8)
            };
            saveButton.Click += Save_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(saveButton);
            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
        }

        private Color ParseHexColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromRgb(r, g, b);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Введите название колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, не занято ли имя (если имя изменилось)
            if (nameTextBox.Text != column.Name)
            {
                // Находим чат с этой колонкой и проверяем уникальность
                foreach (var chat in MockData.DataInitializer.Chats)
                {
                    if (chat.KanbanColumns.Any(c => c.Name == nameTextBox.Text && c != column))
                    {
                        MessageBox.Show("Колонка с таким названием уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                // Обновляем статус у всех задач этой колонки
                foreach (var task in MockData.DataInitializer.Chats.SelectMany(c => c.Tasks).Where(t => t.Status == column.Name))
                {
                    task.Status = nameTextBox.Text;
                }
            }

            // Сохраняем изменения
            column.Name = nameTextBox.Text;

            if (colorComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string hex)
            {
                column.Color = hex;
            }

            DialogResult = true;
        }
    }
}