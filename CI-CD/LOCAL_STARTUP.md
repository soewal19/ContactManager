# Contact Manager - Local Development Guide
# Менеджер Контактов - Руководство по локальной разработке

---

## 🇺🇸 English | 🇷🇺 Русский

---

## 🚀 Quick Start / Быстрый старт

### Windows

**Option 1: Double-click (Easiest) / Вариант 1: Двойной клик (Самый простой)**
- 🇺🇸 Double-click `start-local.bat` in the `CI-CD/` folder
- 🇷🇺 Двойной клик по `start-local.bat` в папке `CI-CD/`

**Option 2: PowerShell / Вариант 2: PowerShell**
```powershell
.\CI-CD\start-local.ps1
```

**Option 3: Command Prompt / Вариант 3: Командная строка**
```cmd
cd CI-CD
start-local.bat
```

### macOS / Linux

```bash
cd CI-CD
chmod +x start-local.sh
./start-local.sh
```

## 🛠️ Manual Start (Advanced) / Ручной запуск (Для продвинутых)

**English:**
```bash
# Navigate to project root
cd ContactManager.Web

# Restore packages
dotnet restore

# Build
dotnet build

# Run
dotnet run
```

**Русский:**
```bash
# Перейти в корень проекта
cd ContactManager.Web

# Восстановить пакеты
dotnet restore

# Собрать
dotnet build

# Запустить
dotnet run
```

## Prerequisites / Требования

| English | Русский |
|---------|---------|
| [.NET 8 SDK](https://dotnet.microsoft.com/download) (or higher) | [.NET 8 SDK](https://dotnet.microsoft.com/download) (или выше) |
| Any modern web browser | Любой современный веб-браузер |

## 🔧 What Happens When You Run / Что происходит при запуске

| Step / Шаг | English | Русский |
|------------|---------|---------|
| 1 | **Restore** - Downloads all required NuGet packages | **Restore** - Загрузка всех необходимых NuGet пакетов |
| 2 | **Build** - Compiles the application | **Build** - Компиляция приложения |
| 3 | **Database** - Creates SQLite database automatically (first run) | **Database** - Автоматическое создание базы данных SQLite (первый запуск) |
| 4 | **Server** - Starts Kestrel web server on port 5021 | **Server** - Запуск веб-сервера Kestrel на порту 5021 |
| 5 | **Browser** - Opens `http://localhost:5021` | **Browser** - Открытие `http://localhost:5021` |

## ⚙️ Default Behavior / Поведение по умолчанию

| Parameter / Параметр | Value / Значение |
|---------------------|------------------|
| Port / Порт | 5021 |
| Database / База данных | SQLite (auto-created / автосоздание) |
| Profile / Профиль | HTTP (not HTTPS, no certificate required / не HTTPS, сертификат не требуется) |
| Logging / Логирование | Minimal (quiet mode / минимальное) |

## 🐛 Troubleshooting / Устранение неполадок

### Port 5021 is already in use / Порт 5021 уже занят

**English:**
Change port in `ContactManager.Web/Properties/launchSettings.json` or run:
```bash
dotnet run --urls "http://localhost:5001"
```

**Русский:**
Измените порт в `ContactManager.Web/Properties/launchSettings.json` или запустите:
```bash
dotnet run --urls "http://localhost:5001"
```

### .NET SDK not found / .NET SDK не найден

**English:**  
Install from: https://dotnet.microsoft.com/download

**Русский:**  
Установите с: https://dotnet.microsoft.com/download

### Build errors / Ошибки сборки

**English:**
```bash
dotnet clean
dotnet restore
dotnet build
```

**Русский:**
```bash
dotnet clean
dotnet restore
dotnet build
```

### Database locked / База данных заблокирована

**English:**
Delete the SQLite files and restart:
```bash
del ContactManager.db*
dotnet run
```

**Русский:**
Удалите файлы SQLite и перезапустите:
```bash
del ContactManager.db*
dotnet run
```

## ✨ Features Available / Доступные функции

| English | Русский |
|---------|---------|
| Upload CSV files with contacts | Загрузка CSV файлов с контактами |
| View all contacts in DataTable | Просмотр всех контактов в DataTable |
| Filter and sort by any column | Фильтрация и сортировка по любой колонке |
| Edit contacts inline | Редактирование контактов в строке |
| Delete contacts | Удаление контактов |
| Export data | Экспорт данных |
| View API documentation at `/Documentation` | Просмотр API документации по `/Documentation` |

## ⏹️ Stop Application / Остановка приложения

**English:** Press `Ctrl+C` in the terminal window where the server is running.  
**Русский:** Нажмите `Ctrl+C` в терминале, где работает сервер.

## 🐳 Alternative: Docker / Альтернатива: Docker

**English:**  
For containerized run with MS SQL Server, see [DOCKER_MSSQL_GUIDE.md](DOCKER_MSSQL_GUIDE.md)

**Русский:**  
Для запуска в контейнерах с MS SQL Server, см. [DOCKER_MSSQL_GUIDE.md](DOCKER_MSSQL_GUIDE.md)

---

## 📂 Available Scripts / Доступные скрипты

| File / Файл | Description / Описание | Platform / Платформа |
|-------------|------------------------|---------------------|
| `start-local.bat` | Windows batch script | Windows |
| `start-local.ps1` | PowerShell script | Windows (PowerShell) |
| `start-local.sh` | Bash script | Linux, macOS |

---

## 🔗 Useful Links / Полезные ссылки

- **Repository / Репозиторий:** https://github.com/soewal19/ContactManager
- **Local URL / Локальный URL:** http://localhost:5021
- **API Docs / API Документация:** http://localhost:5021/Documentation

---

**Last Updated / Последнее обновление:** February 15, 2026 / 15 февраля 2026 г.

**Version / Версия:** 1.0.0
