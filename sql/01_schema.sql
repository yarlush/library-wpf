/* ============================================================
   Курсовая работа. Предметная область «Библиотека»
   Скрипт создания базы данных LibraryDB
   СУБД: Microsoft SQL Server 2019 Express (LocalDB)
   ============================================================ */

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF DB_ID(N'LibraryDB') IS NOT NULL
BEGIN
    ALTER DATABASE LibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryDB;
END
GO

CREATE DATABASE LibraryDB;
GO

USE LibraryDB;
GO

/* ---------- Справочник жанров ---------- */
CREATE TABLE Genres
(
    GenreId     INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(60)      NOT NULL,
    Description NVARCHAR(300)     NULL,
    CONSTRAINT PK_Genres PRIMARY KEY (GenreId),
    CONSTRAINT UQ_Genres_Name UNIQUE (Name)
);
GO

/* ---------- Издательства ---------- */
CREATE TABLE Publishers
(
    PublisherId INT IDENTITY(1,1) NOT NULL,
    Name        NVARCHAR(120)     NOT NULL,
    City        NVARCHAR(60)      NOT NULL,
    Address     NVARCHAR(200)     NULL,
    Phone       NVARCHAR(30)      NULL,
    Email       NVARCHAR(80)      NULL,
    CONSTRAINT PK_Publishers PRIMARY KEY (PublisherId)
);
GO

/* ---------- Авторы ---------- */
CREATE TABLE Authors
(
    AuthorId   INT IDENTITY(1,1) NOT NULL,
    LastName   NVARCHAR(50)      NOT NULL,
    FirstName  NVARCHAR(50)      NOT NULL,
    MiddleName NVARCHAR(50)      NULL,
    Country    NVARCHAR(60)      NULL,
    BirthYear  SMALLINT          NULL,
    CONSTRAINT PK_Authors PRIMARY KEY (AuthorId),
    CONSTRAINT CK_Authors_BirthYear CHECK (BirthYear IS NULL OR BirthYear BETWEEN 1000 AND 2100)
);
GO

/* ---------- Книги (библиографические описания) ---------- */
CREATE TABLE Books
(
    BookId          INT IDENTITY(1,1) NOT NULL,
    Title           NVARCHAR(200)     NOT NULL,
    ISBN            VARCHAR(20)       NOT NULL,
    PublicationYear SMALLINT          NOT NULL,
    Pages           INT               NOT NULL,
    Price           DECIMAL(10,2)     NOT NULL,
    GenreId         INT               NOT NULL,
    PublisherId     INT               NOT NULL,
    Annotation      NVARCHAR(500)     NULL,
    CONSTRAINT PK_Books PRIMARY KEY (BookId),
    CONSTRAINT UQ_Books_ISBN UNIQUE (ISBN),
    CONSTRAINT CK_Books_Year CHECK (PublicationYear BETWEEN 1500 AND 2100),
    CONSTRAINT CK_Books_Pages CHECK (Pages > 0),
    CONSTRAINT CK_Books_Price CHECK (Price >= 0),
    CONSTRAINT FK_Books_Genres FOREIGN KEY (GenreId) REFERENCES Genres (GenreId),
    CONSTRAINT FK_Books_Publishers FOREIGN KEY (PublisherId) REFERENCES Publishers (PublisherId)
);
GO

/* ---------- Связь «книга – автор» (M:N) ---------- */
CREATE TABLE BookAuthors
(
    BookId   INT NOT NULL,
    AuthorId INT NOT NULL,
    CONSTRAINT PK_BookAuthors PRIMARY KEY (BookId, AuthorId),
    CONSTRAINT FK_BookAuthors_Books FOREIGN KEY (BookId) REFERENCES Books (BookId) ON DELETE CASCADE,
    CONSTRAINT FK_BookAuthors_Authors FOREIGN KEY (AuthorId) REFERENCES Authors (AuthorId)
);
GO

