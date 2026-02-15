# C4 Model Documentation

This document provides a comprehensive overview of the Contact Manager Application architecture using the C4 model.

## 1. System Context Diagram (Level 1)
The highest level of abstraction, showing the application as a black box and its interactions with users and external systems.

```mermaid
C4Context
    title System Context Diagram for Contact Manager Application
    
    Person(user, "User", "A person who needs to manage their contact list (upload, view, edit, delete).")
    System(contactManager, "Contact Manager System", "Allows users to manage contacts via CSV upload and interactive web interface.")
    System_Ext(database, "SQL Database", "Persistent storage for contact records.")
    
    Rel(user, contactManager, "Manages contacts via", "HTTPS")
    Rel(contactManager, database, "Stores and retrieves data from", "EF Core / SQL")
```

## 2. Container Diagram (Level 2)
Shows the high-level technical building blocks of the system.

```mermaid
C4Container
    title Container Diagram for Contact Manager Application
    
    Person(user, "User", "A person who manages contacts.")
    
    System_Boundary(c1, "Contact Manager Application") {
        Container(web_app, "Web Application", "ASP.NET Core 8.0 MVC", "Serves HTML, handles user input, and coordinates business logic.")
        Container(logic_lib, "Core & Infrastructure Libraries", ".NET 8.0 DLLs", "Contains business rules, CSV parsing logic, and data access code.")
    }
    
    ContainerDb(db, "Relational Database", "SQLite / MS SQL Server", "Stores contact information in a structured format.")
    
    Rel(user, web_app, "Uses", "HTTPS/Browser")
    Rel(web_app, logic_lib, "Calls", "In-process")
    Rel(logic_lib, db, "Reads from and writes to", "Entity Framework Core")
```

## 3. Component Diagram (Level 3)
Decomposes the containers into their constituent components.

```mermaid
C4Component
    title Component Diagram for Web Application & Libraries
    
    Container_Boundary(web, "Web Application Layer") {
        Component(controller, "ContactsController", "MVC Controller", "Handles HTTP requests for uploading, listing, updating, and deleting contacts.")
        Component(view, "Contacts View", "Razor View + jQuery/DataTables", "Provides the interactive UI for filtering, sorting, and inline editing.")
    }
    
    Container_Boundary(infra, "Infrastructure & Core Layer") {
        Component(contact_service, "ContactService", "Service", "Manages CRUD operations and interacts with the DB Context.")
        Component(csv_service, "CsvService", "Service", "Parses CSV streams into domain models using CsvHelper.")
        Component(validator, "ContactValidator", "FluentValidation", "Enforces business rules on contact data.")
        Component(db_context, "ApplicationDbContext", "EF Core Context", "Maps domain models to database tables.")
    }
    
    ContainerDb(db, "Database", "SQLite/SQL Server", "Physical storage.")
    
    Rel(controller, view, "Renders")
    Rel(controller, contact_service, "Uses")
    Rel(controller, csv_service, "Uses")
    Rel(controller, validator, "Uses")
    Rel(contact_service, db_context, "Uses")
    Rel(db_context, db, "SQL/Command")
```

## 4. Deployment Diagram (Level 4)
Shows how the system is mapped to infrastructure.

```mermaid
deploymentDiagram
    title Deployment Diagram - Dockerized Environment
    
    Node(client_comp, "Client Computer", "Web Browser") {
        Container(browser, "Chrome/Firefox/Edge", "User Interface")
    }
    
    Node(docker_host, "Docker Host", "Linux/Windows with Docker") {
        Node(app_container, "App Container", "Docker (Alpine .NET Runtime)") {
            Container(app_exec, "ContactManager.Web.dll", "Kestrel Server")
        }
        Node(db_node, "Database Node", "SQLite File or SQL Container") {
            ContainerDb(physical_db, "ContactManager.db", "Data Storage")
        }
    }
    
    Rel(browser, app_exec, "HTTPS (Port 80/443)", "TCP/IP")
    Rel(app_exec, physical_db, "File System / Network", "EF Core")

---

**For a deeper dive into the code-level implementation, data flows, and component interactions, please refer to the [Detailed Design Documentation](Detailed_Design.md).**
```
