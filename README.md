# LibraryWpf

WPF-приложение «Библиотека»: C#, .NET 10, EF Core 10, SQL Server LocalDB.

## Запуск

1. Создайте БД `LibraryDB` скриптами из `sql/`.
2. Выполните:

```powershell
sqllocaldb start MSSQLLocalDB
dotnet run --project LibraryWpf.csproj
```

Строка подключения:
`Server=(localdb)\MSSQLLocalDB;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;`

## SQL

- `sql/01_schema.sql` — схема БД и хранимые процедуры;
- `sql/02_seed.sql` — тестовые данные;
- `sql/03_queries.sql` — 10 аналитических запросов.
