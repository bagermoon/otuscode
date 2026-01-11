# Миграции EF Core

Команды ниже выполняются из корня репозитория.

## Сервис Restaurant

- Список миграций (применённых и ожидающих):

    ```pwsh
    dotnet aspire exec --resource ServiceRestaurantApi -- dotnet ef migrations --startup-project ..\..\RestoRate.Migrations --project ..\RestoRate.RestaurantService.Infrastructure list
    ```

  Примечание о рабочей директории и создании миграции

  - Когда вы запускаете команду через `dotnet aspire exec --resource ServiceRestaurantApi -- ...`, команда выполняется с текущей директорией, установленной в корень ресурса ServiceRestaurantApi (в этом репозитории это `src/RestaurantService/RestoRate.RestaurantService.Api`).
  - Поэтому относительные пути для параметров `--startup-project` и `--project` указываются относительно этого каталога:

    - `--startup-project ..\..\RestoRate.Migrations` (поднимаемся до `src`)
    - `--project ..\RestoRate.RestaurantService.Infrastructure` (переходим в соседний проект внутри `src/RestaurantService`)

- Создать новую миграцию локально из корня репозитория (без `aspire`):

    ```pwsh
    dotnet ef migrations --startup-project .\src\RestoRate.Migrations --project .\src\RestaurantService\RestoRate.RestaurantService.Infrastructure add SomeMigration
    ```

- Создать новую миграцию через `aspire exec` (использует пути относительно корня ресурса):

    ```pwsh
    dotnet aspire exec --resource ServiceRestaurantApi -- dotnet ef migrations --startup-project ..\..\RestoRate.Migrations --project ..\RestoRate.RestaurantService.Infrastructure add SomeMigration
    ```

- Удаление последней миграции:

    ```pwsh
    dotnet aspire exec --resource ServiceRestaurantApi -- dotnet ef migrations --startup-project ..\..\RestoRate.Migrations --project ..\RestoRate.RestaurantService.Infrastructure remove
    ```

Примечания:

- Параметр `--startup-project` указывает на проект дизайна (`RestoRate.Migrations`).
- Параметр `--project` указывает на инфраструктурный проект, содержащий `DbContext`.
