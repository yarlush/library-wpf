/* ============================================================
   v3: добавление Authors и BookAuthors к упрощённой схеме
   Books (Genre, Author) -> Books (Genre, PublisherId)
   ============================================================ */
USE LibraryDB;
GO
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

IF OBJECT_ID(N'dbo.Authors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Authors
    (
        AuthorId   INT IDENTITY(1,1) NOT NULL,
        LastName   NVARCHAR(50) NOT NULL,
        FirstName  NVARCHAR(50) NOT NULL,
        MiddleName NVARCHAR(50) NULL,
        CONSTRAINT PK_Authors PRIMARY KEY (AuthorId)
    );
END
GO

IF OBJECT_ID(N'dbo.BookAuthors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BookAuthors
    (
        BookId   INT NOT NULL,
        AuthorId INT NOT NULL,
        CONSTRAINT PK_BookAuthors PRIMARY KEY (BookId, AuthorId),
        CONSTRAINT FK_BookAuthors_Books FOREIGN KEY (BookId)
            REFERENCES dbo.Books (BookId) ON DELETE CASCADE,
        CONSTRAINT FK_BookAuthors_Authors FOREIGN KEY (AuthorId)
            REFERENCES dbo.Authors (AuthorId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Authors)
BEGIN
    SET IDENTITY_INSERT dbo.Authors ON;
    INSERT INTO dbo.Authors (AuthorId, LastName, FirstName, MiddleName) VALUES
    (1, N'Толстой', N'Лев', N'Николаевич'),
    (2, N'Достоевский', N'Фёдор', N'Михайлович'),
    (3, N'Булгаков', N'Михаил', N'Афанасьевич'),
    (4, N'Пушкин', N'Александр', N'Сергеевич'),
    (5, N'Стругацкий', N'Аркадий', N'Натанович'),
    (6, N'Стругацкий', N'Борис', N'Натанович'),
    (7, N'Азимов', N'Айзек', NULL),
    (8, N'Кристи', N'Агата', NULL),
    (9, N'Хокинг', N'Стивен', NULL),
    (10, N'Кнут', N'Дональд', N'Эрвин'),
    (11, N'Дейт', N'Кристофер', N'Дж.'),
    (12, N'Коннолли', N'Томас', NULL),
    (13, N'Павловская', N'Татьяна', N'Александровна'),
    (14, N'Выготский', N'Лев', N'Семёнович'),
    (15, N'Носов', N'Николай', N'Николаевич'),
    (16, N'Роулинг', N'Джоан', NULL),
    (17, N'Сент-Экзюпери', N'Антуан', N'де'),
    (18, N'Оруэлл', N'Джордж', NULL),
    (19, N'Карамзин', N'Николай', N'Михайлович'),
    (20, N'Самуэльсон', N'Пол', NULL),
    (21, N'Маркс', N'Карл', NULL),
    (22, N'Ахматова', N'Анна', N'Андреевна'),
    (23, N'Конан Дойл', N'Артур', NULL),
    (24, N'Остер', N'Григорий', N'Бенционович');
    SET IDENTITY_INSERT dbo.Authors OFF;
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BookAuthors)
BEGIN
    INSERT INTO dbo.BookAuthors (BookId, AuthorId) VALUES
    (1,1),(2,1),(3,2),(4,2),(5,3),(6,3),(7,4),(8,4),
    (9,5),(9,6),(10,5),(10,6),(11,7),(12,7),(13,8),(14,8),
    (15,9),(16,10),(17,11),(18,12),(19,13),(20,14),(21,14),(22,15),
    (23,16),(24,17),(25,18),(26,19),(27,20),(28,21),(29,22),(30,23);
END
GO

IF COL_LENGTH(N'dbo.Books', N'Author') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Books_Author' AND object_id = OBJECT_ID(N'dbo.Books'))
        DROP INDEX IX_Books_Author ON dbo.Books;
    ALTER TABLE dbo.Books DROP COLUMN Author;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_BookAuthors_AuthorId' AND object_id = OBJECT_ID(N'dbo.BookAuthors'))
    CREATE INDEX IX_BookAuthors_AuthorId ON dbo.BookAuthors (AuthorId);
GO

DBCC CHECKIDENT ('dbo.Authors', RESEED, 24) WITH NO_INFOMSGS;
GO
PRINT N'v3: Authors/BookAuthors готовы';
GO
