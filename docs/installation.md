# Инструкция по запуску Infinitea

### Перед запуском проекта необходимо установить:

- [Git](https://git-scm.com/) — для клонирования репозитория.
- [Docker](https://www.docker.com/) и Docker Compose — для запуска микросервисов и баз данных.
- [Node.js](https://nodejs.org/) и npm — для запуска frontend-приложения.

### Открыть терминал и выполнить команды:

```bash
git clone <REPOSITORY_URL>
cd Infinitea
```

В корневой директории проекта создайте файл `.env` на основе примера:

```bash
cp .env.example .env
```
Открыть `.env` и заменить демонстрационные значения паролей и секретов на собственные.

### Запуск Backend

Находясь в корневой директории проекта, выполнить:

```bash
docker compose up -d --build
```
Для проверки состояния контейнеров можно выполнить:

```bash
docker compose ps
```

Все основные сервисы должны находиться в состоянии `Up` или `running`.

### Запуск Frontend

Открыть второй терминал и перейти в директорию Frontend:

```bash
cd Infinitea/Frontend
```

Установить зависимости:

```bash
npm install
```

Запустить приложение:

```bash
npm run dev
```

После запуска frontend будет доступен по адресу:

http://localhost:5173

### Настройка и авторизация Keycloak

После первого запуска Keycloak импортирует конфигурацию Realm `infinitea` из файла:

`keycloak/realm-export.json`

Административная панель Keycloak доступна по адресу:

http://localhost:8083

Для входа используются учётные данные администратора, указанные в `.env`.

После импорта необходимо создать тестового пользователя через административную панель Keycloak.

### Адреса компонентов системы

| Компонент | Адрес |
|---|---|
| Frontend | http://localhost:5173 |
| ProductsService | http://localhost:8080 |
| UsersService | http://localhost:8081 |
| OrdersService | http://localhost:8082 |
| Keycloak | http://localhost:8083 |
| API Gateway / Swagger UI | http://localhost:8084 |

Swagger UI через API Gateway предоставляет доступ к документации трёх API: ProductsService, UsersService и OrdersService.

### Остановка проекта

Для остановки контейнеров выполнить в корневой директории:

```bash
docker compose down
```
и прервать соединение с Frontend в соответствующей вкладке терминала с помощью Ctrl + C
