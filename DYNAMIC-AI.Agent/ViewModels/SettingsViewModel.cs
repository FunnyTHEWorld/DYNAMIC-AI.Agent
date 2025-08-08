using System.Reflection;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DYNAMIC_AI.Agent.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Models;
using DYNAMIC_AI.Agent.Helpers;
using Microsoft.UI.Xaml;
using Windows.ApplicationModel;

namespace DYNAMIC_AI.Agent.ViewModels;

public partial class SettingsViewModel : ObservableRecipient
{
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private ElementTheme _elementTheme;

    [ObservableProperty]
    private string _versionDescription;

    [ObservableProperty]
    private string? _geminiApiKey;

    [ObservableProperty]
    private string? _geminiBaseUrl;

    [ObservableProperty]
    private string? _geminiModel;

    public ObservableCollection<string> GeminiModels { get; } = new ObservableCollection<string> { "gemini-pro", "gemini-pro-vision" };

    public ICommand SwitchThemeCommand
    {
        get;
    }

    public SettingsViewModel(IThemeSelectorService themeSelectorService, ILocalSettingsService localSettingsService)
    {
        _themeSelectorService = themeSelectorService;
        _localSettingsService = localSettingsService;
        _elementTheme = _themeSelectorService.Theme;
        _versionDescription = GetVersionDescription();

        SwitchThemeCommand = new RelayCommand<ElementTheme>(
            async (param) =>
            {
                if (ElementTheme != param)
                {
                    ElementTheme = param;
                    await _themeSelectorService.SetThemeAsync(param);
                }
            });

        LoadGeminiSettingsAsync();
    }

    [RelayCommand]
    private async Task SaveGeminiSettings()
    {
        var settings = new GeminiSettings
        {
            ApiKey = GeminiApiKey,
            BaseUrl = GeminiBaseUrl,
            Model = GeminiModel
        };
        await _localSettingsService.SaveSettingAsync("GeminiSettings", settings);
    }

    private async void LoadGeminiSettingsAsync()
    {
        var settings = await _localSettingsService.ReadSettingAsync<GeminiSettings>("GeminiSettings");
        if (settings != null)
        {
            GeminiApiKey = settings.ApiKey;
            GeminiBaseUrl = settings.BaseUrl;
            GeminiModel = settings.Model;
        }
    }

    private static string GetVersionDescription()
    {
        Version version;

        if (RuntimeHelper.IsMSIX)
        {
            var packageVersion = Package.Current.Id.Version;

            version = new(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
        }
        else
        {
            version = Assembly.GetExecutingAssembly().GetName().Version!;
        }

        return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
