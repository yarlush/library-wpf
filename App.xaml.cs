using System.IO;
using System.Windows;
using LibraryWpf.Services;

namespace LibraryWpf;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        if (e.Args.Length > 0 && e.Args[0] == "--screenshots")
        {
            var outputDirectory = e.Args.Length > 1
                ? e.Args[1]
                : Path.Combine(AppContext.BaseDirectory, "screenshots");

            var window = new MainWindow
            {
                Width = 1320,
                Height = 820,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = 10,
                Top = 10
            };
            window.Show();
            await ScreenshotService.CaptureAll(window, outputDirectory);
            Shutdown();
            return;
        }

        base.OnStartup(e);
    }
}
