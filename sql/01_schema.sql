/* ============================================================
   Курсовая работа. Предметная область «Библиотека»
   Скрипт создания базы данных LibraryDB
   СУБД: Microsoft SQL Server 2019 Express (LocalDB)
   Упрощённая модель: 6 таблиц
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
    CONSTRAINT PK_Readers PRIMARY KEY (ReaderId),
    CONSTRAINT UQ_Readers_Card UNIQUE (LibraryCardNumber),
    CONSTRAINT CK_Readers_Category CHECK (Category IN (N'Студент', N'Преподаватель', N'Сотрудник', N'Школьник', N'Пенсионер', N'Другой'))
);
GO

/* ---------- Сотрудники ---------- */
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

/* ---------- Книги (библиографические описания) ---------- */
CREATE TABLE Books
(
    BookId          INT IDENTITY(1,1) NOT NULL,
    Title           NVARCHAR(200)     NOT NULL,
    ISBN            VARCHAR(20)       NOT NULL,
    PublicationYear SMALLINT          NOT NULL,
    Pages           INT               NOT NULL,
    Price           DECIMAL(10,2)     NOT NULL,
    Genre           NVARCHAR(60)      NOT NULL,
    Author          NVARCHAR(150)     NOT NULL,
    PublisherId     INT               NOT NULL,
    Annotation      NVARCHAR(500)     NULL,
    CONSTRAINT PK_Books PRIMARY KEY (BookId),
    CONSTRAINT UQ_Books_ISBN UNIQUE (ISBN),
    CONSTRAINT CK_Books_Year CHECK (PublicationYear BETWEEN 1500 AND 2100),
    CONSTRAINT CK_Books_Pages CHECK (Pages > 0),
    CONSTRAINT CK_Books_Price CHECK (Price >= 0),
    CONSTRAINT FK_Books_Publishers FOREIGN KEY (PublisherId) REFERENCES Publishers (PublisherId)
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

/* ---------- Индексы ---------- */
CREATE INDEX IX_Books_Title ON Books (Title);
CREATE INDEX IX_Books_Author ON Books (Author);
CREATE INDEX IX_Books_Genre ON Books (Genre);
CREATE INDEX IX_Books_PublisherId ON Books (PublisherId);
CREATE INDEX IX_Copies_BookId ON Copies (BookId);
CREATE INDEX IX_Copies_Status ON Copies (Status);
CREATE INDEX IX_Loans_ReaderId ON Loans (ReaderId);
CREATE INDEX IX_Loans_CopyId ON Loans (CopyId);
CREATE INDEX IX_Loans_ReturnDate ON Loans (ReturnDate);
CREATE INDEX IX_Loans_DueDate ON Loans (DueDate);

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
       b.Author,
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
       b.Author,
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

        IF NOT EXISTS (SELECT 1 FROM Readers WHERE ReaderId = @ReaderId)
            THROW 50003, N'Читатель не найден.', 1;

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
