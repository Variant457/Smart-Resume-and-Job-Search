using System.Security.Cryptography;
using Microsoft.Maui.Controls;
#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif
using Smart_Resume_Job_Search_App.Presentation;
using Smart_Resume_Job_Search_App.Domain;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.UI;

public partial class HomeView : ContentPage
{
    private bool _initialized;

    public HomeView()
    {
        Debug.WriteLine("[HomeView] .ctor start");
        InitializeComponent();

        // Ensure ViewModel initialization happens when the view is realized
        this.Loaded += OnLoadedOnce;
        Debug.WriteLine("[HomeView] .ctor end");
    }

    private void OnLoadedOnce(object? sender, EventArgs e)
    {
        if (_initialized)
        {
            Debug.WriteLine("[HomeView] Loaded fired again (ignored)");
            return;
        }
        _initialized = true;
        // Unsubscribe to avoid duplicate calls if the page is reloaded
        this.Loaded -= OnLoadedOnce;

        if (BindingContext is HomeViewModel vm)
        {
            vm.OnLoaded();
            Debug.WriteLine("[HomeView] Loaded -> vm.OnLoaded()");
        }
        else
        {
            Debug.WriteLine("[HomeView] Loaded: BindingContext is not HomeViewModel");
        }
    }

    HomeViewModel VM => (HomeViewModel)BindingContext;

    protected override void OnAppearing()
    {
        Debug.WriteLine("[HomeView] OnAppearing start");
        base.OnAppearing();
        if (_initialized)
        {
            Debug.WriteLine("[HomeView] OnAppearing skipped (already initialized)");
            return;
        }
        if (BindingContext is HomeViewModel vm)
        {
            Debug.WriteLine("[HomeView] OnAppearing -> vm.OnLoaded()");
            vm.OnLoaded();
            _initialized = true;
            Debug.WriteLine("[HomeView] OnAppearing end");
        }
        else
        {
            Debug.WriteLine("[HomeView] OnAppearing: BindingContext is not HomeViewModel");
        }
    }

    static FilePickerFileType ResumeFileTypes => new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        // iOS
        [DevicePlatform.iOS] = new []
        {
            "com.adobe.pdf",
            "org.openxmlformats.wordprocessingml.document",
            "com.microsoft.word.doc",
            "public.plain-text"
        },
        // Mac Catalyst
        [DevicePlatform.MacCatalyst] = new []
        {
            "com.adobe.pdf",
            "org.openxmlformats.wordprocessingml.document",
            "com.microsoft.word.doc",
            "public.plain-text"
        },
        // Android (MIME types)
        [DevicePlatform.Android] = new []
        {
            "application/pdf",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/msword",
            "text/plain"
        },
        // Windows (file extensions)
        [DevicePlatform.WinUI] = new []
        {
            ".pdf", ".docx", ".doc", ".txt"
        }
    });

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = Microsoft.Maui.Controls.DataPackageOperation.Copy;
    }

    private async void OnDrop(object sender, DropEventArgs e)
    {
#if WINDOWS
        var winArgs = e.PlatformArgs?.DragEventArgs;
        if(winArgs?.DataView.Contains(StandardDataFormats.StorageItems) == true)
        {
            var items = await winArgs.DataView.GetStorageItemsAsync();
            var file = items.OfType<StorageFile>().FirstOrDefault();
            if (file != null)
            {
                using var stream = await file.OpenStreamForReadAsync();
                var input = await ToInputBlobAsync(file.Name, MimeFromName(file.Name), stream);
                if (input != null) await VM.UploadResume(input);
                return;
            }
        }
#endif
        await Task.CompletedTask;
    }

    // Handle Button.Clicked
    private async void OnBrowseClicked(object sender, EventArgs e)
    {
        Debug.WriteLine("[HomeView] OnBrowseClicked");
        await BrowseAsync();
    }

    // Handle TapGestureRecognizer.Tapped
    private async void OnBrowseTapped(object sender, TappedEventArgs e)
    {
        Debug.WriteLine("[HomeView] OnBrowseTapped");
        await BrowseAsync();
    }

    private async Task BrowseAsync()
    {
        Debug.WriteLine("[HomeView] BrowseAsync start");
        var file = await FilePicker.Default.PickAsync(new PickOptions
        {
            FileTypes = ResumeFileTypes,
            PickerTitle = "Select your resume (PDF format)"
        });
        if (file is null) { Debug.WriteLine("[HomeView] BrowseAsync canceled"); return; }

        await using var stream = await file.OpenReadAsync();

        var mime = string.IsNullOrWhiteSpace(file.ContentType)
            ? MimeFromName(file.FileName)
            : file.ContentType;

        var input = await ToInputBlobAsync(file.FileName, mime, stream);
        if (input is not null)
        {
            Debug.WriteLine($"[HomeView] BrowseAsync -> UploadResume {file.FileName}, {mime}, {input.Content?.Length} bytes");
            await VM.UploadResume(input);
        }
        else
        {
            Debug.WriteLine("[HomeView] BrowseAsync: ToInputBlobAsync returned null");
        }
    }

    private static string MimeFromName(string fileName)
    {
        switch (Path.GetExtension(fileName).ToLowerInvariant())
        {
            case ".pdf":
                return "application/pdf";
            case ".docx":
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            case ".doc":
                return "application/msword";
            case ".txt":
                return "text/plain";
            default:
                return "application/octet-stream";
        }
    }

    private static async Task<InputBlob?> ToInputBlobAsync(string fileName, string mime, Stream stream)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        var bytes = ms.ToArray();
        if(bytes.Length == 0) return null;

        var mimeType = !string.IsNullOrEmpty(mime) ? mime : MimeFromName(fileName);
        var sha256 = ComputeSha256(bytes);

        return new InputBlob(fileName: fileName, mimeType: mimeType, content: bytes, sha256: sha256);
    }

    private static string ComputeSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToHexString(sha256.ComputeHash(data));
    }
}