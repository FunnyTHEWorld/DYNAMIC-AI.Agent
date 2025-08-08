using DYNAMIC_AI.Agent.ViewModels;

using Microsoft.UI.Xaml.Controls;

namespace DYNAMIC_AI.Agent.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }
}
