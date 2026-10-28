using System.Windows;
using LibraryWpf.Data;
using LibraryWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWpf.Views;

public static class Pages
{
    public static UIElement Create(string key) => key switch
    {
        "books" => Books(),
        "readers" => Readers(),
        "copies" => Copies(),
        "loans" => Loans(),
        "reservations" => Reservations(),
        "authors" => Authors(),
        "publishers" => Publishers(),
        "genres" => Genres(),
        "employees" => Employees(),
        "bookauthors" => new BookAuthorsView(),
        "operations" => new OperationsView(),
        "about" => new AboutView(),
        _ => new HomeView()
    };

    private static CrudPage<Book> Books()
    {
        using var lookupContext = new LibraryContext();
        var genres = Lookups.Genres(lookupContext);
        var publishers = Lookups.Publishers(lookupContext);

        return new CrudPage<Book>(
            "Книги",
            "Библиографические описания книг, их жанры и издательства",
            context => context.Books.Include(x => x.Genre).Include(x => x.Publisher).OrderBy(x => x.Title),
            context =>
            {
                var genreId = context.Genres.OrderBy(x => x.GenreId).Select(x => x.GenreId).First();
                var publisherId = context.Publishers.OrderBy(x => x.PublisherId).Select(x => x.PublisherId).First();
                return new Book
                {
                    Title = "Новая книга",
                    ISBN = "978" + Guid.NewGuid().ToString("N")[..10],
                    PublicationYear = DateTime.Today.Year,
                    Pages = 100,
                    Price = 0,
                    GenreId = genreId,
                    PublisherId = publisherId,
                    Annotation = string.Empty
                };
            },
            (context, item) => context.Books.Add(item),
            (context, item) => context.Books.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "BookId", Width = 60, ReadOnly = true },
                new() { Header = "Название", Property = "Title", Width = 220 },
                new() { Header = "ISBN", Property = "ISBN", Width = 120 },
                new() { Header = "Год", Property = "PublicationYear", Width = 70 },
                new() { Header = "Страниц", Property = "Pages", Width = 75 },
                new() { Header = "Цена, руб.", Property = "Price", Width = 90, StringFormat = "N2" },
                new() { Header = "Жанр", Property = "GenreId", Kind = ColumnKind.Combo, Width = 130, Items = genres },
                new() { Header = "Издательство", Property = "PublisherId", Kind = ColumnKind.Combo, Width = 150, Items = publishers },
                new() { Header = "Аннотация", Property = "Annotation", Width = 220 }
            },
            book => $"{book.Title} {book.ISBN} {book.Genre?.Name} {book.Publisher?.Name}");
    }

    private static CrudPage<Reader> Readers()
    {
        var categories = Lookups.Options("Студент", "Преподаватель", "Сотрудник", "Школьник", "Пенсионер", "Другой");

        return new CrudPage<Reader>(
            "Читатели",
            "Регистрация и учёт читателей библиотеки",
            context => context.Readers.OrderBy(x => x.LastName).ThenBy(x => x.FirstName),
            context =>
            {
                var nextId = context.Readers.Any() ? context.Readers.Max(x => x.ReaderId) + 1 : 1;
                return new Reader
                {
                    LibraryCardNumber = $"Ч-{nextId:0000}",
                    LastName = "Новый",
                    FirstName = "Читатель",
                    Category = "Студент",
                    RegistrationDate = DateTime.Today,
                    IsActive = true
                };
            },
            (context, item) => context.Readers.Add(item),
            (context, item) => context.Readers.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "ReaderId", Width = 55, ReadOnly = true },
                new() { Header = "Билет", Property = "LibraryCardNumber", Width = 80 },
                new() { Header = "Фамилия", Property = "LastName", Width = 120 },
                new() { Header = "Имя", Property = "FirstName", Width = 100 },
                new() { Header = "Отчество", Property = "MiddleName", Width = 120 },
                new() { Header = "Дата рождения", Property = "BirthDate", Kind = ColumnKind.Date, Width = 110 },
                new() { Header = "Адрес", Property = "Address", Width = 180 },
                new() { Header = "Телефон", Property = "Phone", Width = 120 },
                new() { Header = "E-mail", Property = "Email", Width = 160 },
                new() { Header = "Категория", Property = "Category", Kind = ColumnKind.Combo, Width = 110, Items = categories },
                new() { Header = "Регистрация", Property = "RegistrationDate", Kind = ColumnKind.Date, Width = 110 },
                new() { Header = "Активен", Property = "IsActive", Kind = ColumnKind.Bool, Width = 65 }
            },
            reader => $"{reader.LibraryCardNumber} {reader.LastName} {reader.FirstName} {reader.MiddleName} {reader.Phone} {reader.Email}");
    }

    private static CrudPage<Copy> Copies()
    {
        using var lookupContext = new LibraryContext();
        var books = Lookups.Books(lookupContext);
        var statuses = Lookups.Options("Доступен", "Выдан", "Списан");

        return new CrudPage<Copy>(
            "Экземпляры книг",
            "Учёт физических экземпляров библиотечного фонда",
            context => context.Copies.Include(x => x.Book).OrderBy(x => x.Book.Title).ThenBy(x => x.InventoryNumber),
            context =>
            {
                var bookId = context.Books.OrderBy(x => x.BookId).Select(x => x.BookId).First();
                var nextId = context.Copies.Any() ? context.Copies.Max(x => x.CopyId) + 1 : 1;
                return new Copy
                {
                    BookId = bookId,
                    InventoryNumber = $"INV-{nextId:0000}",
                    Status = "Доступен",
                    AcquisitionDate = DateTime.Today,
                    Hall = "Абонемент",
                    Shelf = string.Empty
                };
            },
            (context, item) => context.Copies.Add(item),
            (context, item) => context.Copies.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "CopyId", Width = 55, ReadOnly = true },
                new() { Header = "Книга", Property = "BookId", Kind = ColumnKind.Combo, Width = 240, Items = books },
                new() { Header = "Инвентарный номер", Property = "InventoryNumber", Width = 140 },
                new() { Header = "Статус", Property = "Status", Kind = ColumnKind.Combo, Width = 110, Items = statuses },
                new() { Header = "Дата поступления", Property = "AcquisitionDate", Kind = ColumnKind.Date, Width = 120 },
                new() { Header = "Зал", Property = "Hall", Width = 120 },
                new() { Header = "Полка", Property = "Shelf", Width = 80 }
            },
            copy => $"{copy.InventoryNumber} {copy.Book?.Title} {copy.Status} {copy.Hall} {copy.Shelf}");
    }

    private static CrudPage<Author> Authors()
    {
        return new CrudPage<Author>(
            "Авторы",
            "Справочник авторов книг",
            context => context.Authors.OrderBy(x => x.LastName).ThenBy(x => x.FirstName),
            _ => new Author { LastName = "Новый", FirstName = "Автор" },
            (context, item) => context.Authors.Add(item),
            (context, item) => context.Authors.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "AuthorId", Width = 55, ReadOnly = true },
                new() { Header = "Фамилия", Property = "LastName", Width = 140 },
                new() { Header = "Имя", Property = "FirstName", Width = 120 },
                new() { Header = "Отчество", Property = "MiddleName", Width = 140 },
                new() { Header = "Страна", Property = "Country", Width = 130 },
                new() { Header = "Год рождения", Property = "BirthYear", Width = 100 }
            },
            author => $"{author.LastName} {author.FirstName} {author.MiddleName} {author.Country}");
    }

    private static CrudPage<Publisher> Publishers()
    {
        return new CrudPage<Publisher>(
            "Издательства",
            "Справочник издательств",
            context => context.Publishers.OrderBy(x => x.Name),
            _ => new Publisher { Name = "Новое издательство", City = "Москва" },
            (context, item) => context.Publishers.Add(item),
            (context, item) => context.Publishers.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "PublisherId", Width = 55, ReadOnly = true },
                new() { Header = "Название", Property = "Name", Width = 200 },
                new() { Header = "Город", Property = "City", Width = 130 },
                new() { Header = "Адрес", Property = "Address", Width = 220 },
                new() { Header = "Телефон", Property = "Phone", Width = 130 },
                new() { Header = "E-mail", Property = "Email", Width = 180 }
            },
            publisher => $"{publisher.Name} {publisher.City} {publisher.Address} {publisher.Phone} {publisher.Email}");
    }

    private static CrudPage<Genre> Genres()
    {
        return new CrudPage<Genre>(
            "Жанры",
            "Справочник жанров литературы",
            context => context.Genres.OrderBy(x => x.Name),
            _ => new Genre { Name = "Новый жанр" },
            (context, item) => context.Genres.Add(item),
            (context, item) => context.Genres.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "GenreId", Width = 55, ReadOnly = true },
                new() { Header = "Название", Property = "Name", Width = 180 },
                new() { Header = "Описание", Property = "Description", Width = 420 }
            },
            genre => $"{genre.Name} {genre.Description}");
    }

    private static CrudPage<Employee> Employees()
    {
        return new CrudPage<Employee>(
            "Сотрудники",
            "Сотрудники библиотеки, оформляющие выдачу книг",
            context => context.Employees.OrderBy(x => x.LastName),
            _ => new Employee
            {
                LastName = "Новый",
                FirstName = "Сотрудник",
                Position = "Библиотекарь",
                HireDate = DateTime.Today
            },
            (context, item) => context.Employees.Add(item),
            (context, item) => context.Employees.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "EmployeeId", Width = 55, ReadOnly = true },
                new() { Header = "Фамилия", Property = "LastName", Width = 140 },
                new() { Header = "Имя", Property = "FirstName", Width = 120 },
                new() { Header = "Отчество", Property = "MiddleName", Width = 140 },
                new() { Header = "Должность", Property = "Position", Width = 180 },
                new() { Header = "Телефон", Property = "Phone", Width = 140 },
                new() { Header = "Дата приёма", Property = "HireDate", Kind = ColumnKind.Date, Width = 110 }
            },
            employee => $"{employee.LastName} {employee.FirstName} {employee.Position} {employee.Phone}");
    }

    private static CrudPage<Loan> Loans()
    {
        using var lookupContext = new LibraryContext();
        var copies = Lookups.Copies(lookupContext);
        var readers = Lookups.Readers(lookupContext);
        var employees = Lookups.Employees(lookupContext);

        return new CrudPage<Loan>(
            "Выдачи книг",
            "Журнал выдачи и возврата экземпляров",
            context => context.Loans
                .Include(x => x.Copy).ThenInclude(x => x.Book)
                .Include(x => x.Reader)
                .Include(x => x.Employee)
                .OrderByDescending(x => x.IssueDate),
            context =>
            {
                var copyId = context.Copies.OrderBy(x => x.CopyId).Select(x => x.CopyId).First();
                var readerId = context.Readers.OrderBy(x => x.ReaderId).Select(x => x.ReaderId).First();
                var employeeId = context.Employees.OrderBy(x => x.EmployeeId).Select(x => x.EmployeeId).First();
                return new Loan
                {
                    CopyId = copyId,
                    ReaderId = readerId,
                    EmployeeId = employeeId,
                    IssueDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(14)
                };
            },
            (context, item) => context.Loans.Add(item),
            (context, item) => context.Loans.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "LoanId", Width = 55, ReadOnly = true },
                new() { Header = "Экземпляр", Property = "CopyId", Kind = ColumnKind.Combo, Width = 220, Items = copies },
                new() { Header = "Читатель", Property = "ReaderId", Kind = ColumnKind.Combo, Width = 220, Items = readers },
                new() { Header = "Сотрудник", Property = "EmployeeId", Kind = ColumnKind.Combo, Width = 160, Items = employees },
                new() { Header = "Дата выдачи", Property = "IssueDate", Kind = ColumnKind.Date, Width = 110 },
                new() { Header = "Срок возврата", Property = "DueDate", Kind = ColumnKind.Date, Width = 110 },
                new() { Header = "Дата возврата", Property = "ReturnDate", Kind = ColumnKind.Date, Width = 110 }
            },
            loan => $"{loan.Reader?.LastName} {loan.Copy?.InventoryNumber} {loan.Copy?.Book?.Title} {loan.IssueDate:dd.MM.yyyy}");
    }

    private static CrudPage<Reservation> Reservations()
    {
        using var lookupContext = new LibraryContext();
        var books = Lookups.Books(lookupContext);
        var readers = Lookups.Readers(lookupContext);
        var statuses = Lookups.Options("Активно", "Выполнено", "Отменено");

        return new CrudPage<Reservation>(
            "Бронирования",
            "Предварительные заявки читателей на книги",
            context => context.Reservations
                .Include(x => x.Book)
                .Include(x => x.Reader)
                .OrderByDescending(x => x.ReservationDate),
            context =>
            {
                var bookId = context.Books.OrderBy(x => x.BookId).Select(x => x.BookId).First();
                var readerId = context.Readers.OrderBy(x => x.ReaderId).Select(x => x.ReaderId).First();
                return new Reservation
                {
                    BookId = bookId,
                    ReaderId = readerId,
                    ReservationDate = DateTime.Today,
                    Status = "Активно"
                };
            },
            (context, item) => context.Reservations.Add(item),
            (context, item) => context.Reservations.Remove(item),
            new List<ColumnSpec>
            {
                new() { Header = "Код", Property = "ReservationId", Width = 55, ReadOnly = true },
                new() { Header = "Книга", Property = "BookId", Kind = ColumnKind.Combo, Width = 260, Items = books },
                new() { Header = "Читатель", Property = "ReaderId", Kind = ColumnKind.Combo, Width = 260, Items = readers },
                new() { Header = "Дата бронирования", Property = "ReservationDate", Kind = ColumnKind.Date, Width = 130 },
                new() { Header = "Статус", Property = "Status", Kind = ColumnKind.Combo, Width = 110, Items = statuses }
            },
            reservation => $"{reservation.Book?.Title} {reservation.Reader?.LastName} {reservation.Status}");
    }
}
