-- Create ContactManager database if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ContactManager')
BEGIN
    CREATE DATABASE ContactManager;
END
GO

USE ContactManager;
GO

-- Create Contacts table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Contacts] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(255) NULL,
        [Phone] NVARCHAR(20) NULL,
        [DateOfBirth] DATE NULL,
        [Address] NVARCHAR(500) NULL,
        [MaritalStatus] NVARCHAR(20) NULL,
        [Salary] DECIMAL(18,2) NULL,
        [Notes] NVARCHAR(MAX) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL
    );
END
GO

-- Create indexes for better performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND name = N'IX_Contacts_Email')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Contacts_Email] ON [dbo].[Contacts] ([Email]) WHERE [Email] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND name = N'IX_Contacts_LastName_FirstName')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Contacts_LastName_FirstName] ON [dbo].[Contacts] ([LastName], [FirstName]);
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND name = N'IX_Contacts_IsDeleted')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Contacts_IsDeleted] ON [dbo].[Contacts] ([IsDeleted]);
END
GO

-- Create stored procedures for common operations

-- Insert Contact
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Contact_Insert]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_Contact_Insert]
        @FirstName NVARCHAR(100),
        @LastName NVARCHAR(100),
        @Email NVARCHAR(255) = NULL,
        @Phone NVARCHAR(20) = NULL,
        @DateOfBirth DATE = NULL,
        @Address NVARCHAR(500) = NULL,
        @MaritalStatus NVARCHAR(20) = NULL,
        @Salary DECIMAL(18,2) = NULL,
        @Notes NVARCHAR(MAX) = NULL
    AS
    BEGIN
        INSERT INTO [dbo].[Contacts] ([FirstName], [LastName], [Email], [Phone], [DateOfBirth], [Address], [MaritalStatus], [Salary], [Notes])
        VALUES (@FirstName, @LastName, @Email, @Phone, @DateOfBirth, @Address, @MaritalStatus, @Salary, @Notes);
        
        SELECT SCOPE_IDENTITY() as Id;
    END');
END
GO

-- Update Contact
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Contact_Update]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_Contact_Update]
        @Id INT,
        @FirstName NVARCHAR(100),
        @LastName NVARCHAR(100),
        @Email NVARCHAR(255) = NULL,
        @Phone NVARCHAR(20) = NULL,
        @DateOfBirth DATE = NULL,
        @Address NVARCHAR(500) = NULL,
        @MaritalStatus NVARCHAR(20) = NULL,
        @Salary DECIMAL(18,2) = NULL,
        @Notes NVARCHAR(MAX) = NULL
    AS
    BEGIN
        UPDATE [dbo].[Contacts]
        SET [FirstName] = @FirstName,
            [LastName] = @LastName,
            [Email] = @Email,
            [Phone] = @Phone,
            [DateOfBirth] = @DateOfBirth,
            [Address] = @Address,
            [MaritalStatus] = @MaritalStatus,
            [Salary] = @Salary,
            [Notes] = @Notes,
            [UpdatedAt] = GETUTCDATE()
        WHERE [Id] = @Id AND [IsDeleted] = 0;
    END');
END
GO

-- Soft Delete Contact
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Contact_SoftDelete]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_Contact_SoftDelete]
        @Id INT
    AS
    BEGIN
        UPDATE [dbo].[Contacts]
        SET [IsDeleted] = 1,
            [DeletedAt] = GETUTCDATE()
        WHERE [Id] = @Id AND [IsDeleted] = 0;
    END');
END
GO

-- Get All Active Contacts with Pagination
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_Contact_GetAll]') AND type in (N'P', N'PC'))
BEGIN
    EXEC('CREATE PROCEDURE [dbo].[sp_Contact_GetAll]
        @PageNumber INT = 1,
        @PageSize INT = 10,
        @SearchTerm NVARCHAR(100) = NULL,
        @SortBy NVARCHAR(50) = ''LastName'',
        @SortDirection NVARCHAR(4) = ''ASC''
    AS
    BEGIN
        DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
        
        WITH FilteredContacts AS (
            SELECT * FROM [dbo].[Contacts]
            WHERE [IsDeleted] = 0
                AND (@SearchTerm IS NULL OR 
                    [FirstName] LIKE ''%'' + @SearchTerm + ''%'' OR
                    [LastName] LIKE ''%'' + @SearchTerm + ''%'' OR
                    [Email] LIKE ''%'' + @SearchTerm + ''%'' OR
                    [Phone] LIKE ''%'' + @SearchTerm + ''%'')
        )
        SELECT 
            [Id], [FirstName], [LastName], [Email], [Phone], [DateOfBirth], 
            [Address], [MaritalStatus], [Salary], [Notes], [CreatedAt], [UpdatedAt]
        FROM FilteredContacts
        ORDER BY 
            CASE WHEN @SortDirection = ''ASC'' AND @SortBy = ''FirstName'' THEN [FirstName] END ASC,
            CASE WHEN @SortDirection = ''DESC'' AND @SortBy = ''FirstName'' THEN [FirstName] END DESC,
            CASE WHEN @SortDirection = ''ASC'' AND @SortBy = ''LastName'' THEN [LastName] END ASC,
            CASE WHEN @SortDirection = ''DESC'' AND @SortBy = ''LastName'' THEN [LastName] END DESC,
            CASE WHEN @SortDirection = ''ASC'' AND @SortBy = ''Email'' THEN [Email] END ASC,
            CASE WHEN @SortDirection = ''DESC'' AND @SortBy = ''Email'' THEN [Email] END DESC,
            CASE WHEN @SortDirection = ''ASC'' AND @SortBy = ''CreatedAt'' THEN [CreatedAt] END ASC,
            CASE WHEN @SortDirection = ''DESC'' AND @SortBy = ''CreatedAt'' THEN [CreatedAt] END DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        
        SELECT COUNT(*) as TotalCount FROM FilteredContacts;
    END');
END
GO

-- Insert sample data
IF NOT EXISTS (SELECT * FROM [dbo].[Contacts])
BEGIN
    INSERT INTO [dbo].[Contacts] ([FirstName], [LastName], [Email], [Phone], [DateOfBirth], [Address], [MaritalStatus], [Salary], [Notes])
    VALUES 
    ('John', 'Doe', 'john.doe@email.com', '+1-555-0101', '1985-03-15', '123 Main St, New York, NY 10001', 'Single', 75000.00, 'Software Engineer'),
    ('Jane', 'Smith', 'jane.smith@email.com', '+1-555-0102', '1990-07-22', '456 Oak Ave, Los Angeles, CA 90001', 'Married', 85000.00, 'Product Manager'),
    ('Robert', 'Johnson', 'robert.j@email.com', '+1-555-0103', '1982-11-08', '789 Pine Rd, Chicago, IL 60601', 'Single', 68000.00, 'Data Analyst'),
    ('Maria', 'Garcia', 'maria.garcia@email.com', '+1-555-0104', '1988-05-12', '321 Elm St, Houston, TX 77001', 'Married', 72000.00, 'UX Designer'),
    ('David', 'Wilson', 'david.wilson@email.com', '+1-555-0105', '1979-09-30', '654 Maple Dr, Phoenix, AZ 85001', 'Divorced', 95000.00, 'Senior Developer');
END
GO

PRINT 'Database initialization completed successfully!';