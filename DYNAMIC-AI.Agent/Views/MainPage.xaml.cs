using CommunityToolkit.WinUI.UI.Controls;
using DYNAMIC_AI.Agent.Core.Models;
using DYNAMIC_AI.Agent.Helpers;
﻿using DYNAMIC_AI.Agent.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System.IO;
using System.Linq;

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

    private void OnImageResolving(object sender, ImageResolvingEventArgs e)
    {
        if (sender is MarkdownTextBlock textBlock && textBlock.DataContext is ChatMessage chatMessage)
        {
            if (e.Url.StartsWith("latex:"))
            {
                var indexStr = e.Url.Substring(6);
                if (int.TryParse(indexStr, out var index))
                {
                    var latexContent = chatMessage.RenderedContent
                        .OfType<LatexImageContent>()
                        .ElementAtOrDefault(index -1);

                    if (latexContent != null)
                    {
                        var image = new BitmapImage();
                        using var stream = new MemoryStream(latexContent.ImageData);
                        image.SetSource(stream.AsRandomAccessStream());
                        e.Image = image;
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
