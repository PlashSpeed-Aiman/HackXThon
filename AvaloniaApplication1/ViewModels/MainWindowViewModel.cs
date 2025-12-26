using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public INavigationService NavigationService { get; }

    public MainWindowViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;

        // Navigate to Home page on startup
        NavigationService.NavigateTo<HomeViewModel>();
    }

    [RelayCommand]
    private void Navigate(string viewModelName)
    {
        switch (viewModelName)
        {
            case nameof(HomeViewModel):
                NavigationService.NavigateTo<HomeViewModel>();
                break;
            case nameof(SettingsViewModel):
                NavigationService.NavigateTo<SettingsViewModel>();
                break;
            case nameof(HaikuViewModel):
                NavigationService.NavigateTo<HaikuViewModel>();
                break;
        }
    }
}