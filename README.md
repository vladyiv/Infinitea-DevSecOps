# Учебный проект по теме "Мини-магазин чая Infinitea"
## Содержание

- [Frontend](Frontend/) — пользовательский интерфейс на Vue.js
  - [Исходный код](Frontend/src/)
  - [Главный компонент](Frontend/src/App.vue)
  - [Авторизация](Frontend/src/auth.js)
  - [Работа с API](Frontend/src/useInfinitea.js)
  - [Стили](Frontend/src/assets/)

- **Backend**
  - [ProductsService](ProductsService/) — управление каталогом товаров
  - [UsersService](UsersService/) — управление пользователями и профилями
  - [OrdersService](OrdersService/) — корзина и оформление заказов
  - [GatewayService](GatewayService/) — API Gateway и маршрутизация запросов

- **Инфраструктура**
  - [Docker Compose](docker-compose.yml) — конфигурация и запуск контейнеров
  - [Keycloak](keycloak/) — настройки авторизации
  - [Экспорт Realm](keycloak/realm-export.json)
  - [Пример переменных окружения](.env.example)
  - [Git ignore](.gitignore)

- **Документация**
  - [Архитектура C4](docs/architecture.png)
  - [Инструкция по запуску](docs/installation.md)
  - [План API](api-plan.md)
  - [Отчёт по Docker](docker-report.md)
  - [Отчёт по Docker Compose](docker-compose-report.md)

- [Решение .NET](Infinitea.sln)

### Коммиты осуществляются в пяти ветках
- main – основная ветка;
- develop — ветка для сборки проекта;
- feature/readme — ветка для редактирования файлов README и project-notes
- feature/api-plan — ветка для редактирования файла api-plan;
- fix/conflict-demo — ветка для демонстрации конфликта;

## Основная информация

### Описание системы
API-приложение "Мини-магазин чая Infnintea" с базой данных на основе образа PostreSQL. В качестве графической оболочки выступает веб-сайт на Vue.js, доступный в браузере после запуска docker-container (инструкция по запуску указанав содержании).

### Схема архитектуры
Указана в содержании

### Список сервисов
Указан в содержании (План API)
