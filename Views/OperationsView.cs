using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LibraryWpf.Data;
using Microsoft.Data.SqlClient;

namespace LibraryWpf.Views;

public class OperationsView : UserControl
{
    private readonly ComboBox _issueReader = new();
    private readonly ComboBox _issueCopy = new();
    private readonly ComboBox _issueEmployee = new();
    private readonly DatePicker _issueDueDate = new();
    private readonly TextBlock _issueResult = new();

    private readonly ComboBox _returnLoan = new();
    private readonly DatePicker _returnDate = new();
    private readonly TextBlock _returnResult = new();

    private readonly ComboBox _reserveReader = new();
    private readonly ComboBox _reserveBook = new();
    private readonly TextBlock _reserveResult = new();

    private readonly ComboBox _writeOffCopy = new();
    private readonly TextBlock _writeOffResult = new();
    private readonly TabControl _tabs = new();

    public OperationsView()
    {
        Background = new SolidColorBrush(Color.FromRgb(244, 246, 249));

        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        header.Children.Add(new TextBlock
        {
            Text = "Операции",
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121))
        });
        header.Children.Add(new TextBlock
        {
            Text = "Оформление выдачи, возврата, бронирования и списания экземпляров",
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126)),
            Margin = new Thickness(0, 4, 0, 0)
        });
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        _tabs.Background = Brushes.White;
        _tabs.BorderBrush = new SolidColorBrush(Color.FromRgb(214, 219, 223));
        _tabs.Padding = new Thickness(4);
        _tabs.Items.Add(BuildIssueTab());
        _tabs.Items.Add(BuildReturnTab());
        _tabs.Items.Add(BuildReserveTab());
        _tabs.Items.Add(BuildWriteOffTab());
        Grid.SetRow(_tabs, 1);
        root.Children.Add(_tabs);

        Content = root;
        Loaded += (_, _) => LoadAllLookups();
    }

    public void SelectTab(int index)
    {
        if (index >= 0 && index < _tabs.Items.Count)
            _tabs.SelectedIndex = index;
    }

    private TabItem BuildIssueTab()
    {
        var panel = CreatePanel(
            "Выдача книги",
            "Выберите читателя, свободный экземпляр и сотрудника. Срок возврата по умолчанию — 14 дней.");

        var grid = CreateFormGrid();
        AddRow(grid, 0, "Читатель:", _issueReader);
        AddRow(grid, 1, "Экземпляр:", _issueCopy);
        AddRow(grid, 2, "Сотрудник:", _issueEmployee);
        AddRow(grid, 3, "Срок возврата:", _issueDueDate);
        panel.Children.Add(grid);

        _issueReader.Width = 460;
        _issueCopy.Width = 460;
        _issueEmployee.Width = 300;
        _issueDueDate.Width = 180;
        _issueDueDate.SelectedDate = DateTime.Today.AddDays(14);

        var button = MakePrimaryButton("Выдать книгу");
        button.Margin = new Thickness(0, 18, 0, 0);
        button.Click += (_, _) => IssueBook();
        panel.Children.Add(button);

        _issueResult.Margin = new Thickness(0, 14, 0, 0);
        _issueResult.FontSize = 13;
        _issueResult.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        panel.Children.Add(_issueResult);

        return MakeTab("Выдача", panel);
    }

    private TabItem BuildReturnTab()
    {
        var panel = CreatePanel(
            "Возврат книги",
            "Выберите активную выдачу. При просрочке система рассчитает штраф из расчёта 5 руб. за день.");

        var grid = CreateFormGrid();
        AddRow(grid, 0, "Активная выдача:", _returnLoan);
        AddRow(grid, 1, "Дата возврата:", _returnDate);
        panel.Children.Add(grid);

        _returnLoan.Width = 620;
        _returnDate.Width = 180;
        _returnDate.SelectedDate = DateTime.Today;

        var button = MakePrimaryButton("Вернуть книгу");
        button.Margin = new Thickness(0, 18, 0, 0);
        button.Click += (_, _) => ReturnBook();
        panel.Children.Add(button);

        _returnResult.Margin = new Thickness(0, 14, 0, 0);
        _returnResult.FontSize = 13;
        _returnResult.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        panel.Children.Add(_returnResult);

        return MakeTab("Возврат", panel);
    }

    private TabItem BuildReserveTab()
    {
        var panel = CreatePanel(
            "Бронирование книги",
            "Читатель может забронировать книгу, если у него ещё нет активного бронирования этой книги.");

        var grid = CreateFormGrid();
        AddRow(grid, 0, "Читатель:", _reserveReader);
        AddRow(grid, 1, "Книга:", _reserveBook);
        panel.Children.Add(grid);

        _reserveReader.Width = 460;
        _reserveBook.Width = 460;

        var button = MakePrimaryButton("Забронировать");
        button.Margin = new Thickness(0, 18, 0, 0);
        button.Click += (_, _) => ReserveBook();
        panel.Children.Add(button);

        _reserveResult.Margin = new Thickness(0, 14, 0, 0);
        _reserveResult.FontSize = 13;
        _reserveResult.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        panel.Children.Add(_reserveResult);

        return MakeTab("Бронирование", panel);
    }

    private TabItem BuildWriteOffTab()
    {
        var panel = CreatePanel(
            "Списание экземпляра",
            "Списать можно только экземпляр, который не находится на руках у читателя.");

        var grid = CreateFormGrid();
        AddRow(grid, 0, "Экземпляр:", _writeOffCopy);
        panel.Children.Add(grid);

        _writeOffCopy.Width = 620;

        var button = MakePrimaryButton("Списать экземпляр");
        button.Margin = new Thickness(0, 18, 0, 0);
        button.Click += (_, _) => WriteOffCopy();
        panel.Children.Add(button);

        _writeOffResult.Margin = new Thickness(0, 14, 0, 0);
        _writeOffResult.FontSize = 13;
        _writeOffResult.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96));
        panel.Children.Add(_writeOffResult);

        return MakeTab("Списание", panel);
    }

    private static TabItem MakeTab(string header, StackPanel content)
        => new()
        {
            Header = header,
            Content = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Content = content
            }
        };

    private static StackPanel CreatePanel(string title, string description)
    {
        var panel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121))
        });
        panel.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126)),
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 20),
            MaxWidth = 720
        });
        return panel;
    }

    private static Grid CreateFormGrid()
    {
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(170) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        for (var i = 0; i < 4; i++)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        return grid;
    }

    private static void AddRow(Grid grid, int row, string label, Control control)
    {
        var text = new TextBlock
        {
            Text = label,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            Margin = new Thickness(0, 6, 12, 6)
        };
        Grid.SetRow(text, row);
        Grid.SetColumn(text, 0);
        grid.Children.Add(text);

        control.Height = 34;
        control.Margin = new Thickness(0, 6, 0, 6);
        control.HorizontalAlignment = HorizontalAlignment.Left;
        Grid.SetRow(control, row);
        Grid.SetColumn(control, 1);
        grid.Children.Add(control);
    }

    private static Button MakePrimaryButton(string text)
        => new()
        {
            Content = text,
            Height = 38,
            MinWidth = 160,
            Padding = new Thickness(18, 6, 18, 6),
            HorizontalAlignment = HorizontalAlignment.Left,
            FontSize = 13,
            Background = new SolidColorBrush(Color.FromRgb(46, 134, 193)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };

    private void LoadAllLookups()
    {
        try
        {
            using var context = new LibraryContext();
            _issueReader.ItemsSource = Lookups.ActiveReaders(context);
            _issueCopy.ItemsSource = Lookups.AvailableCopies(context);
            _issueEmployee.ItemsSource = Lookups.Employees(context);
            if (_issueReader.Items.Count > 0) _issueReader.SelectedIndex = 0;
            if (_issueCopy.Items.Count > 0) _issueCopy.SelectedIndex = 0;
            if (_issueEmployee.Items.Count > 0) _issueEmployee.SelectedIndex = 0;

            _reserveReader.ItemsSource = Lookups.ActiveReaders(context);
            _reserveBook.ItemsSource = Lookups.Books(context);
            if (_reserveReader.Items.Count > 0) _reserveReader.SelectedIndex = 0;
            if (_reserveBook.Items.Count > 0) _reserveBook.SelectedIndex = 0;

            _writeOffCopy.ItemsSource = Lookups.AvailableCopies(context);
            if (_writeOffCopy.Items.Count > 0) _writeOffCopy.SelectedIndex = 0;
            LoadActiveLoans();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось загрузить данные для операций.", ex);
        }
    }

    private void LoadActiveLoans()
    {
        var table = RawDb.Query("""
            SELECT l.LoanId,
                   r.LastName + N' ' + r.FirstName + N' — ' + b.Title +
                   N' (срок ' + CONVERT(NVARCHAR(10), l.DueDate, 104) + N')' AS Display
              FROM Loans l
              JOIN Readers r ON r.ReaderId = l.ReaderId
              JOIN Copies c ON c.CopyId = l.CopyId
              JOIN Books b ON b.BookId = c.BookId
             WHERE l.ReturnDate IS NULL
             ORDER BY l.DueDate
            """);

        var items = new List<LookupItem>();
        foreach (DataRow row in table.Rows)
        {
            items.Add(new LookupItem
            {
                Id = Convert.ToInt32(row["LoanId"]),
                Display = Convert.ToString(row["Display"]) ?? string.Empty
            });
        }
        _returnLoan.ItemsSource = items;
        if (_returnLoan.Items.Count > 0) _returnLoan.SelectedIndex = 0;
    }

    private void IssueBook()
    {
        if (_issueReader.SelectedValue is not int readerId ||
            _issueCopy.SelectedValue is not int copyId ||
            _issueEmployee.SelectedValue is not int employeeId ||
            _issueDueDate.SelectedDate is not DateTime dueDate)
        {
            MessageBox.Show("Выберите читателя, экземпляр, сотрудника и срок возврата.",
                "Библиотека", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var parameters = new[]
            {
                new SqlParameter("@CopyId", SqlDbType.Int) { Value = copyId },
                new SqlParameter("@ReaderId", SqlDbType.Int) { Value = readerId },
                new SqlParameter("@EmployeeId", SqlDbType.Int) { Value = employeeId },
                new SqlParameter("@DueDate", SqlDbType.Date) { Value = dueDate },
                new SqlParameter("@LoanId", SqlDbType.Int) { Direction = ParameterDirection.Output }
            };
            var result = RawDb.ExecuteProcedure("usp_IssueBook", parameters);
            _issueResult.Text = $"Книга выдана. Код выдачи: {result["@LoanId"]}.";
            _issueCopy.ItemsSource = Lookups.AvailableCopies(new LibraryContext());
            LoadActiveLoans();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось оформить выдачу.", ex);
        }
    }

    private void ReturnBook()
    {
        if (_returnLoan.SelectedValue is not int loanId || _returnDate.SelectedDate is not DateTime returnDate)
        {
            MessageBox.Show("Выберите активную выдачу и дату возврата.",
                "Библиотека", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var parameters = new[]
            {
                new SqlParameter("@LoanId", SqlDbType.Int) { Value = loanId },
                new SqlParameter("@ReturnDate", SqlDbType.Date) { Value = returnDate },
                new SqlParameter("@Fine", SqlDbType.Decimal)
                {
                    Direction = ParameterDirection.Output,
                    Precision = 10,
                    Scale = 2
                }
            };
            var result = RawDb.ExecuteProcedure("usp_ReturnBook", parameters);
            var fine = Convert.ToDecimal(result["@Fine"] ?? 0);
            _returnResult.Text = fine > 0
                ? $"Книга возвращена. Начислен штраф: {fine:N2} руб."
                : "Книга возвращена. Задолженность отсутствует.";
            LoadActiveLoans();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось оформить возврат.", ex);
        }
    }

    private void ReserveBook()
    {
        if (_reserveReader.SelectedValue is not int readerId || _reserveBook.SelectedValue is not int bookId)
        {
            MessageBox.Show("Выберите читателя и книгу.",
                "Библиотека", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var parameters = new[]
            {
                new SqlParameter("@BookId", SqlDbType.Int) { Value = bookId },
                new SqlParameter("@ReaderId", SqlDbType.Int) { Value = readerId },
                new SqlParameter("@ReservationId", SqlDbType.Int) { Direction = ParameterDirection.Output }
            };
            var result = RawDb.ExecuteProcedure("usp_ReserveBook", parameters);
            _reserveResult.Text = $"Книга забронирована. Код брони: {result["@ReservationId"]}.";
        }
        catch (Exception ex)
        {
            ShowError("Не удалось оформить бронирование.", ex);
        }
    }

    private void WriteOffCopy()
    {
        if (_writeOffCopy.SelectedValue is not int copyId)
        {
            MessageBox.Show("Выберите экземпляр для списания.",
                "Библиотека", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show("Списать выбранный экземпляр?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            RawDb.ExecuteProcedure("usp_WriteOffCopy",
                new SqlParameter("@CopyId", SqlDbType.Int) { Value = copyId });
            _writeOffResult.Text = "Экземпляр списан.";
            _writeOffCopy.ItemsSource = Lookups.AvailableCopies(new LibraryContext());
        }
        catch (Exception ex)
        {
            ShowError("Не удалось списать экземпляр.", ex);
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
