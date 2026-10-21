using Askart.Models;
using Askart.Services;
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
        private const double ColumnWidth = 300;

        public KanbanWindow(Chat chat)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            currentChat = chat;
            ChatNameText.Text = chat.Name;
            LoadTasks();
        }


        private void LoadTasks()
        {
            ColumnsPanel.Children.Clear();

            List<KanbanColumn> columns = DataService.GetColumns(currentChat);
            List<TaskItem> allTasks = DataService.GetTasks(currentChat);

            foreach (KanbanColumn column in columns)
            {
                
                List<TaskItem> tasksInColumn = new List<TaskItem>();
                foreach (TaskItem task in allTasks)
                {
                    if (task.Status == column.Name)
                    {
                        tasksInColumn.Add(task);
                    }
                }

                Border columnBorder = CreateColumn(column, tasksInColumn);
                ColumnsPanel.Children.Add(columnBorder);
            }
        }

        private Border CreateColumn(KanbanColumn column, List<TaskItem> tasks)
        {
            Border border = new Border();
            border.SetResourceReference(Border.BackgroundProperty, "SecondaryBackgroundBrush");
            border.CornerRadius = new CornerRadius(12);
            border.Width = ColumnWidth;
            border.Margin = new Thickness(0, 0, 10, 0);
            border.VerticalAlignment = VerticalAlignment.Stretch;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            
            Border header = new Border();
            header.Padding = new Thickness(15);
            header.Background = UiHelper.HexToBrush(column.Color);
            header.CornerRadius = new CornerRadius(12, 12, 0, 0);

            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock headerText = new TextBlock();
            headerText.Text = column.Name;
            headerText.FontFamily = UiHelper.GetFont();
            headerText.FontSize = 16;
            headerText.FontWeight = FontWeights.Bold;
            headerText.Foreground = Brushes.White;
            headerText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(headerText, 0);

            Button quickAddButton = CreateHeaderButton("+", 16, column, 1, QuickAddTask_Click);
            Button settingsButton = CreateHeaderButton("⚙", 14, column, 2, ColumnSettings_Click);
            Button moveLeftButton = CreateHeaderButton("←", 16, column, 3, MoveColumnLeft_Click);
            Button moveRightButton = CreateHeaderButton("→", 16, column, 4, MoveColumnRight_Click);
            moveRightButton.Margin = new Thickness(0);

            headerGrid.Children.Add(headerText);
            headerGrid.Children.Add(quickAddButton);
            headerGrid.Children.Add(settingsButton);
            headerGrid.Children.Add(moveLeftButton);
            headerGrid.Children.Add(moveRightButton);
            header.Child = headerGrid;
            Grid.SetRow(header, 0);

            
            Border deleteBorder = new Border();
            deleteBorder.SetResourceReference(Border.BackgroundProperty, "SecondaryBackgroundBrush");
            deleteBorder.SetResourceReference(Border.BorderBrushProperty, "LineBrush");
            deleteBorder.Padding = new Thickness(15, 10, 15, 10);
            deleteBorder.BorderThickness = new Thickness(0, 0, 0, 1);

            Button deleteButton = new Button();
            deleteButton.Content = "✕ Удалить колонку";
            deleteButton.Background = Brushes.Transparent;
            deleteButton.SetResourceReference(Control.ForegroundProperty, "ErrorBrush");
            deleteButton.BorderThickness = new Thickness(0);
            deleteButton.FontSize = 12;
            deleteButton.Cursor = Cursors.Hand;
            deleteButton.Tag = column;
            deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
            deleteButton.Click += DeleteColumn_Click;
            deleteBorder.Child = deleteButton;
            Grid.SetRow(deleteBorder, 1);

            
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.AllowDrop = true;
            scrollViewer.Tag = column; 
            scrollViewer.Drop += TaskDrop;
            scrollViewer.DragOver += TaskDragOver;

            StackPanel tasksPanel = new StackPanel();
            tasksPanel.Margin = new Thickness(10);
            scrollViewer.Content = tasksPanel;
            Grid.SetRow(scrollViewer, 2);

            foreach (TaskItem task in tasks)
            {
                tasksPanel.Children.Add(CreateTaskCard(task));
            }

            grid.Children.Add(header);
            grid.Children.Add(deleteBorder);
            grid.Children.Add(scrollViewer);
            border.Child = grid;

            return border;
        }

        
        private Button CreateHeaderButton(string text, double fontSize, KanbanColumn column, int gridColumn, RoutedEventHandler handler)
        {
            Button button = new Button();
            button.Content = text;
            button.Background = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
            button.Foreground = Brushes.White;
            button.BorderThickness = new Thickness(0);
            button.FontSize = fontSize;
            button.FontWeight = FontWeights.Bold;
            button.Cursor = Cursors.Hand;
            button.Tag = column;
            button.Width = 28;
            button.Height = 28;
            button.Margin = new Thickness(0, 0, 5, 0);
            button.Click += handler;
            Grid.SetColumn(button, gridColumn);
            return button;
        }

        private Border CreateTaskCard(TaskItem task)
        {
            Border border = new Border();
            border.SetResourceReference(Border.BackgroundProperty, "CardBackgroundBrush");
            border.CornerRadius = new CornerRadius(8);
            border.Padding = new Thickness(12);
            border.Margin = new Thickness(0, 0, 0, 10);
            border.Cursor = Cursors.Hand;

            DropShadowEffect shadow = new DropShadowEffect();
            shadow.Color = Colors.Black;
            shadow.BlurRadius = 10;
            shadow.ShadowDepth = 2;
            shadow.Opacity = 0.1;
            border.Effect = shadow;

            StackPanel stackPanel = new StackPanel();

            
            Grid headerGrid = new Grid();
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock nameText = UiHelper.MakeText(task.Name, 14, "PrimaryTextBrush");
            nameText.FontWeight = FontWeights.SemiBold;
            nameText.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(nameText, 0);

            Button deleteButton = UiHelper.MakeIconButton("✕", 16, "Удалить задачу");
            deleteButton.SetResourceReference(Control.ForegroundProperty, "ErrorBrush");
            deleteButton.Padding = new Thickness(5, 0, 5, 0);
            deleteButton.Tag = task;
            deleteButton.Click += DeleteTask_Click;
            Grid.SetColumn(deleteButton, 1);

            headerGrid.Children.Add(nameText);
            headerGrid.Children.Add(deleteButton);
            stackPanel.Children.Add(headerGrid);

            
            if (string.IsNullOrWhiteSpace(task.Description) == false)
            {
                TextBlock descText = UiHelper.MakeText(task.Description, 12, "SecondaryTextBrush");
                descText.TextWrapping = TextWrapping.Wrap;
                descText.Margin = new Thickness(0, 5, 0, 8);
                stackPanel.Children.Add(descText);
            }

            
            TextBlock priorityText = UiHelper.MakeText("Приоритет: " + task.Priority, 11, "SuccessBrush");
            if (task.Priority == "Высокий")
            {
                priorityText.SetResourceReference(TextBlock.ForegroundProperty, "ErrorBrush");
            }
            else if (task.Priority == "Средний")
            {
                priorityText.Foreground = Brushes.Orange;
            }
            priorityText.Margin = new Thickness(0, 0, 0, 3);

            TextBlock assigneeText = UiHelper.MakeText("Исполнитель: " + task.AssigneeName, 11, "SecondaryTextBrush");
            assigneeText.Margin = new Thickness(0, 0, 0, 8);

            TextBlock deadlineText = UiHelper.MakeText("Срок: " + task.Deadline.ToString("dd.MM.yyyy"), 11, "InactiveTextBrush");
            deadlineText.Margin = new Thickness(0, 0, 0, 10);

            stackPanel.Children.Add(priorityText);
            stackPanel.Children.Add(assigneeText);
            stackPanel.Children.Add(deadlineText);

            border.Child = stackPanel;
            border.Tag = task;
            border.MouseLeftButtonDown += TaskCard_MouseDown;

            return border;
        }

        
        
        

        private void TaskCard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            TaskItem task = border.Tag as TaskItem;
            if (task != null)
            {
                DragDrop.DoDragDrop(border, task, DragDropEffects.Move);
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
            if (e.Data.GetDataPresent(typeof(TaskItem)) == false)
            {
                return;
            }

            TaskItem task = e.Data.GetData(typeof(TaskItem)) as TaskItem;
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (task == null || scrollViewer == null)
            {
                return;
            }

            KanbanColumn column = scrollViewer.Tag as KanbanColumn;
            if (column != null && task.Status != column.Name)
            {
                DataService.SetTaskStatus(task, column.Name);
                LoadTasks();
            }
        }

        
        
        

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TaskItem task = button.Tag as TaskItem;
            if (task == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show("Удалить задачу \"" + task.Name + "\"?", "Удаление задачи",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DataService.DeleteTask(currentChat, task);
                LoadTasks();
            }
        }

        
        private void QuickAddTask_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            KanbanColumn column = button.Tag as KanbanColumn;
            if (column != null)
            {
                OpenCreateTaskDialog(column.Name);
            }
        }

        
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            OpenCreateTaskDialog(null);
        }

        private void OpenCreateTaskDialog(string defaultColumn)
        {
            List<KanbanColumn> columns = DataService.GetColumns(currentChat);

            if (columns.Count == 0)
            {
                MessageBox.Show("Сначала создайте хотя бы одну колонку", "Нет колонок",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CreateTaskDialog dialog = new CreateTaskDialog(columns, defaultColumn);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                DataService.AddTask(currentChat, dialog.TaskName, dialog.Description, dialog.SelectedColumn,
                                    dialog.Deadline, dialog.Priority, App.CurrentUser);
                LoadTasks();
            }
        }

        
        
        

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            NewColumnDialog dialog = new NewColumnDialog();
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                string error = DataService.AddColumn(currentChat, dialog.ColumnName);
                if (error != null)
                {
                    MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                LoadTasks();
            }
        }

        private void DeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            KanbanColumn column = button.Tag as KanbanColumn;
            if (column == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить колонку \"" + column.Name + "\" и все её задачи?", "Удаление колонки",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DataService.DeleteColumn(currentChat, column);
                LoadTasks();
            }
        }

        private void MoveColumnLeft_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            KanbanColumn column = button.Tag as KanbanColumn;
            if (column != null)
            {
                DataService.MoveColumn(currentChat, column, -1);
                LoadTasks();
            }
        }

        private void MoveColumnRight_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            KanbanColumn column = button.Tag as KanbanColumn;
            if (column != null)
            {
                DataService.MoveColumn(currentChat, column, 1);
                LoadTasks();
            }
        }

        private void ColumnSettings_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            KanbanColumn column = button.Tag as KanbanColumn;
            if (column == null)
            {
                return;
            }

            ColumnSettingsWindow settingsWindow = new ColumnSettingsWindow(currentChat, column);
            settingsWindow.Owner = this;

            if (settingsWindow.ShowDialog() == true)
            {
                LoadTasks();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    
    
    
    public class NewColumnDialog : Window
    {
        private TextBox textBox;

        public string ColumnName { get; private set; }

        public NewColumnDialog()
        {
            ThemeHelper.Watch(this);

            Title = "Новая колонка";
            Width = 350;
            Height = 200;
            Icon = UiHelper.GetLogo();
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");

            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(20);

            TextBlock label = UiHelper.MakeText("Название колонки", 13, "PrimaryTextBrush");
            label.Margin = new Thickness(0, 0, 0, 10);

            textBox = new TextBox();
            textBox.Style = (Style)FindResource("InputTextBoxStyle");
            textBox.Margin = new Thickness(0, 0, 0, 20);

            StackPanel buttonPanel = new StackPanel();
            buttonPanel.Orientation = Orientation.Horizontal;
            buttonPanel.HorizontalAlignment = HorizontalAlignment.Right;

            Button cancelButton = UiHelper.MakeCancelButton();
            cancelButton.Click += Cancel_Click;

            Button createButton = UiHelper.MakeMainButton("Создать");
            createButton.Click += Create_Click;

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(createButton);

            stackPanel.Children.Add(label);
            stackPanel.Children.Add(textBox);
            stackPanel.Children.Add(buttonPanel);

            Content = stackPanel;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                MessageBox.Show("Введите название колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ColumnName = textBox.Text.Trim();
            DialogResult = true;
        }
    }

    
    
    
    
    
    
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

        public CreateTaskDialog(List<KanbanColumn> columns, string defaultColumn)
        {
            ThemeHelper.Watch(this);

            Title = "Новая задача";
            Icon = UiHelper.GetLogo();
            Width = 450;
            Height = 450;
            MinWidth = 400;
            MinHeight = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");

            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(20);

            
            TextBlock nameLabel = UiHelper.MakeText("Название задачи", 13, "PrimaryTextBrush");
            nameLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(nameLabel);

            nameTextBox = new TextBox();
            nameTextBox.Style = (Style)FindResource("InputTextBoxStyle");
            nameTextBox.Margin = new Thickness(0, 0, 0, 15);
            stackPanel.Children.Add(nameTextBox);

            
            TextBlock descLabel = UiHelper.MakeText("Описание", 13, "PrimaryTextBrush");
            descLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(descLabel);

            descTextBox = new TextBox();
            descTextBox.Style = (Style)FindResource("InputTextBoxStyle");
            descTextBox.Height = 60;
            descTextBox.TextWrapping = TextWrapping.Wrap;
            descTextBox.AcceptsReturn = true;
            descTextBox.Margin = new Thickness(0, 0, 0, 15);
            stackPanel.Children.Add(descTextBox);

            
            TextBlock columnLabel = UiHelper.MakeText("Колонка", 13, "PrimaryTextBrush");
            columnLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(columnLabel);

            columnComboBox = new ComboBox();
            columnComboBox.Style = (Style)FindResource("ComboBoxStyle");
            columnComboBox.Margin = new Thickness(0, 0, 0, 15);

            foreach (KanbanColumn column in columns)
            {
                columnComboBox.Items.Add(column.Name);
            }

            
            columnComboBox.SelectedIndex = 0;
            if (defaultColumn != null)
            {
                int index = columnComboBox.Items.IndexOf(defaultColumn);
                if (index >= 0)
                {
                    columnComboBox.SelectedIndex = index;
                }
            }
            stackPanel.Children.Add(columnComboBox);

            
            TextBlock priorityLabel = UiHelper.MakeText("Приоритет", 13, "PrimaryTextBrush");
            priorityLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(priorityLabel);

            priorityComboBox = new ComboBox();
            priorityComboBox.Style = (Style)FindResource("ComboBoxStyle");
            priorityComboBox.Margin = new Thickness(0, 0, 0, 15);
            priorityComboBox.Items.Add("Низкий");
            priorityComboBox.Items.Add("Средний");
            priorityComboBox.Items.Add("Высокий");
            priorityComboBox.SelectedIndex = 1;
            stackPanel.Children.Add(priorityComboBox);

            
            TextBlock deadlineLabel = UiHelper.MakeText("Срок выполнения", 13, "PrimaryTextBrush");
            deadlineLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(deadlineLabel);

            deadlinePicker = new DatePicker();
            deadlinePicker.FontFamily = UiHelper.GetFont();
            deadlinePicker.Padding = new Thickness(15, 12, 15, 12);
            deadlinePicker.Margin = new Thickness(0, 0, 0, 20);
            deadlinePicker.SelectedDate = DateTime.Now.AddDays(7);
            stackPanel.Children.Add(deadlinePicker);

            
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

            scrollViewer.Content = stackPanel;
            Content = scrollViewer;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
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

            TaskName = nameTextBox.Text.Trim();
            Description = descTextBox.Text;
            Priority = "Средний";
            if (priorityComboBox.SelectedItem != null)
            {
                Priority = priorityComboBox.SelectedItem.ToString();
            }
            SelectedColumn = columnComboBox.SelectedItem.ToString();
            Deadline = deadlinePicker.SelectedDate.Value;
            DialogResult = true;
        }
    }

    
    
    
    public class ColumnSettingsWindow : Window
    {
        private Chat chat;
        private KanbanColumn column;
        private TextBox nameTextBox;
        private ComboBox colorComboBox;
        private Border previewBorder;

        
        private string[] colorNames = { "Серый", "Голубой", "Зелёный", "Красный", "Оранжевый", "Жёлтый",
                                        "Фиолетовый", "Розовый", "Синий", "Бирюзовый", "Тёмно-синий", "Коричневый" };
        private string[] colorCodes = { "#9E9E9E", "#5DD4FF", "#4CAF50", "#F44336", "#FF9800", "#FFEB3B",
                                        "#9C27B0", "#E91E63", "#2196F3", "#00BCD4", "#3F51B5", "#795548" };

        public ColumnSettingsWindow(Chat chat, KanbanColumn column)
        {
            ThemeHelper.Watch(this);

            this.chat = chat;
            this.column = column;

            Title = "Настройки колонки";
            Icon = UiHelper.GetLogo();
            Width = 400;
            Height = 350;
            MinWidth = 350;
            MinHeight = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            SetResourceReference(Control.BackgroundProperty, "BackgroundBrush");

            StackPanel stackPanel = new StackPanel();
            stackPanel.Margin = new Thickness(20);

            
            TextBlock nameLabel = UiHelper.MakeText("Название колонки", 13, "PrimaryTextBrush");
            nameLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(nameLabel);

            nameTextBox = new TextBox();
            nameTextBox.Style = (Style)FindResource("InputTextBoxStyle");
            nameTextBox.Text = column.Name;
            nameTextBox.Margin = new Thickness(0, 0, 0, 20);
            stackPanel.Children.Add(nameTextBox);

            
            TextBlock colorLabel = UiHelper.MakeText("Цвет колонки", 13, "PrimaryTextBrush");
            colorLabel.Margin = new Thickness(0, 0, 0, 5);
            stackPanel.Children.Add(colorLabel);

            colorComboBox = new ComboBox();
            colorComboBox.Style = (Style)FindResource("ComboBoxStyle");
            colorComboBox.Margin = new Thickness(0, 0, 0, 20);

            int selectedIndex = 0;
            for (int i = 0; i < colorNames.Length; i++)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Tag = colorCodes[i];

                
                StackPanel itemPanel = new StackPanel();
                itemPanel.Orientation = Orientation.Horizontal;

                Ellipse circle = new Ellipse();
                circle.Width = 16;
                circle.Height = 16;
                circle.Fill = UiHelper.HexToBrush(colorCodes[i]);
                circle.Margin = new Thickness(0, 0, 10, 0);
                circle.VerticalAlignment = VerticalAlignment.Center;

                TextBlock itemText = UiHelper.MakeText(colorNames[i], 13, "PrimaryTextBrush");
                itemText.VerticalAlignment = VerticalAlignment.Center;

                itemPanel.Children.Add(circle);
                itemPanel.Children.Add(itemText);
                item.Content = itemPanel;

                colorComboBox.Items.Add(item);

                if (column.Color != null && colorCodes[i].ToUpper() == column.Color.ToUpper())
                {
                    selectedIndex = i;
                }
            }
            stackPanel.Children.Add(colorComboBox);

            
            previewBorder = new Border();
            previewBorder.Height = 40;
            previewBorder.CornerRadius = new CornerRadius(8);
            previewBorder.Background = UiHelper.HexToBrush(column.Color);
            previewBorder.Margin = new Thickness(0, 0, 0, 20);
            stackPanel.Children.Add(previewBorder);

            colorComboBox.SelectionChanged += ColorComboBox_SelectionChanged;
            colorComboBox.SelectedIndex = selectedIndex;

            
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

        
        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem selectedItem = colorComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                string hex = selectedItem.Tag as string;
                previewBorder.Background = UiHelper.HexToBrush(hex);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string newName = nameTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Введите название колонки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string newColor = column.Color;
            ComboBoxItem selectedItem = colorComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                newColor = selectedItem.Tag as string;
            }

            
            string error = DataService.UpdateColumn(chat, column, newName, newColor);
            if (error != null)
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
        }
    }
}
