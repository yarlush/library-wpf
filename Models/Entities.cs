using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWpf.Models;

[Table("Genres")]
public class Genre
{
    [Key]
    public int GenreId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

[Table("Publishers")]
public class Publisher
{
    [Key]
    public int PublisherId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public ICollection<Book> Books { get; set; } = new List<Book>();
}

[Table("Authors")]
public class Author
{
    [Key]
    public int AuthorId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Country { get; set; }
    public int? BirthYear { get; set; }
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
}

[Table("Books")]
public class Book
{
    [Key]
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public int Pages { get; set; }
    public decimal Price { get; set; }
    public int GenreId { get; set; }
    public int PublisherId { get; set; }
    public string? Annotation { get; set; }

    public Genre Genre { get; set; } = null!;
    public Publisher Publisher { get; set; } = null!;
    public ICollection<BookAuthor> BookAuthors { get; set; } = new List<BookAuthor>();
    public ICollection<Copy> Copies { get; set; } = new List<Copy>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

[Table("BookAuthors")]
public class BookAuthor
{
    public int BookId { get; set; }
    public int AuthorId { get; set; }
    public Book Book { get; set; } = null!;
    public Author Author { get; set; } = null!;
}

[Table("Copies")]
public class Copy
{
    [Key]
    public int CopyId { get; set; }
    public int BookId { get; set; }
    public string InventoryNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Доступен";
    public DateTime AcquisitionDate { get; set; }
    public string? Hall { get; set; }
    public string? Shelf { get; set; }

    public Book Book { get; set; } = null!;
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}

[Table("Readers")]
public class Reader
{
    [Key]
    public int ReaderId { get; set; }
    public string LibraryCardNumber { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Category { get; set; } = "Студент";
    public DateTime RegistrationDate { get; set; }
    public bool IsActive { get; set; }

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

[Table("Employees")]
public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string Position { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime HireDate { get; set; }
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}

[Table("Loans")]
public class Loan
{
    [Key]
    public int LoanId { get; set; }
    public int CopyId { get; set; }
    public int ReaderId { get; set; }
    public int EmployeeId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }

    public Copy Copy { get; set; } = null!;
    public Reader Reader { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}

[Table("Reservations")]
public class Reservation
{
    [Key]
    public int ReservationId { get; set; }
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = "Активно";

    public Book Book { get; set; } = null!;
    public Reader Reader { get; set; } = null!;
}
