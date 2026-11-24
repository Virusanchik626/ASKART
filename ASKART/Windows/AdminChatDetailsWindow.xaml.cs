using Askart.ViewModels.Administration;
using System.Windows;

namespace Askart.Windows;

public partial class AdminChatDetailsWindow : Window
{
    private readonly AdminChatDetailsViewModel viewModel;
    public AdminChatDetailsWindow(AdminChatDetailsViewModel viewModel)
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
}
