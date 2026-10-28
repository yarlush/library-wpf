using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryWpf.Data;
using LibraryWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWpf.Views;

public class BookAuthorsView : UserControl
{
    private sealed class Row
    {
        public int BookId { get; set; }
        public int AuthorId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }

    private readonly LibraryContext _context = new();
    private readonly ObservableCollection<Row> _rows = new();
    private readonly DataGrid _grid = new();
    private readonly ComboBox _bookCombo = new();
    private readonly ComboBox _authorCombo = new();
    private readonly TextBlock _status = new();

    public BookAuthorsView()
    {
        Background = new SolidColorBrush(Color.FromRgb(244, 246, 249));

        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        header.Children.Add(new TextBlock
        {
            Text = "Связи книг и авторов",
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121))
        });
        header.Children.Add(new TextBlock
        {
            Text = "Управление отношением «многие-ко-многим» между книгами и авторами",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126)),
            Margin = new Thickness(0, 4, 0, 0)
        });
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
        _bookCombo.Width = 250;
        _bookCombo.Height = 34;
        _bookCombo.DisplayMemberPath = "Display";
        _bookCombo.SelectedValuePath = "Id";
        _bookCombo.Margin = new Thickness(0, 0, 8, 0);

        _authorCombo.Width = 220;
        _authorCombo.Height = 34;
        _authorCombo.DisplayMemberPath = "Display";
        _authorCombo.SelectedValuePath = "Id";
        _authorCombo.Margin = new Thickness(0, 0, 8, 0);

        var addButton = new Button
        {
            Content = "Добавить связь",
            Height = 34,
            MinWidth = 110,
            Padding = new Thickness(12, 4, 12, 4),
            Background = new SolidColorBrush(Color.FromRgb(46, 134, 193)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 0, 8, 0)
        };
        addButton.Click += (_, _) => AddLink();

        var deleteButton = new Button
        {
            Content = "Удалить связь",
            Height = 34,
            MinWidth = 110,
            Padding = new Thickness(12, 4, 12, 4),
            Background = new SolidColorBrush(Color.FromRgb(236, 240, 241)),
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 0, 8, 0)
        };
        deleteButton.Click += (_, _) => DeleteLink();

        var refreshButton = new Button
        {
            Content = "Обновить",
            Height = 34,
            MinWidth = 100,
            Padding = new Thickness(12, 4, 12, 4),
            Background = new SolidColorBrush(Color.FromRgb(236, 240, 241)),
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            BorderThickness = new Thickness(0)
        };
        refreshButton.Click += (_, _) => LoadData();

        toolbar.Children.Add(new TextBlock
        {
            Text = "Книга:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });
        toolbar.Children.Add(_bookCombo);
        toolbar.Children.Add(new TextBlock
        {
            Text = "Автор:",
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 6, 0)
        });
        toolbar.Children.Add(_authorCombo);
        toolbar.Children.Add(addButton);
        toolbar.Children.Add(deleteButton);
        toolbar.Children.Add(refreshButton);
        Grid.SetRow(toolbar, 1);
        root.Children.Add(toolbar);

        _grid.AutoGenerateColumns = false;
        _grid.CanUserAddRows = false;
        _grid.EnableRowVirtualization = false;
        _grid.EnableColumnVirtualization = false;
        _grid.IsReadOnly = true;
        _grid.SelectionMode = DataGridSelectionMode.Single;
        _grid.SelectionUnit = DataGridSelectionUnit.FullRow;
        _grid.HeadersVisibility = DataGridHeadersVisibility.Column;
        _grid.GridLinesVisibility = DataGridGridLinesVisibility.Horizontal;
        _grid.RowHeight = 32;
        _grid.ColumnHeaderHeight = 36;
        _grid.Background = Brushes.White;
        _grid.AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(248, 249, 251));
        _grid.BorderBrush = new SolidColorBrush(Color.FromRgb(214, 219, 223));
        _grid.BorderThickness = new Thickness(1);
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Код книги",
            Binding = new System.Windows.Data.Binding("BookId"),
            Width = 90
        });
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Книга",
            Binding = new System.Windows.Data.Binding("BookTitle"),
            Width = 360
        });
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Код автора",
            Binding = new System.Windows.Data.Binding("AuthorId"),
            Width = 90
        });
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Автор",
            Binding = new System.Windows.Data.Binding("AuthorName"),
            Width = 260
        });
        _grid.ItemsSource = _rows;
        Grid.SetRow(_grid, 2);
        root.Children.Add(_grid);

        _status.Margin = new Thickness(0, 8, 0, 0);
        _status.FontSize = 12;
        _status.Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126));
        Grid.SetRow(_status, 3);
        root.Children.Add(_status);

        Content = root;
        Loaded += (_, _) => LoadData();
        Unloaded += (_, _) => _context.Dispose();
    }

    private void LoadData()
    {
        try
        {
            _context.ChangeTracker.Clear();
            _bookCombo.ItemsSource = Lookups.Books(_context);
            _authorCombo.ItemsSource = Lookups.Authors(_context);

            var rows = _context.BookAuthors
                .Include(x => x.Book)
                .Include(x => x.Author)
                .OrderBy(x => x.Book.Title)
                .ThenBy(x => x.Author.LastName)
                .AsEnumerable()
                .Select(x => new Row
                {
                    BookId = x.BookId,
                    AuthorId = x.AuthorId,
                    BookTitle = x.Book.Title,
                    AuthorName = $"{x.Author.LastName} {x.Author.FirstName}"
                })
                .ToList();

            _rows.Clear();
            foreach (var row in rows)
                _rows.Add(row);

            _status.Text = $"Связей: {_rows.Count}";
        }
        catch (Exception ex)
        {
            ShowError("Не удалось загрузить связи.", ex);
        }
    }

    private void AddLink()
    {
        if (_bookCombo.SelectedValue is not int bookId || _authorCombo.SelectedValue is not int authorId)
        {
            MessageBox.Show("Выберите книгу и автора.", "Библиотека",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            if (_context.BookAuthors.Any(x => x.BookId == bookId && x.AuthorId == authorId))
            {
                MessageBox.Show("Такая связь уже существует.", "Библиотека",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            _context.BookAuthors.Add(new BookAuthor { BookId = bookId, AuthorId = authorId });
            _context.SaveChanges();
            LoadData();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось добавить связь.", ex);
        }
    }

    private void DeleteLink()
    {
        if (_grid.SelectedItem is not Row row)
        {
            MessageBox.Show("Выберите связь для удаления.", "Библиотека",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show("Удалить выбранную связь?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            var entity = _context.BookAuthors
                .FirstOrDefault(x => x.BookId == row.BookId && x.AuthorId == row.AuthorId);
            if (entity is not null)
            {
                _context.BookAuthors.Remove(entity);
                _context.SaveChanges();
            }
            LoadData();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось удалить связь.", ex);
        }
    }

    private static void ShowError(string message, Exception ex)
    {
        var inner = ex;
        while (inner.InnerException is not null)
            inner = inner.InnerException;
        MessageBox.Show($"{message}\n\n{inner.Message}", "Ошибка",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}


