# LibraryWpf

WPF-приложение для курсовой работы «Разработка базы данных „Библиотека“».

## Стек

- C#, WPF, .NET 10
- Entity Framework Core 10 (`Microsoft.EntityFrameworkCore.SqlServer`)
- ADO.NET (`Microsoft.Data.SqlClient`)
- Microsoft SQL Server 2019 Express (LocalDB)

## Возможности

- справочники книг, читателей, экземпляров, авторов, издательств, жанров и сотрудников;
- связи книг и авторов;
- журнал выдач и бронирований;
- оформление выдачи, возврата, бронирования и списания экземпляров;
- поиск и фильтрация по таблицам.

## Запуск

1. Установите .NET 10 SDK и SQL Server LocalDB.
2. Создайте базу `LibraryDB` и заполните её тестовыми данными
   (SQL-скрипты входят в комплект курсовой работы).
3. Запустите LocalDB:
   ```powershell
   sqllocaldb start MSSQLLocalDB
   ```
4. Соберите и запустите приложение:
   ```powershell
   dotnet run --project LibraryWpf.csproj
   ```

Строка подключения:

```text
Server=(localdb)\MSSQLLocalDB;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;
```

## Структура

- `Data/` — `LibraryContext`, работа с SQL через `SqlClient`;
- `Models/` — сущности EF Core;
- `Views/` — страницы и универсальные CRUD-компоненты;
- `MainWindow.xaml` — главное окно с навигацией;
- `Services/` — вспомогательные сервисы (скриншоты).
