# library-wpf

Курсовая работа «Разработка базы данных „Библиотека“».

- СУБД: Microsoft SQL Server 2019 Express (LocalDB)
- Приложение: C#, WPF, .NET 10, Entity Framework Core 10, ADO.NET
- Модель: 8 таблиц — `Readers`, `Employees`, `Publishers`, `Books`, `Authors`, `BookAuthors`, `Copies`, `Loans`

## Состав репозитория

- `App.xaml`, `MainWindow.xaml` — приложение WPF;
- `Models` — сущности предметной области;
- `Data` — контекст EF Core и доступ к данным;
- `Views` — страницы приложения и операции обслуживания;
- `sql/01_schema.sql` — создание базы данных, таблиц, индексов, триггера и процедур;
- `sql/02_seed.sql` — тестовые данные;
- `sql/03_queries.sql` — 10 аналитических запросов для исходной схемы;
- `sql_v3/01_add_authors.sql` — миграция схемы с авторами и связью M:N;
- `sql_v3/03_queries.sql` — 5 аналитических запросов для схемы с авторами.

## Запуск

```powershell
sqllocaldb start MSSQLLocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i "sql/01_schema.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i "sql/02_seed.sql"
dotnet run --project LibraryWpf.csproj
```

Строка подключения:
`Server=(localdb)\MSSQLLocalDB;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;`
