using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryWpf.Models;

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

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
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
    public string Genre { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int PublisherId { get; set; }
    public Publisher Publisher { get; set; } = null!;
    public ICollection<Copy> Copies { get; set; } = new List<Copy>();
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
