# Тестирование в RestoRate

## E2E тесты

`RestoRate.E2ETests` — проект для end-to-end (E2E) тестов, покрывающих взаимодействие сервисов, инфраструктуры и UI. Использует [Playwright](https://playwright.dev/dotnet/) для E2E тестирования, включая сценарии с аутентификацией.

### Быстрый старт

### Требования

- Docker должен быть запущен (используется для сервисов и тестовой среды).
- .NET 9 SDK установлен.
- Playwright установлен (см. ниже).


### Установка Playwright

В корне репозитория есть скрипт [`playwright.ps1`](playwright.ps1):

- Установить браузеры:
  ```
  pwsh ./playwright.ps1 install
  ```
- Генерация кода теста (запуск интерактивного рекордера):
  ```
  pwsh ./playwright.ps1 codegen https://localhost:7291/
  ```
- Просмотр trace после теста:
  ```
  pwsh ./playwright.ps1 show-trace .\artifacts\bin\RestoRate.E2ETests\debug\playwright-traces\IntegrationTest1.DashboardIsLoaded.zip
  ```


### Запуск E2E тестов

1. Убедитесь, что Docker запущен.
2. Запустите все сервисы через Aspire AppHost:
  ```
  dotnet run --project src/RestoRate.AppHost
  ```
3. Запустите тесты:
  ```
  dotnet test tests/RestoRate.E2ETests
  ```


### Создание E2E теста

- Тесты размещаются в проекте [`tests/RestoRate.E2ETests`](../tests/RestoRate.E2ETests).
- Для тестов с аутентификацией используйте атрибут `[User(TestUser.Admin)]` (или другой пользователь).
- В проекте используется файл `GlobalUsings.cs` для глобальных директив:
  ```csharp
  global using Microsoft.Playwright;
  global using RestoRate.E2ETests.Auth;
  global using RestoRate.E2ETests.Base;
  global using Microsoft.Playwright.Xunit.v3;
  ```
- Пример теста с Playwright и авторизацией:

  ```csharp
  // filepath: tests/RestoRate.E2ETests/IntegrationTest1.cs
  [User(TestUser.Admin)]
  public class IntegrationTest1(AspireAppHost appHost) : BasePageTest(appHost)
  {
    [Fact]
    public async Task DashboardIsLoaded()
    {
      await Page.GotoAsync("/");
      // Проверка: кнопка Logout видна
      await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();
    }
  }
  ```

- Для новых тестов используйте базовые классы и инфраструктуру из проекта, чтобы обеспечить запуск в Aspire окружении.

### Примечания


- Все тесты предполагают, что сервисы подняты через Aspire и доступны для взаимодействия.
- Для корректной работы Playwright и браузеров используйте скрипт установки перед первым запуском.
- Для отладки используйте `show-trace` после выполнения теста.
- Трейсы записываются автоматически для всех тестов.
- Трейсы располагаются в директории `.\artifacts\bin\RestoRate.E2ETests\debug\playwright-traces`

---
## Интеграционные тесты

Интеграционные тесты находятся в проектах `tests/<Context>/<Context>.IntegrationTests` (например, `tests/Restaurant/RestoRate.RestaurantService.IntegrationTests`). Они тестируют API endpoints с использованием WebApplicationFactory и требуют запущенных зависимостей (например, PostgreSQL, MongoDB), которые запускаются автоматически в docker.

### Запуск интеграционных тестов

```powershell
# Все интеграционные тесты
dotnet test tests/Restaurant/RestoRate.RestaurantService.IntegrationTests

# Конкретный тест
dotnet test tests/Restaurant/RestoRate.RestaurantService.IntegrationTests --filter "FullyQualifiedName~CreateRestaurant"
```

### Аутентификация в интеграционных тестах

Интеграционные тесты используют `TestAuthHandler` из `RestoRate.Testing.Common` для симуляции аутентифицированных пользователей без реального Keycloak.

**Ключевые компоненты:**

- **`TestAuthHandler`** (`tests/RestoRate.Testing.Common/Auth/TestAuthHandler.cs`): Кастомный обработчик аутентификации, создающий claims как в production JWT
  - Использует claim type `"roles"` (не `ClaimTypes.Role`), чтобы соответствовать production Keycloak JWT
  - Устанавливает `ClaimsIdentity` с `roleType: "roles"`, чтобы role-based authorization работала корректно
  - Конфигурирует `nameType: "preferred_username"` для соответствия Keycloak username claim

- **`TestUsers`** (`tests/RestoRate.Testing.Common/Auth/TestUsers.cs`): Предопределенные тестовые пользователи
  - `TestUser.Anonymous`: Без аутентификации
  - `TestUser.User`: Обычный пользователь с ролью `["user"]`
  - `TestUser.Admin`: Администратор с ролью `["admin"]`

- **`CreateClientWithUser()`** расширение (`tests/RestoRate.Testing.Common/WebApplicationFactoryExtensions.cs`):
  - Создает `HttpClient` с настроенной тестовой аутентификацией
  - Пример: `_client = _factory.CreateClientWithUser(TestUser.Admin);`
  - Автоматически инжектит `TestAuthHandler` с указанным пользователем в тестовый сервер

**Пример использования:**

```csharp
public class RestaurantApiTests : IClassFixture<RestaurantWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly HttpClient _adminClient;
    
    public RestaurantApiTests(RestaurantWebApplicationFactory factory)
    {
        // Клиент для обычного пользователя
        _client = factory.CreateClientWithUser(TestUser.User);
        // Клиент для администратора
        _adminClient = factory.CreateClientWithUser(TestUser.Admin);
    }
    
    [Fact]
    public async Task AdminEndpoint_WithAdminUser_Returns200()
    {
        var response = await _adminClient.GetAsync("/admin/endpoint");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
    
    [Fact]
    public async Task AdminEndpoint_WithRegularUser_Returns403()
    {
        var response = await _client.GetAsync("/admin/endpoint");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
```

**Важно:** Тестовая аутентификация полностью имитирует production поведение:
- Claims используют те же типы, что и Keycloak JWT (`"roles"`, `"preferred_username"`, `sub`, `email`)
- `IUserContext` работает идентично в тестах и production
- Role-based authorization policies (`[Authorize(Roles = "admin")]`) работают без изменений
- `HttpContextUserContext` читает claims из `HttpContext.User` так же, как в production

---
