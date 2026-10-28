using System.Windows;
using System.Windows.Controls;
using LibraryWpf.Data;
using LibraryWpf.Views;

namespace LibraryWpf;

public partial class MainWindow : Window
{
    private bool _suppressNavigation;

    public MainWindow()
    {
        InitializeComponent();
        DateStatus.Text = DateTime.Now.ToString("dd.MM.yyyy");
        Loaded += (_, _) =>
        {
            NavList.SelectedIndex = 0;
            CheckConnection();
        };
    }

    private void NavList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressNavigation)
            return;

        if (NavList.SelectedItem is ListBoxItem item && item.Tag is string key)
            ContentHost.Content = Pages.Create(key);
    }

    public void NavigateTo(string key)
    {
        ContentHost.Content = Pages.Create(key);

        _suppressNavigation = true;
        foreach (var obj in NavList.Items)
        {
            if (obj is ListBoxItem item && item.Tag is string tag && tag == key)
            {
                NavList.SelectedItem = item;
                break;
            }
        }
        _suppressNavigation = false;
    }

    private void CheckConnection()
    {
        try
        {
            using var context = new LibraryContext();
            DbStatus.Text = context.Database.CanConnect()
                ? "Подключение: LibraryDB (LocalDB) — активно"
                : "Подключение: LibraryDB (LocalDB) — нет связи";
        }
        catch
        {
            DbStatus.Text = "Подключение: LibraryDB (LocalDB) — ошибка";
        }
    }
}
