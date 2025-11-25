# Тестирование в RestoRate

## Назначение RestoRate.E2ETests

`RestoRate.E2ETests` — проект для end-to-end (E2E) тестов, покрывающих взаимодействие сервисов, инфраструктуры и UI. Использует [Playwright](https://playwright.dev/dotnet/) для E2E тестирования, включая сценарии с аутентификацией.

## Быстрый старт

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


## Создание E2E теста

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

## Примечания


- Все тесты предполагают, что сервисы подняты через Aspire и доступны для взаимодействия.
- Для корректной работы Playwright и браузеров используйте скрипт установки перед первым запуском.
- Для отладки используйте `show-trace` после выполнения теста.
- Трейсы записываются автоматически для всех тестов.
- Трейсы располагаются в директории `.\artifacts\bin\RestoRate.E2ETests\debug\playwright-traces`

---
