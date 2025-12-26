# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project AvaloniaApplication1/AvaloniaApplication1.csproj

# Clean build artifacts
dotnet clean
```

## Project Overview

This is an Avalonia UI desktop application (.NET 8.0) using the MVVM pattern with the CommunityToolkit.Mvvm library. The application integrates with Google's Gemini API for AI-powered haiku generation.

**Key Dependencies:**
- Avalonia 11.3.0 (cross-platform UI framework)
- CommunityToolkit.Mvvm 8.2.1 (MVVM helpers, source generators for commands/properties)
- Google.GenAI 0.9.0 (Gemini API client)

## Architecture

### Dependency Injection Pattern

The application uses a custom `ServiceLocator` pattern (AvaloniaApplication1/Services/ServiceLocator.cs:12) for dependency injection:

- **Initialization:** `ServiceLocator.Initialize()` is called in `App.axaml.cs:29` during app startup
- **Registration:** Services and ViewModels are registered as singletons in the `Initialize()` method
- **Resolution:** Services are retrieved using `ServiceLocator.GetService<T>()`

**Service Registration Flow:**
1. Core services (NavigationService, GeminiService) are registered first
2. Page ViewModels are registered next (can depend on services)
3. MainWindowViewModel is registered last (depends on NavigationService)

### Navigation System

The application uses a service-based navigation pattern:

- **INavigationService** (AvaloniaApplication1/Services/Interfaces/INavigationService.cs) exposes `CurrentViewModel` property and `NavigateTo<TViewModel>()` method
- **NavigationService** (AvaloniaApplication1/Services/NavigationService.cs:7) implements the interface and inherits from `ObservableObject` for property change notifications
- Navigation updates `CurrentViewModel`, which is data-bound to the MainWindow's ContentControl (AvaloniaApplication1/Views/MainWindow.axaml:51-52)
- The **ViewLocator** (AvaloniaApplication1/ViewLocator.cs:9) automatically resolves Views from ViewModels by convention (e.g., `HomeViewModel` → `HomeView`)

### MVVM Structure

**ViewModels:**
- All ViewModels inherit from `ViewModelBase` (AvaloniaApplication1/ViewModels/Base/ViewModelBase.cs:5), which extends `ObservableObject` from CommunityToolkit.Mvvm
- Use `[RelayCommand]` attribute for commands (generates ICommand properties via source generators)
- ViewModels are registered as singletons in ServiceLocator and injected with dependencies

**Views:**
- Views are UserControls (or Window for MainWindow) with corresponding .axaml/.axaml.cs files
- Views are automatically resolved from ViewModels via ViewLocator convention
- Data binding uses compiled bindings (`AvaloniaUseCompiledBindingsByDefault` is enabled in .csproj:8)

**Project Structure:**
The .csproj file defines a structured folder organization:
- `Models/` - Domain, DTOs, Entities subfolders
- `Services/` - Interfaces and Implementation subfolders
- `ViewModels/` - Base subfolder for base classes
- `Views/` - View files (.axaml)
- `Converters/` - Common and Specialized subfolders
- `Controls/` - Custom and Behaviors subfolders
- `Resources/` - Styles, Themes, Brushes subfolders

### External API Integration

**Gemini Service:**
- Requires `GEMINI_API_KEY` environment variable
- Uses `gemini-2.5-flash` model
- Returns `GeminiResponse` objects with Success/Error pattern (AvaloniaApplication1/Models/GeminiResponse.cs:3)
- Handles errors gracefully with user-friendly error messages

## Important Conventions

**View-ViewModel Naming:**
- ViewModels must end with "ViewModel" suffix
- Corresponding Views must have the same name with "View" suffix
- Example: `HomeViewModel` → `HomeView`

**Adding New Pages:**
1. Create ViewModel in `ViewModels/` inheriting from `ViewModelBase`
2. Create View (.axaml/.axaml.cs) in `Views/` with matching name
3. Register ViewModel in `ServiceLocator.Initialize()`
4. Add navigation case in `MainWindowViewModel.Navigate()` method
5. Add navigation button in `MainWindow.axaml` sidebar

**Service Registration:**
When adding new services, register them in `ServiceLocator.Initialize()` before any ViewModels that depend on them.
