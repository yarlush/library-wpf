using LibraryWpf.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWpf.Data;

public class LibraryContext : DbContext
{
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Publisher> Publishers => Set<Publisher>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();
    public DbSet<Copy> Copies => Set<Copy>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(RawDb.ConnectionString);
        optionsBuilder.EnableDetailedErrors();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookAuthor>().HasKey(x => new { x.BookId, x.AuthorId });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.Property(x => x.ISBN).HasColumnType("varchar(20)");
            entity.Property(x => x.PublicationYear).HasColumnType("smallint");
            entity.Property(x => x.Price).HasPrecision(10, 2);
            entity.HasOne(x => x.Genre).WithMany(x => x.Books)
                .HasForeignKey(x => x.GenreId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Publisher).WithMany(x => x.Books)
                .HasForeignKey(x => x.PublisherId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Author>()
            .Property(x => x.BirthYear).HasColumnType("smallint");

        modelBuilder.Entity<BookAuthor>(entity =>
        {
            entity.HasOne(x => x.Book).WithMany(x => x.BookAuthors)
                .HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Author).WithMany(x => x.BookAuthors)
                .HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Copy>(entity =>
        {
            entity.Property(x => x.AcquisitionDate).HasColumnType("date");
            entity.HasOne(x => x.Book).WithMany(x => x.Copies)
                .HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reader>()
            .Property(x => x.BirthDate).HasColumnType("date");
        modelBuilder.Entity<Reader>()
            .Property(x => x.RegistrationDate).HasColumnType("date");

        modelBuilder.Entity<Employee>()
            .Property(x => x.HireDate).HasColumnType("date");

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.Property(x => x.IssueDate).HasColumnType("date");
            entity.Property(x => x.DueDate).HasColumnType("date");
            entity.Property(x => x.ReturnDate).HasColumnType("date");
            entity.HasOne(x => x.Copy).WithMany(x => x.Loans)
                .HasForeignKey(x => x.CopyId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Reader).WithMany(x => x.Loans)
                .HasForeignKey(x => x.ReaderId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Employee).WithMany(x => x.Loans)
                .HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.Property(x => x.ReservationDate).HasColumnType("date");
            entity.HasOne(x => x.Book).WithMany(x => x.Reservations)
                .HasForeignKey(x => x.BookId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Reader).WithMany(x => x.Reservations)
                .HasForeignKey(x => x.ReaderId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
