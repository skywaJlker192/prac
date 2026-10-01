2. Запуск
Один раз выполни dotnet dev-certs https --trust, чтобы HTTPS работал без ошибок сертификата.
В папке проекта выполни:
   dotnet restore
   dotnet run

4. Открой в браузере https://localhost:7001/swagger.

База taskflow.db создастся сама при первом запуске, уже с seed-данными: проект №1, задача №1 и пользователь admin с Id 1.

3. Где что лежит

Controllers/ — эндпоинты: проекты, задачи v1 и v2, комментарии.
DTOs/ — модели запросов и ответов.
Entities/ и Data/AppDbContext.cs — сущности, связи, индекс, seed.
Middleware/IdempotencyMiddleware.cs — защита от дублей (задание 3.2).
Exceptions/ — исключения и глобальный обработчик ошибок.
Mapping/ — маппинг Entity в DTO.
Program.cs — вся настройка: Swagger, версии, CORS, обработка ошибок.
TaskFlowApi.http — готовые запросы для проверки.

4. Как проверять

Через .http: открой TaskFlowApi.http в VS, Rider или VS Code (нужно расширение REST Client) и жми Send над каждым запросом. Идти надо сверху вниз, потому что запросы зависят друг от друга. Например, 422 на PATCH tasks/1/status сработает только после удаления комментария.
Через Swagger: в выпадающем списке сверху переключай версии v1 и v2. У v1 будет пометка deprecated.

5. Адреса
Все маршруты с версией: /api/v1/projects, /api/v1/tasks, /api/v2/tasks, /api/v1/tasks/{id}/comments. Без версии (/api/projects) не работает. Для POST на /api/v2/tasks нужен заголовок X-Idempotency-Key с любым GUID.

6. Миграции
Останови приложение и удали taskflow.db.
Выполни dotnet ef migrations add Init. Если команды dotnet ef нет, сначала dotnet tool install --global dotnet-ef.
Запусти приложение снова. Теперь оно само применит миграцию.