/* ---------- Экземпляры книг (фонд) ---------- */
CREATE TABLE Copies
(
    CopyId          INT IDENTITY(1,1) NOT NULL,
    BookId          INT               NOT NULL,
    InventoryNumber NVARCHAR(20)      NOT NULL,
    Status          NVARCHAR(20)      NOT NULL CONSTRAINT DF_Copies_Status DEFAULT N'Доступен',
    AcquisitionDate DATE              NOT NULL CONSTRAINT DF_Copies_AcquisitionDate DEFAULT CAST(GETDATE() AS DATE),
    Hall            NVARCHAR(40)      NULL,
    Shelf           NVARCHAR(40)      NULL,
    CONSTRAINT PK_Copies PRIMARY KEY (CopyId),
    CONSTRAINT UQ_Copies_InventoryNumber UNIQUE (InventoryNumber),
    CONSTRAINT CK_Copies_Status CHECK (Status IN (N'Доступен', N'Выдан', N'Списан')),
    CONSTRAINT FK_Copies_Books FOREIGN KEY (BookId) REFERENCES Books (BookId)
);
GO

/* ---------- Читатели ---------- */
CREATE TABLE Readers
(
    ReaderId          INT IDENTITY(1,1) NOT NULL,
    LibraryCardNumber NVARCHAR(20)      NOT NULL,
    LastName          NVARCHAR(50)      NOT NULL,
    FirstName         NVARCHAR(50)      NOT NULL,
    MiddleName        NVARCHAR(50)      NULL,
    BirthDate         DATE              NULL,
    Address           NVARCHAR(200)     NULL,
    Phone             NVARCHAR(30)      NULL,
    Email             NVARCHAR(80)      NULL,
    Category          NVARCHAR(30)      NOT NULL,
    RegistrationDate  DATE              NOT NULL CONSTRAINT DF_Readers_RegistrationDate DEFAULT CAST(GETDATE() AS DATE),
    IsActive          BIT               NOT NULL CONSTRAINT DF_Readers_IsActive DEFAULT 1,
    CONSTRAINT PK_Readers PRIMARY KEY (ReaderId),
    CONSTRAINT UQ_Readers_Card UNIQUE (LibraryCardNumber),
    CONSTRAINT CK_Readers_Category CHECK (Category IN (N'Студент', N'Преподаватель', N'Сотрудник', N'Школьник', N'Пенсионер', N'Другой'))
);
GO

/* ---------- Сотрудники библиотеки ---------- */
CREATE TABLE Employees
(
    EmployeeId INT IDENTITY(1,1) NOT NULL,
    LastName   NVARCHAR(50)      NOT NULL,
    FirstName  NVARCHAR(50)      NOT NULL,
    MiddleName NVARCHAR(50)      NULL,
    Position   NVARCHAR(60)      NOT NULL,
    Phone      NVARCHAR(30)      NULL,
    HireDate   DATE              NOT NULL,
    CONSTRAINT PK_Employees PRIMARY KEY (EmployeeId)
);
GO

/* ---------- Выдачи книг ---------- */
CREATE TABLE Loans
(
    LoanId     INT IDENTITY(1,1) NOT NULL,
    CopyId     INT               NOT NULL,
    ReaderId   INT               NOT NULL,
    EmployeeId INT               NOT NULL,
    IssueDate  DATE              NOT NULL CONSTRAINT DF_Loans_IssueDate DEFAULT CAST(GETDATE() AS DATE),
    DueDate    DATE              NOT NULL,
    ReturnDate DATE              NULL,
    CONSTRAINT PK_Loans PRIMARY KEY (LoanId),
    CONSTRAINT FK_Loans_Copies FOREIGN KEY (CopyId) REFERENCES Copies (CopyId),
    CONSTRAINT FK_Loans_Readers FOREIGN KEY (ReaderId) REFERENCES Readers (ReaderId),
    CONSTRAINT FK_Loans_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees (EmployeeId),
    CONSTRAINT CK_Loans_Dates CHECK (DueDate >= IssueDate),
    CONSTRAINT CK_Loans_ReturnDate CHECK (ReturnDate IS NULL OR ReturnDate >= IssueDate)
);
GO

