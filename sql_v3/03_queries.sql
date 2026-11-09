/* ============================================================
   Курсовая работа. Версия v2: 5 основных запросов
   Схема: Books (Genre — текст, без Author), Authors, BookAuthors,
          Publishers, Copies, Readers, Employees, Loans.
   ============================================================ */
USE LibraryDB;
GO
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

/* Запрос 1. Книги заданного автора */
DECLARE @Author NVARCHAR(150) = N'Толстой';

SELECT b.BookId,
       b.Title,
       b.PublicationYear,
       b.Genre,
       p.Name AS Publisher,
       STRING_AGG(a.LastName + N' ' + a.FirstName, N', ')
           WITHIN GROUP (ORDER BY a.LastName) AS Authors
  FROM Books b
  JOIN BookAuthors ba ON ba.BookId = b.BookId
  JOIN Authors a ON a.AuthorId = ba.AuthorId
  JOIN Publishers p ON p.PublisherId = b.PublisherId
 WHERE EXISTS (
       SELECT 1
         FROM BookAuthors ba2
         JOIN Authors a2 ON a2.AuthorId = ba2.AuthorId
        WHERE ba2.BookId = b.BookId
          AND a2.LastName LIKE N'%' + @Author + N'%')
 GROUP BY b.BookId, b.Title, b.PublicationYear, b.Genre, p.Name
 ORDER BY b.Title;
GO

/* Запрос 2. Книги выбранного жанра */
DECLARE @Genre NVARCHAR(60) = N'Фантастика';

SELECT b.Title,
       b.PublicationYear,
       p.Name AS Publisher,
       (SELECT STRING_AGG(a.LastName + N' ' + a.FirstName, N', ')
                   WITHIN GROUP (ORDER BY a.LastName, a.FirstName)
        FROM BookAuthors ba
        JOIN Authors a ON a.AuthorId = ba.AuthorId
        WHERE ba.BookId = b.BookId) AS Authors,
       (SELECT COUNT(*)
        FROM Copies c
        WHERE c.BookId = b.BookId) AS CopiesCount
  FROM Books b
  JOIN Publishers p ON p.PublisherId = b.PublisherId
 WHERE b.Genre = @Genre
 ORDER BY b.Title;
GO

/* Запрос 3. Активные выдачи книг */
SELECT l.LoanId,
       r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       b.Title AS BookTitle,
       STRING_AGG(a.LastName + N' ' + a.FirstName, N', ')
           WITHIN GROUP (ORDER BY a.LastName) AS Authors,
       c.InventoryNumber,
       l.IssueDate,
       l.DueDate,
       DATEDIFF(DAY, CAST(GETDATE() AS DATE), l.DueDate) AS DaysLeft
  FROM Loans l
  JOIN Readers r ON r.ReaderId = l.ReaderId
  JOIN Copies c ON c.CopyId = l.CopyId
  JOIN Books b ON b.BookId = c.BookId
  LEFT JOIN BookAuthors ba ON ba.BookId = b.BookId
  LEFT JOIN Authors a ON a.AuthorId = ba.AuthorId
 WHERE l.ReturnDate IS NULL
 GROUP BY l.LoanId, r.LibraryCardNumber, r.LastName, r.FirstName, r.MiddleName,
          b.Title, c.InventoryNumber, l.IssueDate, l.DueDate
 ORDER BY l.DueDate, ReaderFIO;
GO

/* Запрос 4. Просроченные выдачи и сумма штрафа */
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

/* Запрос 5. История выдач конкретного читателя */
DECLARE @CardNumber NVARCHAR(20) = N'Ч-0001';

SELECT b.Title AS BookTitle,
       STRING_AGG(a.LastName + N' ' + a.FirstName, N', ')
           WITHIN GROUP (ORDER BY a.LastName) AS Authors,
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
  LEFT JOIN BookAuthors ba ON ba.BookId = b.BookId
  LEFT JOIN Authors a ON a.AuthorId = ba.AuthorId
 WHERE r.LibraryCardNumber = @CardNumber
 GROUP BY b.Title, c.InventoryNumber, l.IssueDate, l.DueDate, l.ReturnDate
 ORDER BY l.IssueDate DESC;
GO
