# ASKART

ASKART — проект корпоративного мессенджера с личными и групповыми чатами, сообщениями и задачами. Ветка `main` содержит ASP.NET Core Web API на C# и материалы проекта. SQL-дамп схемы хранится отдельно, в ветке `database`.

## Возможности API

- регистрация и вход пользователей;
- создание личных и групповых чатов;
- просмотр чатов пользователя и управление участниками;
- отправка, просмотр и редактирование сообщений, в том числе ответ на сообщение.

Основные маршруты начинаются с `/api`: `Users`, `Chats`, `ChatMembers` и `Messages`. В режиме разработки доступна Swagger UI.

## Требования

- .NET SDK 8;
- MySQL Server 8;
- база данных `askart` со схемой из `project_bd.sql` в ветке [`database`](https://github.com/Virusanchik626/ASKART/tree/database).

## Установка и запуск

Из корня репозитория создайте базу и загрузите схему из ветки `database`:

```bash
git fetch origin database
git show origin/database:project_bd.sql > project_bd.sql
mysql -u root -p -e "CREATE DATABASE IF NOT EXISTS askart CHARACTER SET utf8mb4;"
mysql -u root -p askart < project_bd.sql
```

Перед запуском проверьте подключение к MySQL в `ASKART.API — копия/ASKART.API/Program.cs`: сейчас в строке подключения заданы `localhost`, база `askart`, пользователь `root` и пароль `1234`. Укажите свои параметры локально.

Затем из корня репозитория выполните:

```bash
dotnet restore "ASKART.API — копия/ASKART.API/ASKART.API.csproj"
dotnet run --project "ASKART.API — копия/ASKART.API/ASKART.API.csproj"
```

Профиль `http` поднимает API на `http://localhost:5025`; Swagger UI — по адресу `http://localhost:5025/swagger`.

> Скрипт `project_bd.sql` содержит `DROP TABLE`. Повторный импорт удаляет и пересоздаёт таблицы, поэтому сначала сохраните нужные данные.

## Структура

- `ASKART.API — копия/ASKART.API/Controllers/` — обработчики пользователей, чатов, участников и сообщений;
- `DTOs/` — модели запросов API;
- `Models/` — сущности базы данных;
- `Data/AskartContext.cs` — контекст Entity Framework Core;
- `Program.cs` — регистрация сервисов, MySQL и Swagger;
- SQL-дамп схемы `project_bd.sql` — в ветке `database`;
- `kanban` — текстовый файл со ссылкой на доску YouGile;
- `Описание предметной области_ASKART.docx` — описание предметной области.

## Связанные материалы

- [Макет в Figma](https://www.figma.com/design/p2o391S8R6sbY3Z3iAMrEx/ASKART?node-id=0-1&t=VyHZBgP0jgwvzxtl-1)
- [Доска ASKART в YouGile](https://ru.yougile.com/team/72028b9b085a/ASKART#ASK-3)