/* ---------- Бронирования ---------- */
CREATE TABLE Reservations
(
    ReservationId   INT IDENTITY(1,1) NOT NULL,
    BookId          INT               NOT NULL,
    ReaderId        INT               NOT NULL,
    ReservationDate DATE              NOT NULL CONSTRAINT DF_Reservations_Date DEFAULT CAST(GETDATE() AS DATE),
    Status          NVARCHAR(20)      NOT NULL CONSTRAINT DF_Reservations_Status DEFAULT N'Активно',
    CONSTRAINT PK_Reservations PRIMARY KEY (ReservationId),
    CONSTRAINT CK_Reservations_Status CHECK (Status IN (N'Активно', N'Выполнено', N'Отменено')),
    CONSTRAINT FK_Reservations_Books FOREIGN KEY (BookId) REFERENCES Books (BookId),
    CONSTRAINT FK_Reservations_Readers FOREIGN KEY (ReaderId) REFERENCES Readers (ReaderId)
);
GO

/* ---------- Индексы ---------- */
CREATE INDEX IX_Books_Title ON Books (Title);
CREATE INDEX IX_Books_GenreId ON Books (GenreId);
CREATE INDEX IX_Books_PublisherId ON Books (PublisherId);
CREATE INDEX IX_BookAuthors_AuthorId ON BookAuthors (AuthorId);
CREATE INDEX IX_Copies_BookId ON Copies (BookId);
CREATE INDEX IX_Copies_Status ON Copies (Status);
CREATE INDEX IX_Loans_ReaderId ON Loans (ReaderId);
CREATE INDEX IX_Loans_CopyId ON Loans (CopyId);
CREATE INDEX IX_Loans_ReturnDate ON Loans (ReturnDate);
CREATE INDEX IX_Loans_DueDate ON Loans (DueDate);
CREATE INDEX IX_Reservations_BookId ON Reservations (BookId);
CREATE INDEX IX_Reservations_ReaderId ON Reservations (ReaderId);

/* Один экземпляр не может быть одновременно выдан более одного раза */
CREATE UNIQUE INDEX UX_Loans_ActiveCopy ON Loans (CopyId) WHERE ReturnDate IS NULL;
GO

/* ---------- Триггер синхронизации статуса экземпляра ---------- */
CREATE TRIGGER trg_Loans_SyncCopyStatus
ON Loans
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE c
       SET c.Status = CASE
                          WHEN EXISTS (SELECT 1
                                         FROM Loans l
                                        WHERE l.CopyId = c.CopyId
                                          AND l.ReturnDate IS NULL)
                              THEN N'Выдан'
                          ELSE N'Доступен'
                      END
      FROM Copies c
     WHERE c.CopyId IN (SELECT CopyId FROM inserted
                        UNION
                        SELECT CopyId FROM deleted)
       AND c.Status <> N'Списан';
END;
GO

/* ---------- Представления ---------- */
CREATE VIEW vw_ActiveLoans
AS
SELECT l.LoanId,
       r.LibraryCardNumber,
       r.LastName + N' ' + r.FirstName + ISNULL(N' ' + r.MiddleName, N'') AS ReaderFIO,
       b.Title AS BookTitle,
       c.InventoryNumber,
       l.IssueDate,
       l.DueDate,
       DATEDIFF(DAY, l.DueDate, CAST(GETDATE() AS DATE)) AS OverdueDays,
       e.LastName + N' ' + e.FirstName AS EmployeeFIO
  FROM Loans l
  JOIN Readers r ON r.ReaderId = l.ReaderId
  JOIN Copies c ON c.CopyId = l.CopyId
  JOIN Books b ON b.BookId = c.BookId
  JOIN Employees e ON e.EmployeeId = l.EmployeeId
 WHERE l.ReturnDate IS NULL;
GO

CREATE VIEW vw_AvailableCopies
AS
SELECT c.CopyId,
       c.InventoryNumber,
       b.BookId,
       b.Title,
       b.PublicationYear,
       c.Hall,
       c.Shelf
  FROM Copies c
  JOIN Books b ON b.BookId = c.BookId
 WHERE c.Status = N'Доступен';
GO

