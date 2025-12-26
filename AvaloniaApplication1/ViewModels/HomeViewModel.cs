using AvaloniaApplication1.ViewModels.Base;

namespace AvaloniaApplication1.ViewModels;

public class HomeViewModel : ViewModelBase
{
    public string Title => "Home";
    public string WelcomeMessage => "Welcome to Avalonia Application!";
    public string Description => "This is the home page. Use the sidebar to navigate between different pages.";
}
