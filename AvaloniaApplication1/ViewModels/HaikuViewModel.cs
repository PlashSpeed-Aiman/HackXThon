using System.Threading.Tasks;
using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class HaikuViewModel : ViewModelBase
{
    private readonly IGeminiService _geminiService;

    [ObservableProperty]
    private string _frustrationText = string.Empty;

    [ObservableProperty]
    private string? _generatedHaiku;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private string? _errorMessage;

    public HaikuViewModel(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    private bool CanGenerate => !string.IsNullOrWhiteSpace(FrustrationText) && !IsGenerating;

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateHaikuAsync()
    {
        // Clear previous results
        GeneratedHaiku = null;
        ErrorMessage = null;
        IsGenerating = true;

        try
        {
            var response = await _geminiService.GenerateHaikuAsync(FrustrationText);

            if (response.Success)
            {
                GeneratedHaiku = response.Haiku;
            }
            else
            {
                ErrorMessage = response.ErrorMessage;
            }
        }
        finally
        {
            IsGenerating = false;
        }
    }

    partial void OnFrustrationTextChanged(string value)
    {
        // Notify that CanGenerate might have changed
        GenerateHaikuCommand.NotifyCanExecuteChanged();
    }
}
