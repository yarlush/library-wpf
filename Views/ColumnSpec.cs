namespace LibraryWpf.Views;

public enum ColumnKind
{
    Text,
    Combo,
    Date,
    Bool
}

public sealed class LookupItem
{
    public object Id { get; set; } = 0;
    public string Display { get; set; } = string.Empty;
    public override string ToString() => Display;
}

public sealed class ColumnSpec
{
    public string Header { get; init; } = string.Empty;
    public string Property { get; init; } = string.Empty;
    public ColumnKind Kind { get; init; } = ColumnKind.Text;
    public double Width { get; init; } = 120;
    public bool ReadOnly { get; init; }
    public IReadOnlyList<LookupItem>? Items { get; init; }
    public string? StringFormat { get; init; }
}
