using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DYNAMIC_AI.Agent.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Contracts.Services;
using DYNAMIC_AI.Agent.Core.Models;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Windows.Graphics.Capture;
using Windows.Graphics.Imaging;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;

namespace DYNAMIC_AI.Agent.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    private readonly IGeminiService _geminiService;
    private readonly ILocalSettingsService _localSettingsService;

    public ObservableCollection<ChatMessageViewModel> ChatMessages { get; } = new ObservableCollection<ChatMessageViewModel>();

    [ObservableProperty]
    private string? _userInput;

    [ObservableProperty]
    private double _temperature = 0.5;

    [ObservableProperty]
    private double _topP = 0.9;

    [ObservableProperty]
    private string? _attachmentPath;

    [ObservableProperty]
    private BitmapImage? _attachmentThumbnail;

    public MainViewModel(IGeminiService geminiService, ILocalSettingsService localSettingsService)
    {
        _geminiService = geminiService;
        _localSettingsService = localSettingsService;
    }

    [RelayCommand]
    private async Task AttachFile()
    {
        var filePicker = new FileOpenPicker();
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(filePicker, hwnd);
        filePicker.ViewMode = PickerViewMode.Thumbnail;
        filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        filePicker.FileTypeFilter.Add(".jpg");
        filePicker.FileTypeFilter.Add(".jpeg");
        filePicker.FileTypeFilter.Add(".png");

        var file = await filePicker.PickSingleFileAsync();
        if (file != null)
        {
            AttachmentPath = file.Path;
            var thumbnail = new BitmapImage();
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                await thumbnail.SetSourceAsync(stream);
            }
            AttachmentThumbnail = thumbnail;
        }
    }

    [RelayCommand]
    private async Task Screenshot()
    {
        var picker = new GraphicsCapturePicker();
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);
        GraphicsCaptureItem item = await picker.PickSingleItemAsync();

        if (item != null)
        {
            var device = Helpers.DirectXHelper.CreateDevice();
            var framePool = Direct3D11CaptureFramePool.Create(
                device,
                DirectXPixelFormat.B8G8R8A8UIntNormalized,
                2,
                item.Size);

            var session = framePool.CreateCaptureSession(item);
            var tcs = new TaskCompletionSource<SoftwareBitmap>();

            framePool.FrameArrived += async (s, a) =>
            {
                using (var frame = s.TryGetNextFrame())
                {
                    if (frame != null)
                    {
                        try
                        {
                            var softwareBitmap = await SoftwareBitmap.CreateCopyFromSurfaceAsync(frame.Surface);
                            tcs.TrySetResult(softwareBitmap);
                        }
                        catch (Exception ex)
                        {
                            tcs.TrySetException(ex);
                        }
                        finally
                        {
                            session.Dispose();
                            framePool.Dispose();
                        }
                    }
                }
            };

            session.StartCapture();
            var capturedBitmap = await tcs.Task;

            if (capturedBitmap != null)
            {
                var folder = ApplicationData.Current.TemporaryFolder;
                var file = await folder.CreateFileAsync("screenshot.png", CreationCollisionOption.GenerateUniqueName);

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                    encoder.SetSoftwareBitmap(capturedBitmap);
                    await encoder.FlushAsync();
                }

                AttachmentPath = file.Path;
                var thumbnail = new BitmapImage();
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    await thumbnail.SetSourceAsync(stream);
                }
                AttachmentThumbnail = thumbnail;
            }
        }
    }

    [RelayCommand]
    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(UserInput) && string.IsNullOrWhiteSpace(AttachmentPath))
        {
            return;
        }

        var userMessage = new ChatMessage
        {
            Content = UserInput,
            Sender = SenderType.User,
            Timestamp = System.DateTime.Now,
            AttachmentPath = AttachmentPath,
        };
        var userMessageViewModel = new ChatMessageViewModel(userMessage)
        {
            AttachmentThumbnail = AttachmentThumbnail
        };
        ChatMessages.Add(userMessageViewModel);

        var prompt = UserInput;
        var attachmentPath = AttachmentPath;

        UserInput = string.Empty;
        AttachmentPath = null;
        AttachmentThumbnail = null;

        var settings = await _localSettingsService.ReadSettingAsync<GeminiSettings>("GeminiSettings");
        if (settings == null)
        {
            // Handle case where settings are not found
            var errorMessage = new ChatMessage
            {
                Content = "Gemini settings not configured. Please go to the settings page.",
                Sender = SenderType.AI,
                Timestamp = System.DateTime.Now
            };
            ChatMessages.Add(new ChatMessageViewModel(errorMessage));
            return;
        }

        var response = await _geminiService.GetChatResponseAsync(prompt, attachmentPath, settings);

        var aiMessage = new ChatMessage
        {
            Content = response,
            Sender = SenderType.AI,
            Timestamp = System.DateTime.Now
        };
        ChatMessages.Add(new ChatMessageViewModel(aiMessage));
    }
}
