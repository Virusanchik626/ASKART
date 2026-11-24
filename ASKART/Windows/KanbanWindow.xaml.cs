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

        public KanbanWindow(Chat chat)
        {
            ThemeHelper.Watch(this);
            InitializeComponent();
            currentChat = chat;
            ChatNameText.Text = chat.Name;
            Loaded += async (_, _) => await App.Dialogs.RunAsync(LoadTasks);
        }


        private int loadVersion;
        private async Task LoadTasks()
        {
            int version = ++loadVersion;
            var columns = await App.Client.Kanban.GetColumns(currentChat);
            var tasks = await App.Client.Tasks.GetTasks(currentChat);
            if (version != loadVersion || !IsLoaded) return;
            ColumnsPanel.ItemsSource = columns.Select(column => new Askart.ViewModels.KanbanColumnItem(
                column, tasks.Where(task => task.Status == column.Name).ToList())).ToList();
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

        private async void TaskDrop(object sender, DragEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
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
                    await App.Client.Tasks.SetTaskStatus(task, column.Name);
                    await LoadTasks();
                }
            });
        }

        
        
        

        private async void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
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
                    await App.Client.Tasks.DeleteTask(currentChat, task);
                    await LoadTasks();
                }
            });
        }

        
        private async void QuickAddTask_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                Button button = sender as Button;
                KanbanColumn column = button.Tag as KanbanColumn;
                if (column != null)
                {
                    await OpenCreateTaskDialog(column.Name);
                }
            });
        }

        
        private async void AddTask_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                await OpenCreateTaskDialog(null);
            });
        }

        private async Task OpenCreateTaskDialog(string defaultColumn)
        {
            List<KanbanColumn> columns = await App.Client.Kanban.GetColumns(currentChat);

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
                await App.Client.Tasks.AddTask(currentChat, dialog.TaskName, dialog.Description, dialog.SelectedColumn,
                                    dialog.Deadline, dialog.Priority, App.CurrentUser);
                await LoadTasks();
            }
        }

        
        
        

        private async void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                NewColumnDialog dialog = new NewColumnDialog();
                dialog.Owner = this;
    
                if (dialog.ShowDialog() == true)
                {
                    string error = App.Client.Kanban.AddColumn(currentChat, dialog.ColumnName);
                    if (error != null)
                    {
                        MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    await LoadTasks();
                }
            });
        }

        private async void DeleteColumn_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
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
                    App.Client.Kanban.DeleteColumn(currentChat, column);
                    await LoadTasks();
                }
            });
        }

        private async void MoveColumnLeft_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                Button button = sender as Button;
                KanbanColumn column = button.Tag as KanbanColumn;
                if (column != null)
                {
                    await App.Client.Kanban.MoveColumn(currentChat, column, -1);
                    await LoadTasks();
                }
            });
        }

        private async void MoveColumnRight_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
            {
                Button button = sender as Button;
                KanbanColumn column = button.Tag as KanbanColumn;
                if (column != null)
                {
                    await App.Client.Kanban.MoveColumn(currentChat, column, 1);
                    await LoadTasks();
                }
            });
        }

        private async void ColumnSettings_Click(object sender, RoutedEventArgs e)
        {
            await App.Dialogs.RunAsync(async () =>
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
                    await LoadTasks();
                }
            });
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    
    
    
}
