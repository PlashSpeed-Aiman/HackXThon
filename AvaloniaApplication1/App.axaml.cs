using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;

namespace AvaloniaApplication1;

public partial class App : Application
{
    private IWindowManagerService? _windowManager;
    private IGlobalHotkeyService? _hotkeyService;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            // Initialize dependency injection
            ServiceLocator.Initialize();

            var mainWindow = new MainWindow
            {
                DataContext = ServiceLocator.GetService<MainWindowViewModel>(),
            };

            desktop.MainWindow = mainWindow;

            // Initialize window manager service with MainWindow reference
            _windowManager = ServiceLocator.GetService<IWindowManagerService>();
            _windowManager.Initialize(mainWindow);

            // Initialize and start global hotkey service
            _hotkeyService = ServiceLocator.GetService<IGlobalHotkeyService>();
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;

            try
            {
                _hotkeyService.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register global hotkey: {ex.Message}");
                // App continues to work without hotkey
            }

            // Handle app shutdown for cleanup
            desktop.ShutdownRequested += OnShutdownRequested;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnHotkeyPressed(object? sender, EventArgs e)
    {
        _windowManager?.ToggleWindow();
    }

    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        if (_hotkeyService != null)
        {
            _hotkeyService.HotkeyPressed -= OnHotkeyPressed;
            _hotkeyService.Dispose();
        }
    }

    // Tray icon menu event handlers
    private void ShowWindow_Click(object? sender, EventArgs e)
    {
        _windowManager?.ShowWindow();
    }

    private void HideWindow_Click(object? sender, EventArgs e)
    {
        _windowManager?.HideWindow();
    }

    private void Settings_Click(object? sender, EventArgs e)
    {
        _windowManager?.ShowWindow();
        var navigationService = ServiceLocator.GetService<INavigationService>();
        navigationService.NavigateTo<SettingsViewModel>();
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        _windowManager?.ExitApplication();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}