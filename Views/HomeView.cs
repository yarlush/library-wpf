using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using LibraryWpf.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryWpf.Views;

public class HomeView : UserControl
{
    private sealed class OverdueRow
    {
        public string Reader { get; set; } = string.Empty;
        public string Book { get; set; } = string.Empty;
        public string InventoryNumber { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int OverdueDays { get; set; }
    }

    public HomeView()
    {
        Background = new SolidColorBrush(Color.FromRgb(244, 246, 249));

        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
        header.Children.Add(new TextBlock
        {
            Text = "Главная панель",
            FontSize = 28,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121))
        });
        header.Children.Add(new TextBlock
        {
            Text = "Информационная система учёта библиотечного фонда и обслуживания читателей",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126)),
            Margin = new Thickness(0, 4, 0, 0)
        });
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        using var context = new LibraryContext();
        var booksCount = context.Books.Count();
        var readersCount = context.Readers.Count();
        var copiesCount = context.Copies.Count();
        var activeLoans = context.Loans.Count(x => x.ReturnDate == null);
        var overdue = context.Loans.Count(x => x.ReturnDate == null && x.DueDate < DateTime.Today);
        var reservations = context.Reservations.Count(x => x.Status == "Активно");

        var cards = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 18) };
        cards.Children.Add(MakeCard("Книг в каталоге", booksCount.ToString(), $"{copiesCount} экземпляров", Color.FromRgb(31, 78, 121)));
        cards.Children.Add(MakeCard("Читателей", readersCount.ToString(), "зарегистрировано", Color.FromRgb(46, 134, 193)));
        cards.Children.Add(MakeCard("Активных выдач", activeLoans.ToString(), "книги на руках", Color.FromRgb(39, 174, 96)));
        cards.Children.Add(MakeCard("Просрочено", overdue.ToString(), $"{reservations} активных брони", Color.FromRgb(192, 57, 43)));
        Grid.SetRow(cards, 1);
        root.Children.Add(cards);

        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        var welcome = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(22),
            Margin = new Thickness(0, 0, 12, 0),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 231, 236)),
            BorderThickness = new Thickness(1)
        };
        var welcomeStack = new StackPanel();
        welcomeStack.Children.Add(new TextBlock
        {
            Text = "О системе",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121)),
            Margin = new Thickness(0, 0, 0, 10)
        });
        welcomeStack.Children.Add(new TextBlock
        {
            Text = "Приложение предназначено для автоматизации работы библиотеки: ведения каталога книг, " +
                   "учёта экземпляров, регистрации читателей, оформления выдачи и возврата, бронирования " +
                   "и формирования отчётности.",
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            LineHeight = 22,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
        });
        welcomeStack.Children.Add(new TextBlock
        {
            Text = "Реализованные возможности:",
            FontWeight = FontWeights.SemiBold,
            FontSize = 13,
            Margin = new Thickness(0, 16, 0, 6),
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
        });
        foreach (var item in new[]
                 {
                     "просмотр, добавление, изменение и удаление записей;",
                     "поиск по таблицам и справочникам;",
                     "оформление выдачи и возврата книг;",
                     "бронирование книг и списание экземпляров;",
                     "контроль просроченных выдач и задолженностей;",
                     "управление связями книг и авторов."
                 })
        {
            welcomeStack.Children.Add(new TextBlock
            {
                Text = "•  " + item,
                FontSize = 13,
                Margin = new Thickness(0, 3, 0, 3),
                Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94))
            });
        }
        welcome.Child = welcomeStack;
        Grid.SetColumn(welcome, 0);
        content.Children.Add(welcome);

        var overduePanel = new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 231, 236)),
            BorderThickness = new Thickness(1)
        };
        var overdueGrid = new Grid();
        overdueGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        overdueGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        var overdueTitle = new TextBlock
        {
            Text = "Просроченные выдачи",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121)),
            Margin = new Thickness(0, 0, 0, 10)
        };
        overdueGrid.Children.Add(overdueTitle);

        var table = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            EnableRowVirtualization = false,
            EnableColumnVirtualization = false,
            IsReadOnly = true,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            RowHeight = 30,
            ColumnHeaderHeight = 34,
            Background = Brushes.White,
            BorderThickness = new Thickness(0),
            AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(248, 249, 251))
        };
        table.Columns.Add(new DataGridTextColumn { Header = "Читатель", Binding = new System.Windows.Data.Binding("Reader"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
        table.Columns.Add(new DataGridTextColumn { Header = "Книга", Binding = new System.Windows.Data.Binding("Book"), Width = new DataGridLength(1.4, DataGridLengthUnitType.Star) });
        table.Columns.Add(new DataGridTextColumn { Header = "Экземпляр", Binding = new System.Windows.Data.Binding("InventoryNumber"), Width = 110 });
        table.Columns.Add(new DataGridTextColumn { Header = "Срок", Binding = new System.Windows.Data.Binding("DueDate") { StringFormat = "dd.MM.yyyy" }, Width = 90 });
        table.Columns.Add(new DataGridTextColumn { Header = "Дней", Binding = new System.Windows.Data.Binding("OverdueDays"), Width = 60 });

        var overdueRows = context.Loans
            .Include(x => x.Reader)
            .Include(x => x.Copy).ThenInclude(x => x.Book)
            .Where(x => x.ReturnDate == null && x.DueDate < DateTime.Today)
            .OrderBy(x => x.DueDate)
            .Take(8)
            .AsEnumerable()
            .Select(x => new OverdueRow
            {
                Reader = $"{x.Reader.LastName} {x.Reader.FirstName}",
                Book = x.Copy.Book.Title,
                InventoryNumber = x.Copy.InventoryNumber,
                DueDate = x.DueDate,
                OverdueDays = (DateTime.Today - x.DueDate).Days
            })
            .ToList();

        table.ItemsSource = overdueRows;
        Grid.SetRow(table, 1);
        overdueGrid.Children.Add(table);
        overduePanel.Child = overdueGrid;
        Grid.SetColumn(overduePanel, 1);
        content.Children.Add(overduePanel);

        Grid.SetRow(content, 2);
        root.Children.Add(content);

        Content = root;
    }

    private static Border MakeCard(string title, string value, string subtitle, Color color)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126))
        });
        stack.Children.Add(new TextBlock
        {
            Text = value,
            FontSize = 34,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(color),
            Margin = new Thickness(0, 4, 0, 2)
        });
        stack.Children.Add(new TextBlock
        {
            Text = subtitle,
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(127, 140, 141))
        });

        return new Border
        {
            Background = Brushes.White,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18),
            Margin = new Thickness(0, 0, 12, 0),
            BorderBrush = new SolidColorBrush(Color.FromRgb(226, 231, 236)),
            BorderThickness = new Thickness(1),
            Child = stack
        };
    }
}

