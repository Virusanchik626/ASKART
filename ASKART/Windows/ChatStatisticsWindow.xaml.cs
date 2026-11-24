using Askart.ViewModels;
using System.Windows;

namespace Askart.Windows;

public partial class ChatStatisticsWindow : Window
{
    private readonly ChatStatisticsViewModel viewModel;
    public ChatStatisticsWindow(ChatStatisticsViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        ThemeHelper.Watch(this);
        Loaded += Load;
        Closed += (_, _) => viewModel.Dispose();
    }
    private async void Load(object sender, RoutedEventArgs e)
    {
        Loaded -= Load;
        await viewModel.LoadAsync();
    }
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
