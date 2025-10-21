using System.Security.Cryptography;
using Microsoft.Maui.Controls;
#if WINDOWS
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
#endif
using Smart_Resume_Job_Search_App.Presentation;
using Smart_Resume_Job_Search_App.Domain;

namespace Smart_Resume_Job_Search_App.UI;

public partial class HomeView : ContentPage
{
	public HomeView()
	{
		InitializeComponent();
		Appearing += (_, _) => VM.OnLoaded();
    }

	HomeViewModel VM => (HomeViewModel)BindingContext;

    static FilePickerFileType ResumeFileTypes => new(new Dictionary<DevicePlatform, IEnumerable<string>>
    {
        // iOS
        [DevicePlatform.iOS] = new[]
    {
        "com.adobe.pdf",                                // PDF
        "org.openxmlformats.wordprocessingml.document", // DOCX
        "com.microsoft.word.doc",                       // DOC
        "public.plain-text"                             // TXT
    },
        // Mac Catalyst
        [DevicePlatform.MacCatalyst] = new[]
    {
        "com.adobe.pdf",
        "org.openxmlformats.wordprocessingml.document",
        "com.microsoft.word.doc",
        "public.plain-text"
    },
        // Android (MIME types)
        [DevicePlatform.Android] = new[]
    {
        "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/msword",
        "text/plain"
    },
        // Windows (file extensions)
        [DevicePlatform.WinUI] = new[]
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

    private async void OnBrowseClicked(object sender, EventArgs e)
	{
		var file = await FilePicker.Default.PickAsync(new PickOptions
		{
			FileTypes = ResumeFileTypes,
			PickerTitle = "Select your resume (PDF format)"
		});
		if (file is null) return;

		await using var stream = await file.OpenReadAsync();

        var mime = string.IsNullOrWhiteSpace(file.ContentType)
            ? MimeFromName(file.FileName)
            : file.ContentType;

        var input = await ToInputBlobAsync(file.FileName, mime, stream);
        if (input is not null) await VM.UploadResume(input);
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