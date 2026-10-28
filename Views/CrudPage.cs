using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using LibraryWpf.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryWpf.Views;

public class CrudPage<T> : UserControl where T : class, new()
{
    private readonly LibraryContext _context = new();
    private readonly Func<LibraryContext, IQueryable<T>> _loader;
    private readonly Func<LibraryContext, T> _create;
    private readonly Action<LibraryContext, T> _add;
    private readonly Action<LibraryContext, T> _remove;
    private readonly IReadOnlyList<ColumnSpec> _columns;
    private readonly Func<T, string> _searchText;
    private readonly ObservableCollection<T> _items = new();
    private readonly DataGrid _grid;
    private readonly TextBox _searchBox;
    private readonly TextBlock _status;
    private ICollectionView? _view;

    public CrudPage(
        string title,
        string description,
        Func<LibraryContext, IQueryable<T>> loader,
        Func<LibraryContext, T> create,
        Action<LibraryContext, T> add,
        Action<LibraryContext, T> remove,
        IReadOnlyList<ColumnSpec> columns,
        Func<T, string> searchText)
    {
        _loader = loader;
        _create = create;
        _add = add;
        _remove = remove;
        _columns = columns;
        _searchText = searchText;

        Background = new SolidColorBrush(Color.FromRgb(244, 246, 249));

        var root = new Grid { Margin = new Thickness(24) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 16) };
        header.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Color.FromRgb(31, 78, 121))
        });
        header.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126)),
            Margin = new Thickness(0, 4, 0, 0)
        });
        Grid.SetRow(header, 0);
        root.Children.Add(header);

        var toolbar = new WrapPanel { Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = VerticalAlignment.Center };
        var addButton = MakeButton("Добавить", true);
        addButton.Click += (_, _) => AddItem();
        var deleteButton = MakeButton("Удалить");
        deleteButton.Click += (_, _) => DeleteItem();
        var saveButton = MakeButton("Сохранить");
        saveButton.Click += (_, _) => SaveChanges();
        var refreshButton = MakeButton("Обновить");
        refreshButton.Click += (_, _) => Reload();

        _searchBox = new TextBox
        {
            Width = 260,
            Height = 34,
            Margin = new Thickness(18, 0, 0, 0),
            Padding = new Thickness(8, 5, 8, 5),
            VerticalContentAlignment = VerticalAlignment.Center,
            ToolTip = "Поиск по таблице"
        };
        _searchBox.TextChanged += (_, _) => ApplyFilter();

        toolbar.Children.Add(addButton);
        toolbar.Children.Add(deleteButton);
        toolbar.Children.Add(saveButton);
        toolbar.Children.Add(refreshButton);
        toolbar.Children.Add(_searchBox);
        Grid.SetRow(toolbar, 1);
        root.Children.Add(toolbar);

        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            EnableRowVirtualization = false,
            EnableColumnVirtualization = false,
            CanUserDeleteRows = false,
            SelectionMode = DataGridSelectionMode.Single,
            SelectionUnit = DataGridSelectionUnit.FullRow,
            HeadersVisibility = DataGridHeadersVisibility.Column,
            GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            RowHeight = 32,
            ColumnHeaderHeight = 36,
            AlternatingRowBackground = new SolidColorBrush(Color.FromRgb(248, 249, 251)),
            Background = Brushes.White,
            BorderBrush = new SolidColorBrush(Color.FromRgb(214, 219, 223)),
            BorderThickness = new Thickness(1),
            HorizontalGridLinesBrush = new SolidColorBrush(Color.FromRgb(231, 235, 239)),
            VerticalGridLinesBrush = Brushes.Transparent
        };
        foreach (var column in _columns)
            _grid.Columns.Add(CreateColumn(column));
        _grid.ItemsSource = _items;
        _grid.MouseDoubleClick += (_, _) => _grid.BeginEdit();
        Grid.SetRow(_grid, 2);
        root.Children.Add(_grid);

        _status = new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromRgb(93, 109, 126))
        };
        Grid.SetRow(_status, 3);
        root.Children.Add(_status);

        Content = root;
        Loaded += (_, _) => Reload();
        Unloaded += (_, _) => _context.Dispose();
    }

    private static Button MakeButton(string text, bool primary = false)
    {
        var button = new Button
        {
            Content = text,
            Height = 34,
            MinWidth = 100,
            Margin = new Thickness(0, 0, 8, 0),
            Padding = new Thickness(12, 4, 12, 4),
            FontSize = 13,
            Background = primary
                ? new SolidColorBrush(Color.FromRgb(46, 134, 193))
                : new SolidColorBrush(Color.FromRgb(236, 240, 241)),
            Foreground = primary ? Brushes.White : new SolidColorBrush(Color.FromRgb(52, 73, 94)),
            BorderThickness = new Thickness(0),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        return button;
    }

    private DataGridColumn CreateColumn(ColumnSpec spec)
    {
        switch (spec.Kind)
        {
            case ColumnKind.Combo:
            {
                var combo = new FrameworkElementFactory(typeof(ComboBox));
                combo.SetValue(ComboBox.ItemsSourceProperty, spec.Items);
                combo.SetValue(ComboBox.DisplayMemberPathProperty, "Display");
                combo.SetValue(ComboBox.SelectedValuePathProperty, "Id");
                combo.SetBinding(ComboBox.SelectedValueProperty, new Binding(spec.Property)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                combo.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
                var template = new DataTemplate { VisualTree = combo };
                return new DataGridTemplateColumn
                {
                    Header = spec.Header,
                    Width = new DataGridLength(spec.Width),
                    CellTemplate = template,
                    CellEditingTemplate = template,
                    IsReadOnly = spec.ReadOnly
                };
            }
            case ColumnKind.Date:
            {
                var picker = new FrameworkElementFactory(typeof(DatePicker));
                picker.SetBinding(DatePicker.SelectedDateProperty, new Binding(spec.Property)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
                picker.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
                var template = new DataTemplate { VisualTree = picker };
                return new DataGridTemplateColumn
                {
                    Header = spec.Header,
                    Width = new DataGridLength(spec.Width),
                    CellTemplate = template,
                    CellEditingTemplate = template,
                    IsReadOnly = spec.ReadOnly
                };
            }
            case ColumnKind.Bool:
                return new DataGridCheckBoxColumn
                {
                    Header = spec.Header,
                    Width = new DataGridLength(spec.Width),
                    Binding = new Binding(spec.Property)
                    {
                        Mode = BindingMode.TwoWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                    },
                    IsReadOnly = spec.ReadOnly
                };
            default:
            {
                var binding = new Binding(spec.Property)
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
                };
                if (!string.IsNullOrWhiteSpace(spec.StringFormat))
                    binding.StringFormat = spec.StringFormat;

                return new DataGridTextColumn
                {
                    Header = spec.Header,
                    Width = new DataGridLength(spec.Width),
                    Binding = binding,
                    IsReadOnly = spec.ReadOnly
                };
            }
        }
    }

    private void Reload()
    {
        try
        {
            _context.ChangeTracker.Clear();
            var rows = _loader(_context).ToList();
            _items.Clear();
            foreach (var row in rows)
                _items.Add(row);

            _view = CollectionViewSource.GetDefaultView(_items);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось загрузить данные.", ex);
        }
    }

    private void ApplyFilter()
    {
        if (_view is null)
            return;

        var text = _searchBox.Text.Trim();
        _view.Filter = item => text.Length == 0 ||
                               (item is T typed && _searchText(typed).Contains(text, StringComparison.OrdinalIgnoreCase));
        _view.Refresh();
        UpdateStatus();
    }

    private void AddItem()
    {
        try
        {
            var item = _create(_context);
            _add(_context, item);
            _items.Add(item);
            _view?.Refresh();
            _grid.SelectedItem = item;
            _grid.ScrollIntoView(item);
            _grid.Focus();
            var editable = _grid.Columns.FirstOrDefault(c => !c.IsReadOnly) ?? _grid.Columns.FirstOrDefault();
            if (editable is not null)
            {
                _grid.CurrentCell = new DataGridCellInfo(item, editable);
                _grid.BeginEdit();
            }
            UpdateStatus();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось добавить запись.", ex);
        }
    }

    private void DeleteItem()
    {
        if (_grid.SelectedItem is not T item)
        {
            MessageBox.Show("Выберите запись для удаления.", "Библиотека",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show("Удалить выбранную запись?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            _remove(_context, item);
            _context.SaveChanges();
            _items.Remove(item);
            UpdateStatus();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось удалить запись. Возможно, на неё ссылаются другие таблицы.", ex);
        }
    }

    private void SaveChanges()
    {
        try
        {
            _grid.CommitEdit(DataGridEditingUnit.Cell, true);
            _grid.CommitEdit(DataGridEditingUnit.Row, true);
            _context.SaveChanges();
            Reload();
        }
        catch (Exception ex)
        {
            ShowError("Не удалось сохранить изменения. Проверьте заполнение обязательных полей.", ex);
        }
    }

    private void UpdateStatus()
    {
        var count = _view?.Cast<object>().Count() ?? _items.Count;
        _status.Text = $"Записей: {count}";
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

