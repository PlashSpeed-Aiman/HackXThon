using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaApplication1.ViewModels;

namespace AvaloniaApplication1.Views;

public partial class HaikuView : UserControl
{
    public HaikuView()
    {
        InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is HaikuViewModel viewModel)
        {
            await viewModel.OnViewLoadedAsync();
        }
    }
}
