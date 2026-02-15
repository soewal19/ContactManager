# Docker Multi-stage Build с поддержкой MS SQL Server и SQLite
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Установка инструментов для работы с SQL Server
RUN apt-get update && apt-get install -y \
    curl \
    apt-transport-https \
    software-properties-common \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl -fsSL https://packages.microsoft.com/config/debian/11/prod.list | tee /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev \
    && echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["ContactManager.Web/ContactManager.Web.csproj", "ContactManager.Web/"]
COPY ["ContactManager.Core/ContactManager.Core.csproj", "ContactManager.Core/"]
COPY ["ContactManager.Infrastructure/ContactManager.Infrastructure.csproj", "ContactManager.Infrastructure/"]
RUN dotnet restore "ContactManager.Web/ContactManager.Web.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/ContactManager.Web"
RUN dotnet build "ContactManager.Web.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "ContactManager.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage: build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Установка SQL Server Tools в runtime образ
RUN apt-get update && apt-get install -y \
    curl \
    apt-transport-https \
    software-properties-common \
    && curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add - \
    && curl -fsSL https://packages.microsoft.com/config/debian/11/prod.list | tee /etc/apt/sources.list.d/mssql-release.list \
    && apt-get update \
    && ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev \
    && echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> ~/.bashrc \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Установка SQLite (должен быть по умолчанию, но на всякий случай)
RUN apt-get update && apt-get install -y sqlite3 libsqlite3-dev && apt-get clean && rm -rf /var/lib/apt/lists/*

# Создание директорий для приложения и данных
RUN mkdir -p /app/data /app/logs /app/sql-scripts

WORKDIR /app

# Копирование опубликованного приложения
COPY --from=publish /app/publish .

# Копирование SQL скриптов инициализации
COPY ../sql-scripts/ /app/sql-scripts/

# Установка прав на директории
RUN chmod -R 755 /app/data /app/logs /app/sql-scripts

# Создание скрипта инициализации
RUN echo '#!/bin/bash\n\
echo "Starting Contact Manager Application..."\n\
echo "Database Configuration:"\n\
echo "Connection String: $ConnectionStrings__DefaultConnection"\n\
\n\
# Проверка подключения к SQL Server если используется\n\
if [[ "$ConnectionStrings__DefaultConnection" == *"Server=sqlserver"* ]]; then\n\
    echo "Waiting for SQL Server to be ready..."\n\
    until /opt/mssql-tools18/bin/sqlcmd -S sqlserver -U sa -P \"YourStrong@Passw0rd\" -Q \"SELECT 1\" -C; do\n\
        echo \"SQL Server is not ready yet...\"\n\
        sleep 5\n\
    done\n\
    echo \"SQL Server is ready!\"\n\
fi\n\
\n\
echo \"Starting ASP.NET Core application...\"\n\
exec dotnet ContactManager.Web.dll' > /app/entrypoint.sh && chmod +x /app/entrypoint.sh

# Expose ports
EXPOSE 80
EXPOSE 443

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:80/health || exit 1

# Entry point
ENTRYPOINT ["/app/entrypoint.sh"]