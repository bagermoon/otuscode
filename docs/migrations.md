# Миграции EF Core

Команды ниже выполняются из корня репозитория.

Требования:
- Установлен EF CLI: `dotnet tool install --global dotnet-ef`
- Запущен проект .NET Aspire (AppHost), чтобы база данных была поднята и значения конфигурации/переменные окружения были доступны.

## Сервис Restaurant

- Список миграций (применённых и ожидающих):

```pwsh
dotnet ef migrations --startup-project .\src\RestoRate.Migrations --project .\src\Restaurant\RestoRate.Restaurant.Infrastructure list
```

- Создать новую миграцию (замените `SomeMigration` на нужное имя):

```pwsh
dotnet ef migrations --startup-project .\src\RestoRate.Migrations --project .\src\Restaurant\RestoRate.Restaurant.Infrastructure add SomeMigration
```

Примечания:
- Параметр `--startup-project` указывает на проект дизайна (`RestoRate.Migrations`).
- Параметр `--project` указывает на инфраструктурный проект, содержащий `DbContext`.
- Если AppHost не запущен, убедитесь, что PostgreSQL доступен и строки подключения корректны.