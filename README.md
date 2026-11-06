# library-wpf

Курсовая работа «Разработка базы данных „Библиотека“».

- СУБД: Microsoft SQL Server 2019 Express (LocalDB)
- Приложение: C#, WPF, .NET 10, Entity Framework Core 10, ADO.NET
- Модель: 6 таблиц — `Readers`, `Employees`, `Publishers`, `Books`, `Copies`, `Loans`

## Состав репозитория

- `App.xaml`, `MainWindow.xaml` — приложение WPF;
- `Models` — сущности предметной области;
- `Data` — контекст EF Core и доступ к данным;
- `Views` — страницы приложения и операции обслуживания;
- `sql/01_schema.sql` — создание базы данных, таблиц, индексов, триггера и процедур;
- `sql/02_seed.sql` — тестовые данные;
- `sql/03_queries.sql` — 10 аналитических запросов.

## Запуск

```powershell
sqllocaldb start MSSQLLocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i "sql/01_schema.sql"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -f 65001 -i "sql/02_seed.sql"
dotnet run --project LibraryWpf.csproj
```

Строка подключения:
`Server=(localdb)\MSSQLLocalDB;Database=LibraryDB;Integrated Security=True;TrustServerCertificate=True;Encrypt=False;`
