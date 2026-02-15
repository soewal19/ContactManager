# Configuration Architecture

## Overview

ContactManager.Web uses a centralized configuration system that organizes all application settings and service configurations into dedicated classes. This approach follows the **Single Responsibility Principle** and provides better maintainability and testability.

## Configuration Structure

```
ContactManager.Web/
├── Config/
│   ├── ApplicationConfiguration.cs      # Main configuration coordinator
│   ├── DatabaseConfiguration.cs       # Database setup and initialization
│   └── CsvConfigurationHelper.cs      # CSV parsing configurations
```

## ApplicationConfiguration

The `ApplicationConfiguration` class serves as the main entry point for all service configurations. It coordinates the setup of:

### Services Configured:

1. **MVC and Validation**
   - Controllers with Views
   - FluentValidation integration
   - Custom model binding messages

2. **Database Layer**
   - Automatic database type detection (SQLite/SQL Server)
   - Connection string validation
   - Database initialization

3. **Business Services**
   - CSV Service
   - Contact Service
   - Backend Service

4. **External Services**
   - Memory Caching
   - Response Caching
   - Security Headers

### Usage:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure all services
ApplicationConfiguration.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Configure HTTP pipeline
ApplicationConfiguration.ConfigurePipeline(app);
```

## DatabaseConfiguration

The `DatabaseConfiguration` class handles all database-related setup:

### Features:

1. **Automatic Database Detection**
   - SQLite: Detected by "Data Source=" in connection string
   - SQL Server: Detected by "Server=" and "Initial Catalog="

2. **Database-Specific Configuration**
   - SQLite: Optimized for file-based operations
   - SQL Server: Connection retry policies, timeout settings

3. **Initialization Strategies**
   - SQLite: Uses `EnsureCreated()` for development
   - SQL Server: Uses migrations for production

### Connection String Examples:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=ContactManager.db" // SQLite
    // OR
    "DefaultConnection": "Server=localhost;Database=ContactManager;Trusted_Connection=true;" // SQL Server
  }
}
```

## CsvConfigurationHelper

Provides standardized CSV parsing configurations for different scenarios:

### Available Configurations:

1. **Default Configuration**
   - Invariant culture for consistent parsing
   - Trim whitespace
   - Skip blank lines
   - Ignore missing fields

2. **Strict Configuration**
   - Throws exceptions on data quality issues
   - Validates headers and data types
   - Suitable for data import validation

3. **Bulk Configuration**
   - Optimized for large datasets
   - 64KB buffer size
   - Disabled byte counting for performance

4. **Culture-Specific**
   - Russian locale support
   - Configurable culture settings

### Usage Examples:

```csharp
// Default configuration
var config = CsvConfigurationHelper.GetDefaultConfiguration();

// Strict validation
var strictConfig = CsvConfigurationHelper.GetStrictConfiguration();

// Bulk operations
var bulkConfig = CsvConfigurationHelper.GetBulkConfiguration();

// Russian locale
var russianConfig = CsvConfigurationHelper.GetRussianConfiguration();
```

## Best Practices

### 1. Configuration Organization
- Keep related configurations together
- Use descriptive method names
- Document configuration purposes

### 2. Error Handling
- Validate connection strings before use
- Provide meaningful error messages
- Log configuration steps

### 3. Performance
- Use appropriate configurations for scenarios
- Enable caching where beneficial
- Configure timeouts and retry policies

### 4. Security
- Never hardcode connection strings
- Use environment-specific configurations
- Implement security headers

### 5. Testing
- Configuration classes are testable
- Mock dependencies for unit tests
- Test different database scenarios

## Migration Guide

### From Old Structure:

```csharp
// Old way - direct configuration in Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (connectionString.Contains("Data Source="))
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString);
});
```

### To New Structure:

```csharp
// New way - centralized configuration
ApplicationConfiguration.ConfigureServices(builder.Services, builder.Configuration);
```

## Troubleshooting

### Common Issues:

1. **Database Connection Failures**
   - Check connection string format
   - Verify database server is running
   - Check file permissions for SQLite

2. **CSV Parsing Errors**
   - Use appropriate configuration for your data
   - Check culture settings for numeric formats
   - Validate data before processing

3. **Performance Issues**
   - Use bulk configuration for large files
   - Enable response caching
   - Configure memory cache appropriately

## Environment-Specific Configuration

### Development:
- SQLite database for simplicity
- Detailed error pages
- Developer exception page

### Production:
- SQL Server for scalability
- Security headers enabled
- Optimized caching settings

## Future Enhancements

1. **Additional Database Support**
   - PostgreSQL
   - MySQL
   - Cosmos DB

2. **Configuration Validation**
   - Schema validation
   - Dependency validation
   - Performance validation

3. **Monitoring Integration**
   - Application Insights
   - Performance counters
   - Health checks