/* ---------- Хранимая процедура выдачи книги ---------- */
CREATE PROCEDURE usp_IssueBook
    @CopyId     INT,
    @ReaderId   INT,
    @EmployeeId INT,
    @DueDate    DATE = NULL,
    @LoanId     INT  = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @DueDate IS NULL
        SET @DueDate = DATEADD(DAY, 14, CAST(GETDATE() AS DATE));

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Copies WHERE CopyId = @CopyId)
            THROW 50001, N'Экземпляр не найден.', 1;

        IF NOT EXISTS (SELECT 1 FROM Copies WHERE CopyId = @CopyId AND Status = N'Доступен')
            THROW 50002, N'Экземпляр недоступен для выдачи.', 1;

        IF NOT EXISTS (SELECT 1 FROM Readers WHERE ReaderId = @ReaderId AND IsActive = 1)
            THROW 50003, N'Читатель не найден или неактивен.', 1;

        IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeId = @EmployeeId)
            THROW 50004, N'Сотрудник не найден.', 1;

        IF EXISTS (SELECT 1 FROM Loans WHERE CopyId = @CopyId AND ReturnDate IS NULL)
            THROW 50005, N'По этому экземпляру уже есть активная выдача.', 1;

        INSERT INTO Loans (CopyId, ReaderId, EmployeeId, IssueDate, DueDate, ReturnDate)
        VALUES (@CopyId, @ReaderId, @EmployeeId, CAST(GETDATE() AS DATE), @DueDate, NULL);

        SET @LoanId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

/* ---------- Хранимая процедура возврата книги ---------- */
CREATE PROCEDURE usp_ReturnBook
    @LoanId     INT,
    @ReturnDate DATE = NULL,
    @Fine       DECIMAL(10,2) = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @ReturnDate IS NULL
        SET @ReturnDate = CAST(GETDATE() AS DATE);

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Loans WHERE LoanId = @LoanId)
            THROW 50011, N'Выдача не найдена.', 1;

        IF EXISTS (SELECT 1 FROM Loans WHERE LoanId = @LoanId AND ReturnDate IS NOT NULL)
            THROW 50012, N'Книга по этой выдаче уже возвращена.', 1;

        UPDATE Loans
           SET ReturnDate = @ReturnDate
         WHERE LoanId = @LoanId;

        SELECT @Fine = CASE
                           WHEN DATEDIFF(DAY, DueDate, @ReturnDate) > 0
                               THEN DATEDIFF(DAY, DueDate, @ReturnDate) * 5.00
                           ELSE 0
                       END
          FROM Loans
         WHERE LoanId = @LoanId;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

/* ---------- Хранимая процедура бронирования ---------- */
CREATE PROCEDURE usp_ReserveBook
    @BookId   INT,
    @ReaderId INT,
    @ReservationId INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM Books WHERE BookId = @BookId)
            THROW 50021, N'Книга не найдена.', 1;

        IF NOT EXISTS (SELECT 1 FROM Readers WHERE ReaderId = @ReaderId AND IsActive = 1)
            THROW 50022, N'Читатель не найден или неактивен.', 1;

        IF EXISTS (SELECT 1
                     FROM Reservations
                    WHERE BookId = @BookId
                      AND ReaderId = @ReaderId
                      AND Status = N'Активно')
            THROW 50023, N'У читателя уже есть активное бронирование этой книги.', 1;

        INSERT INTO Reservations (BookId, ReaderId, ReservationDate, Status)
        VALUES (@BookId, @ReaderId, CAST(GETDATE() AS DATE), N'Активно');

        SET @ReservationId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

/* ---------- Хранимая процедура списания экземпляра ---------- */
CREATE PROCEDURE usp_WriteOffCopy
    @CopyId INT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Copies WHERE CopyId = @CopyId)
        THROW 50031, N'Экземпляр не найден.', 1;

    IF EXISTS (SELECT 1 FROM Loans WHERE CopyId = @CopyId AND ReturnDate IS NULL)
        THROW 50032, N'Нельзя списать экземпляр, находящийся на руках.', 1;

    UPDATE Copies
       SET Status = N'Списан'
     WHERE CopyId = @CopyId;
END;
GO
