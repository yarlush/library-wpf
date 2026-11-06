/* ============================================================
   Курсовая работа. Предметная область «Библиотека»
   Запросы к базе данных LibraryDB
   Упрощённая модель: 6 таблиц
   ============================================================ */
USE LibraryDB;
GO
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

/* ------------------------------------------------------------
   Запрос 1. Книги заданного автора
   ------------------------------------------------------------ */
DECLARE @Author NVARCHAR(150) = N'Толстой';

SELECT b.BookId,
       b.Title,
       b.Author,
       b.PublicationYear,
       b.Genre,
       p.Name AS Publisher
  FROM Books b
  JOIN Publishers p ON p.PublisherId = b.PublisherId
 WHERE b.Author LIKE N'%' + @Author + N'%'
 ORDER BY b.Title;
GO

/* ------------------------------------------------------------
   Запрос 2. Книги выбранного жанра с количеством экземпляров
   ------------------------------------------------------------ */
DECLARE @Genre NVARCHAR(60) = N'Фантастика';

SELECT b.Title,
       b.Author,
       b.PublicationYear,
       p.Name AS Publisher,
       COUNT(c.CopyId) AS CopiesCount
  FROM Books b
  JOIN Publishers p ON p.PublisherId = b.PublisherId
  LEFT JOIN Copies c ON c.BookId = b.BookId
 WHERE b.Genre = @Genre
 GROUP BY b.BookId, b.Title, b.Author, b.PublicationYear, p.Name
 ORDER BY b.Title;
GO

/* ------------------------------------------------------------
   Запрос 3. Активные выдачи книг
   ------------------------------------------------------------ */
SELECT l.LoanId,
       r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       b.Title AS BookTitle,
       b.Author,
       c.InventoryNumber,
       l.IssueDate,
       l.DueDate,
       DATEDIFF(DAY, CAST(GETDATE() AS DATE), l.DueDate) AS DaysLeft
  FROM Loans l
  JOIN Readers r ON r.ReaderId = l.ReaderId
  JOIN Copies c ON c.CopyId = l.CopyId
  JOIN Books b ON b.BookId = c.BookId
 WHERE l.ReturnDate IS NULL
 ORDER BY l.DueDate, ReaderFIO;
GO

/* ------------------------------------------------------------
   Запрос 4. Просроченные выдачи и сумма штрафа
   ------------------------------------------------------------ */
SELECT r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       b.Title AS BookTitle,
       c.InventoryNumber,
       l.DueDate,
       DATEDIFF(DAY, l.DueDate, CAST(GETDATE() AS DATE)) AS OverdueDays,
       DATEDIFF(DAY, l.DueDate, CAST(GETDATE() AS DATE)) * 5.00 AS Fine
  FROM Loans l
  JOIN Readers r ON r.ReaderId = l.ReaderId
  JOIN Copies c ON c.CopyId = l.CopyId
  JOIN Books b ON b.BookId = c.BookId
 WHERE l.ReturnDate IS NULL
   AND l.DueDate < CAST(GETDATE() AS DATE)
 ORDER BY OverdueDays DESC, ReaderFIO;
GO

/* ------------------------------------------------------------
   Запрос 5. История выдач конкретного читателя
   ------------------------------------------------------------ */
DECLARE @CardNumber NVARCHAR(20) = N'Ч-0001';

SELECT b.Title AS BookTitle,
       b.Author,
       c.InventoryNumber,
       l.IssueDate,
       l.DueDate,
       l.ReturnDate,
       CASE
           WHEN l.ReturnDate IS NULL THEN N'На руках'
           WHEN l.ReturnDate > l.DueDate THEN N'Возвращена с просрочкой'
           ELSE N'Возвращена вовремя'
       END AS LoanStatus
  FROM Loans l
  JOIN Readers r ON r.ReaderId = l.ReaderId
  JOIN Copies c ON c.CopyId = l.CopyId
  JOIN Books b ON b.BookId = c.BookId
 WHERE r.LibraryCardNumber = @CardNumber
 ORDER BY l.IssueDate DESC;
GO

/* ------------------------------------------------------------
   Запрос 6. Топ-10 самых популярных книг
   ------------------------------------------------------------ */
SELECT TOP (10)
       b.Title,
       b.Author,
       COUNT(l.LoanId) AS LoansCount,
       COUNT(DISTINCT l.ReaderId) AS ReadersCount
  FROM Books b
  JOIN Copies c ON c.BookId = b.BookId
  JOIN Loans l ON l.CopyId = c.CopyId
 GROUP BY b.BookId, b.Title, b.Author
 ORDER BY LoansCount DESC, b.Title;
GO

/* ------------------------------------------------------------
   Запрос 7. Наличие экземпляров книги
   ------------------------------------------------------------ */
DECLARE @BookTitle NVARCHAR(200) = N'Война и мир';

SELECT b.Title,
       b.Author,
       COUNT(c.CopyId) AS TotalCopies,
       SUM(CASE WHEN c.Status = N'Доступен' THEN 1 ELSE 0 END) AS AvailableCopies,
       SUM(CASE WHEN c.Status = N'Выдан' THEN 1 ELSE 0 END) AS IssuedCopies,
       SUM(CASE WHEN c.Status = N'Списан' THEN 1 ELSE 0 END) AS WrittenOffCopies
  FROM Books b
  LEFT JOIN Copies c ON c.BookId = b.BookId
 WHERE b.Title LIKE N'%' + @BookTitle + N'%'
 GROUP BY b.BookId, b.Title, b.Author;
GO

/* ------------------------------------------------------------
   Запрос 8. Статистика библиотечного фонда по жанрам
   ------------------------------------------------------------ */
SELECT b.Genre,
       COUNT(DISTINCT b.BookId) AS BooksCount,
       COUNT(c.CopyId) AS CopiesCount,
       COUNT(l.LoanId) AS LoansCount
  FROM Books b
  LEFT JOIN Copies c ON c.BookId = b.BookId
  LEFT JOIN Loans l ON l.CopyId = c.CopyId
 GROUP BY b.Genre
 ORDER BY BooksCount DESC, b.Genre;
GO

/* ------------------------------------------------------------
   Запрос 9. Издательства и количество выпущенных книг
   ------------------------------------------------------------ */
SELECT p.Name AS Publisher,
       p.City,
       COUNT(DISTINCT b.BookId) AS BooksCount,
       COUNT(c.CopyId) AS CopiesCount
  FROM Publishers p
  LEFT JOIN Books b ON b.PublisherId = p.PublisherId
  LEFT JOIN Copies c ON c.BookId = b.BookId
 GROUP BY p.PublisherId, p.Name, p.City
 ORDER BY BooksCount DESC, p.Name;
GO

/* ------------------------------------------------------------
   Запрос 10. Читатели, не бравшие книги
   ------------------------------------------------------------ */
SELECT r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       r.Category,
       r.RegistrationDate
  FROM Readers r
  LEFT JOIN Loans l ON l.ReaderId = r.ReaderId
 WHERE l.LoanId IS NULL
 ORDER BY r.LastName, r.FirstName;
GO
