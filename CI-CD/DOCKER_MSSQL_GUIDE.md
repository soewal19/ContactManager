# Docker Configuration с поддержкой MS SQL Server

## Быстрый старт

### 1. Запуск с SQLite (по умолчанию)
```bash
docker-compose up -d
```

### 2. Запуск с MS SQL Server
```bash
# Запуск с SQL Server
docker-compose up -d sqlserver web

# Или с дополнительными сервисами
docker-compose --profile management --profile cache up -d
```

## Конфигурация баз данных

### SQLite (по умолчанию)
- Файл базы данных: `/app/data/ContactManager.db`
- Настройка: `Data Source=/app/data/ContactManager.db`
- Идеально для разработки и тестирования

### MS SQL Server
- Контейнер: `contactmanager-sqlserver`
- Порт: `1433`
- Пользователь: `sa`
- Пароль: `YourStrong@Passw0rd` (измените в продакшене!)
- Строка подключения:
```
Server=sqlserver,1433;Database=ContactManager;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true
```

## Профили Docker Compose

### Доступные профили:
- **management**: SQL Server Management Studio (порт 1434)
- **cache**: Redis для кэширования (порт 6379)
- **production**: Nginx reverse proxy с SSL (порты 80/443)

### Использование профилей:
```bash
# Запуск с конкретными профилями
docker-compose --profile management --profile cache up -d

# Запуск всех профилей
docker-compose --profile management --profile cache --profile production up -d
```

## Команды управления

### Запуск и остановка
```bash
# Запуск
docker-compose up -d

# Остановка
docker-compose down

# Остановка с удалением volumes
docker-compose down -v

# Перезапуск
docker-compose restart
```

### Просмотр логов
```bash
# Все логи
docker-compose logs

# Логи конкретного сервиса
docker-compose logs web
docker-compose logs sqlserver

# Просмотр логов в реальном времени
docker-compose logs -f web
```

### Проверка статуса
```bash
docker-compose ps
docker-compose top
```

## Работа с MS SQL Server

### Подключение к SQL Server
```bash
# Подключение через командную строку
docker exec -it contactmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd

# Подключение через Management Studio (если включен профиль)
# Сервер: localhost,1434
# Пользователь: sa
# Пароль: YourStrong@Passw0rd
```

### Выполнение SQL скриптов
```bash
# Копирование скрипта в контейнер
docker cp your-script.sql contactmanager-sqlserver:/tmp/

# Выполнение скрипта
docker exec -it contactmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -i /tmp/your-script.sql
```

## Безопасность

### Продакшен рекомендации:
1. **Смените пароль SA**:
   ```bash
   docker exec -it contactmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "ALTER LOGIN sa WITH PASSWORD='YourNewStrong@Passw0rd'"
   ```

2. **Используйте SSL сертификаты**:
   - Поместите сертификаты в папку `./ssl/`
   - Обновите `nginx.conf` при необходимости

3. **Ограничьте доступ к портам**:
   - В продакшене откройте только необходимые порты
   - Используйте firewall и security groups

4. **Регулярные обновления**:
   ```bash
   docker-compose pull
docker-compose up -d
   ```

## Мониторинг и диагностика

### Проверка healthcheck
```bash
docker-compose ps
# Смотрите столбец STATUS
```

### Проверка ресурсов
```bash
docker stats
```

### Диагностика SQL Server
```bash
# Проверка статуса
docker exec contactmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "SELECT @@VERSION"

# Просмотр логов SQL Server
docker exec contactmanager-sqlserver cat /var/opt/mssql/log/errorlog
```

## Резервное копирование и восстановление

### Резервное копирование SQL Server
```bash
# Создание резервной копии
docker exec contactmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "BACKUP DATABASE [ContactManager] TO DISK = '/var/opt/mssql/backup/ContactManager.bak'"

# Копирование резервной копии на хост
docker cp contactmanager-sqlserver:/var/opt/mssql/backup/ContactManager.bak ./backups/
```

### Восстановление SQL Server
```bash
# Копирование резервной копии в контейнер
docker cp ./backups/ContactManager.bak contactmanager-sqlserver:/var/opt/mssql/backup/

# Восстановление
docker exec contactmanager-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "RESTORE DATABASE [ContactManager] FROM DISK = '/var/opt/mssql/backup/ContactManager.bak'"
```

## Переменные окружения

### Основные переменные:
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: Строка подключения к БД
- `ASPNETCORE_URLS`: URL для приложения

### Переменные SQL Server:
- `ACCEPT_EULA`: Принятие лицензии (Y/N)
- `MSSQL_SA_PASSWORD`: Пароль SA пользователя
- `MSSQL_PID`: Версия (Developer/Express/Standard/Enterprise)
- `MSSQL_COLLATION`: Кодировка

## Проблемы и решения

### SQL Server не запускается
```bash
# Проверка требований к памяти
docker exec contactmanager-sqlserver cat /var/opt/mssql/log/errorlog | grep memory

# Минимум 2GB RAM требуется для SQL Server
```

### Проблемы с подключением
```bash
# Проверка сети между контейнерами
docker exec web ping sqlserver

# Проверка порта SQL Server
docker exec web telnet sqlserver 1433
```

### Очистка данных
```bash
# Удаление всех данных и volumes
docker-compose down -v
docker system prune -f
```