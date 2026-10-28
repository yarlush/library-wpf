using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LibraryWpf.Views;

namespace LibraryWpf.Services;

public static class ScreenshotService
{
    public static async Task CaptureAll(MainWindow window, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        await CapturePage(window, "home", "01_home.png", outputDirectory);
        await CapturePage(window, "books", "02_books.png", outputDirectory);
        await CapturePage(window, "readers", "03_readers.png", outputDirectory);
        await CapturePage(window, "copies", "04_copies.png", outputDirectory);
        await CapturePage(window, "loans", "05_loans.png", outputDirectory);
        await CapturePage(window, "reservations", "06_reservations.png", outputDirectory);
        await CapturePage(window, "bookauthors", "07_bookauthors.png", outputDirectory);

        window.NavigateTo("operations");
        await WaitAsync();
        if (window.FindName("ContentHost") is ContentControl host && host.Content is OperationsView operations)
        {
            operations.SelectTab(0);
            await WaitAsync();
            Capture(window, Path.Combine(outputDirectory, "08_issue.png"));

            operations.SelectTab(1);
            await WaitAsync();
            Capture(window, Path.Combine(outputDirectory, "09_return.png"));

            operations.SelectTab(2);
            await WaitAsync();
            Capture(window, Path.Combine(outputDirectory, "10_reserve.png"));

            operations.SelectTab(3);
            await WaitAsync();
            Capture(window, Path.Combine(outputDirectory, "11_writeoff.png"));
        }

        await CapturePage(window, "about", "12_about.png", outputDirectory);
    }

    private static async Task CapturePage(MainWindow window, string key, string fileName, string outputDirectory)
    {
        window.NavigateTo(key);
        await WaitAsync();
        Capture(window, Path.Combine(outputDirectory, fileName));
    }

    private static async Task WaitAsync()
    {
        await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.ContextIdle);
        await Task.Delay(120);
    }

    private static void Capture(Window window, string path)
    {
        window.UpdateLayout();
        var width = Math.Max(1, (int)Math.Ceiling(window.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(window.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }
}
