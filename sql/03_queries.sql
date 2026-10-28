/* ============================================================
   Курсовая работа. Предметная область «Библиотека»
   Запросы к базе данных LibraryDB
   ============================================================ */
USE LibraryDB;
GO
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

/* ------------------------------------------------------------
   Запрос 1. Книги заданного автора (по фамилии)
   ------------------------------------------------------------ */
DECLARE @AuthorLastName NVARCHAR(50) = N'Толстой';

SELECT DISTINCT
       b.BookId,
       b.Title,
       b.PublicationYear,
       g.Name AS Genre,
       p.Name AS Publisher
  FROM Books b
  JOIN BookAuthors ba ON ba.BookId = b.BookId
  JOIN Authors a ON a.AuthorId = ba.AuthorId
  JOIN Genres g ON g.GenreId = b.GenreId
  JOIN Publishers p ON p.PublisherId = b.PublisherId
 WHERE a.LastName = @AuthorLastName
 ORDER BY b.Title;
GO

/* ------------------------------------------------------------
   Запрос 2. Книги выбранного жанра с количеством экземпляров
   ------------------------------------------------------------ */
DECLARE @GenreName NVARCHAR(60) = N'Фантастика';

SELECT b.Title,
       b.PublicationYear,
       p.Name AS Publisher,
       COUNT(c.CopyId) AS CopiesCount
  FROM Books b
  JOIN Genres g ON g.GenreId = b.GenreId
  JOIN Publishers p ON p.PublisherId = b.PublisherId
  LEFT JOIN Copies c ON c.BookId = b.BookId
 WHERE g.Name = @GenreName
 GROUP BY b.BookId, b.Title, b.PublicationYear, p.Name
 ORDER BY b.Title;
GO

/* ------------------------------------------------------------
   Запрос 3. Активные выдачи книг
   ------------------------------------------------------------ */
SELECT l.LoanId,
       r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       b.Title AS BookTitle,
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
       COUNT(l.LoanId) AS LoansCount,
       COUNT(DISTINCT l.ReaderId) AS ReadersCount
  FROM Books b
  JOIN Copies c ON c.BookId = b.BookId
  JOIN Loans l ON l.CopyId = c.CopyId
 GROUP BY b.BookId, b.Title
 ORDER BY LoansCount DESC, b.Title;
GO

/* ------------------------------------------------------------
   Запрос 7. Наличие экземпляров книги
   ------------------------------------------------------------ */
DECLARE @BookTitle NVARCHAR(200) = N'Война и мир';

SELECT b.Title,
       COUNT(c.CopyId) AS TotalCopies,
       SUM(CASE WHEN c.Status = N'Доступен' THEN 1 ELSE 0 END) AS AvailableCopies,
       SUM(CASE WHEN c.Status = N'Выдан' THEN 1 ELSE 0 END) AS IssuedCopies,
       SUM(CASE WHEN c.Status = N'Списан' THEN 1 ELSE 0 END) AS WrittenOffCopies
  FROM Books b
  LEFT JOIN Copies c ON c.BookId = b.BookId
 WHERE b.Title LIKE N'%' + @BookTitle + N'%'
 GROUP BY b.BookId, b.Title;
GO

/* ------------------------------------------------------------
   Запрос 8. Статистика библиотечного фонда по жанрам
   ------------------------------------------------------------ */
SELECT g.Name AS Genre,
       COUNT(DISTINCT b.BookId) AS BooksCount,
       COUNT(c.CopyId) AS CopiesCount,
       COUNT(l.LoanId) AS LoansCount
  FROM Genres g
  LEFT JOIN Books b ON b.GenreId = g.GenreId
  LEFT JOIN Copies c ON c.BookId = b.BookId
  LEFT JOIN Loans l ON l.CopyId = c.CopyId
 GROUP BY g.GenreId, g.Name
 ORDER BY BooksCount DESC, g.Name;
GO

/* ------------------------------------------------------------
   Запрос 9. Очередь активных бронирований
   ------------------------------------------------------------ */
SELECT b.Title AS BookTitle,
       r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       res.ReservationDate,
       ROW_NUMBER() OVER (PARTITION BY res.BookId ORDER BY res.ReservationDate) AS QueuePosition
  FROM Reservations res
  JOIN Books b ON b.BookId = res.BookId
  JOIN Readers r ON r.ReaderId = res.ReaderId
 WHERE res.Status = N'Активно'
 ORDER BY b.Title, QueuePosition;
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
