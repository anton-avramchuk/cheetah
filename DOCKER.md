# Docker Setup для Cheetah CRM

## Архитектура

Docker Compose включает следующие сервисы:

- **PostgreSQL** - основная база данных (порт 5432)
- **pgAdmin** - веб-интерфейс для управления PostgreSQL (порт 5050)
- **API** - Cheetah CRM бэкенд (внутренний порт 8080)
- **Nginx** - реверс-прокси для API (порты 80, 443)

## Быстрый старт

### 1. Запуск всех сервисов

```bash
docker compose up -d
```

### 2. Просмотр логов

```bash
# Все сервисы
docker compose logs -f

# Конкретный сервис
docker compose logs -f api
docker compose logs -f nginx
docker compose logs -f postgres
```

### 3. Остановка сервисов

```bash
docker compose down
```

### 4. Остановка с удалением volumes (БД будет очищена!)

```bash
docker compose down -v
```

## Доступ к сервисам

### API (через Nginx)
- **API Endpoints**: http://localhost/api/
- **Swagger UI**: http://localhost/swagger
- **Scalar Docs**: http://localhost/scalar
- **Health Check**: http://localhost/health

### Прямой доступ к API (без Nginx)
- API работает на порту 8080 внутри Docker сети
- Прямой доступ снаружи не открыт (используйте Nginx)

### PostgreSQL
- **Host**: localhost
- **Port**: 5432
- **Database**: cheetah_master
- **Username**: cheetah
- **Password**: cheetah123

**Connection String для приложения:**
```
Host=postgres;Port=5432;Database=cheetah_master;Username=cheetah;Password=cheetah123
```

### pgAdmin
- **URL**: http://localhost:5050
- **Email**: admin@cheetah.local
- **Password**: admin123

**Как подключиться к PostgreSQL из pgAdmin:**
1. Открыть http://localhost:5050
2. Войти с указанными выше credentials
3. Добавить новый сервер:
   - Name: Cheetah PostgreSQL
   - Host: postgres (имя контейнера в Docker сети)
   - Port: 5432
   - Username: cheetah
   - Password: cheetah123

## Полезные команды

### Пересборка образа API

```bash
docker compose build api
docker compose up -d api
```

### Пересборка всего с нуля

```bash
docker compose down -v
docker compose build --no-cache
docker compose up -d
```

### Выполнение миграций

```bash
docker compose exec api dotnet ef database update --project /app/Cheetah.Tenants.DataAccess.dll
```

### Подключение к PostgreSQL через CLI

```bash
docker compose exec postgres psql -U cheetah -d cheetah_master
```

### Просмотр статуса контейнеров

```bash
docker compose ps
```

### Перезапуск конкретного сервиса

```bash
docker compose restart api
docker compose restart nginx
```

## Переменные окружения

Вы можете переопределить переменные окружения, создав файл `.env` в корне проекта:

```env
# PostgreSQL
POSTGRES_USER=cheetah
POSTGRES_PASSWORD=cheetah123
POSTGRES_DB=cheetah_master

# pgAdmin
PGADMIN_EMAIL=admin@cheetah.local
PGADMIN_PASSWORD=admin123

# API
ASPNETCORE_ENVIRONMENT=Development
```

## Volumes (Персистентность данных)

Docker Compose создает следующие volumes:

- `postgres_data` - данные PostgreSQL
- `pgadmin_data` - конфигурация pgAdmin

Данные сохраняются между перезапусками контейнеров.

## Сеть

Все сервисы работают в изолированной Docker сети `cheetah-network`.

## Troubleshooting

### API не запускается

```bash
# Проверить логи
docker compose logs api

# Проверить что PostgreSQL готова
docker compose exec postgres pg_isready -U cheetah
```

### Nginx возвращает 502 Bad Gateway

```bash
# Проверить что API запущен
docker compose ps api

# Проверить логи Nginx
docker compose logs nginx

# Проверить логи API
docker compose logs api
```

### Ошибка подключения к базе данных

Убедитесь, что:
1. PostgreSQL контейнер запущен: `docker compose ps postgres`
2. Healthcheck прошел: `docker compose exec postgres pg_isready -U cheetah`
3. Connection string правильный (используйте имя контейнера `postgres`, а не `localhost`)

### Очистка всех Docker ресурсов

```bash
# ВНИМАНИЕ: Удалит ВСЕ данные!
docker compose down -v
docker system prune -a --volumes
```

## Production настройки

Для production окружения:

1. Измените пароли в `compose.yaml`
2. Настройте SSL сертификаты для Nginx
3. Используйте secrets вместо environment переменных
4. Настройте резервное копирование PostgreSQL
5. Настройте мониторинг (Prometheus, Grafana)
6. Используйте `ASPNETCORE_ENVIRONMENT=Production`

## Структура файлов

```
cheetah/
├── compose.yaml          # Docker Compose конфигурация
├── nginx.conf            # Nginx конфигурация
├── .dockerignore         # Файлы, игнорируемые Docker
├── src/
│   └── Cheetah.Crm/
│       └── Dockerfile    # Dockerfile для API
└── DOCKER.md            # Эта документация
```
