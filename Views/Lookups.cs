using LibraryWpf.Data;
using Microsoft.EntityFrameworkCore;

namespace LibraryWpf.Views;

public static class Lookups
{
    public static List<LookupItem> Options(params string[] values)
        => values.Select(v => new LookupItem { Id = v, Display = v }).ToList();

    public static List<LookupItem> Genres(LibraryContext context)
        => context.Genres.AsNoTracking().OrderBy(x => x.Name)
            .AsEnumerable()
            .Select(x => new LookupItem { Id = x.GenreId, Display = x.Name })
            .ToList();

    public static List<LookupItem> Publishers(LibraryContext context)
        => context.Publishers.AsNoTracking().OrderBy(x => x.Name)
            .AsEnumerable()
            .Select(x => new LookupItem { Id = x.PublisherId, Display = x.Name })
            .ToList();

    public static List<LookupItem> Authors(LibraryContext context)
        => context.Authors.AsNoTracking().OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .AsEnumerable()
            .Select(x => new LookupItem
            {
                Id = x.AuthorId,
                Display = $"{x.LastName} {x.FirstName} {x.MiddleName}".Trim()
            })
            .ToList();

    public static List<LookupItem> Books(LibraryContext context)
        => context.Books.AsNoTracking().OrderBy(x => x.Title)
            .AsEnumerable()
            .Select(x => new LookupItem { Id = x.BookId, Display = x.Title })
            .ToList();

    public static List<LookupItem> Readers(LibraryContext context)
        => context.Readers.AsNoTracking().OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .AsEnumerable()
            .Select(x => new LookupItem
            {
                Id = x.ReaderId,
                Display = $"{x.LibraryCardNumber} — {x.LastName} {x.FirstName}"
            })
            .ToList();

    public static List<LookupItem> Employees(LibraryContext context)
        => context.Employees.AsNoTracking().OrderBy(x => x.LastName)
            .AsEnumerable()
            .Select(x => new LookupItem
            {
                Id = x.EmployeeId,
                Display = $"{x.LastName} {x.FirstName}"
            })
            .ToList();

    public static List<LookupItem> Copies(LibraryContext context)
        => context.Copies.AsNoTracking().Include(x => x.Book)
            .OrderBy(x => x.Book.Title).ThenBy(x => x.InventoryNumber)
            .AsEnumerable()
            .Select(x => new LookupItem
            {
                Id = x.CopyId,
                Display = $"{x.InventoryNumber} — {x.Book.Title} ({x.Status})"
            })
            .ToList();

    public static List<LookupItem> AvailableCopies(LibraryContext context)
        => context.Copies.AsNoTracking().Include(x => x.Book)
            .Where(x => x.Status == "Доступен")
            .OrderBy(x => x.Book.Title).ThenBy(x => x.InventoryNumber)
            .AsEnumerable()
            .Select(x => new LookupItem
            {
                Id = x.CopyId,
                Display = $"{x.InventoryNumber} — {x.Book.Title}"
            })
            .ToList();

    public static List<LookupItem> ActiveReaders(LibraryContext context)
        => context.Readers.AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.LastName).ThenBy(x => x.FirstName)
            .AsEnumerable()
            .Select(x => new LookupItem
            {
                Id = x.ReaderId,
                Display = $"{x.LibraryCardNumber} — {x.LastName} {x.FirstName}"
            })
            .ToList();
}
