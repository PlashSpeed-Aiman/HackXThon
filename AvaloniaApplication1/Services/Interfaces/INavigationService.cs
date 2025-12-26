using AvaloniaApplication1.ViewModels.Base;

namespace AvaloniaApplication1.Services.Interfaces;

public interface INavigationService
{
    ViewModelBase? CurrentViewModel { get; }
    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}
