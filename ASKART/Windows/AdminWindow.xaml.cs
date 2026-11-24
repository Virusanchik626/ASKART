using Askart.ViewModels.Administration;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace Askart.Windows;

public partial class AdminWindow : Window
{
    private readonly AdminShellViewModel viewModel;
    private Storyboard? transition;

    public AdminWindow(AdminShellViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        ThemeHelper.Watch(this);
        Loaded += Load;
        Closed += CloseSession;
        viewModel.Navigation.PropertyChanged += SectionChanged;
        SystemParameters.StaticPropertyChanged += SystemSettingsChanged;
    }

    private async void Load(object sender, RoutedEventArgs e)
    {
        Loaded -= Load;
        await viewModel.LoadSelectedAsync();
    }

    private async void SectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AdminNavigationViewModel.Section) || !IsLoaded) return;
        StopTransition();
        if (SystemParameters.ClientAreaAnimation)
        {
            transition = ((Storyboard)SectionHost.FindResource("SectionTransition")).Clone();
            transition.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }
        await viewModel.LoadSelectedAsync();
    }

    private void SystemSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation) &&
            !SystemParameters.ClientAreaAnimation)
            StopTransition();
    }

    private void StopTransition()
    {
        transition?.Remove(this);
        transition = null;
        SectionHost.Opacity = 1;
    }

    private void CloseSession(object? sender, EventArgs e)
    {
        Loaded -= Load;
        viewModel.Navigation.PropertyChanged -= SectionChanged;
        SystemParameters.StaticPropertyChanged -= SystemSettingsChanged;
        StopTransition();
        viewModel.Dispose();
    }
}
