USE master

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'CIMDatabase')
BEGIN
    CREATE DATABASE CIMDatabase;
END
GO

USE CIMDatabase;
GO


-- Customers
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Customers') AND type in (N'U'))
BEGIN
CREATE TABLE Customers (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);
END
GO

-- Invoices
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Invoices') AND type in (N'U'))
BEGIN
CREATE TABLE Invoices (
    InvoiceId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceNumber NVARCHAR(50) NOT NULL UNIQUE,
    CustomerId INT NOT NULL,
    InvoiceDate DATE NOT NULL,
    DueDate DATE NOT NULL,
    Terms INT NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);
END
GO

-- InvoiceLines
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'InvoiceLines') AND type in (N'U'))
BEGIN
CREATE TABLE InvoiceLines (
    LineId INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId INT NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId)
);
END
GO


-- STORED PROCEDURES (CUSTOMER)


IF OBJECT_ID('usp_CreateCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_CreateCustomer;
GO
CREATE PROCEDURE usp_CreateCustomer
    @Name NVARCHAR(100),
    @Address NVARCHAR(255),
    @PhoneNumber NVARCHAR(20),
    @Email NVARCHAR(150)
AS
BEGIN
    INSERT INTO Customers (Name, Address, PhoneNumber, Email)
    VALUES (@Name, @Address, @PhoneNumber, @Email);
END
GO

IF OBJECT_ID('usp_GetCustomers', 'P') IS NOT NULL DROP PROCEDURE usp_GetCustomers;
GO
CREATE PROCEDURE usp_GetCustomers
    @Search NVARCHAR(100) = NULL,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDir NVARCHAR(4) = 'DESC',
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SELECT *
    FROM Customers
    WHERE IsDeleted = 0
    AND (
        @Search IS NULL OR 
        Name LIKE '%' + @Search + '%' OR 
        Email LIKE '%' + @Search + '%' OR 
        PhoneNumber LIKE '%' + @Search + '%'
    )
    ORDER BY
        CASE WHEN @SortBy = 'Name' AND @SortDir = 'ASC' THEN Name END ASC,
        CASE WHEN @SortBy = 'Name' AND @SortDir = 'DESC' THEN Name END DESC,
        CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) FROM Customers WHERE IsDeleted = 0;
END
GO

IF OBJECT_ID('usp_SoftDeleteCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_SoftDeleteCustomer;
GO
CREATE PROCEDURE usp_SoftDeleteCustomer
    @CustomerId INT
AS
BEGIN
    UPDATE Customers
    SET IsDeleted = 1, UpdatedAt = GETDATE()
    WHERE CustomerId = @CustomerId;
END
GO

-- STORED PROCEDURES (INVOICE)


IF OBJECT_ID('usp_CreateInvoice', 'P') IS NOT NULL DROP PROCEDURE usp_CreateInvoice;
GO

CREATE PROCEDURE usp_CreateInvoice
    @InvoiceNumber NVARCHAR(50),
    @CustomerId INT,
    @InvoiceDate DATE,
    @DueDate DATE,
    @Terms INT,
    @TotalAmount DECIMAL(18,2)
AS
BEGIN
    INSERT INTO Invoices (InvoiceNumber, CustomerId, InvoiceDate, DueDate, Terms, TotalAmount)
    VALUES (@InvoiceNumber, @CustomerId, @InvoiceDate, @DueDate, @Terms, @TotalAmount);

    SELECT SCOPE_IDENTITY(); 
END

GO
IF OBJECT_ID('usp_CreateInvoiceLine', 'P') IS NOT NULL DROP PROCEDURE usp_CreateInvoiceLine;
GO

CREATE PROCEDURE usp_CreateInvoiceLine
    @InvoiceId INT,
    @Description NVARCHAR(255),
    @Quantity INT,
    @UnitPrice DECIMAL(18,2),
    @LineTotal DECIMAL(18,2)
AS
BEGIN
    INSERT INTO InvoiceLines (InvoiceId, Description, Quantity, UnitPrice, LineTotal)
    VALUES (@InvoiceId, @Description, @Quantity, @UnitPrice, @LineTotal);
END

GO 


-- SEED DATA

IF NOT EXISTS (SELECT 1 FROM Customers)
BEGIN
INSERT INTO Customers (Name, Address, PhoneNumber, Email)
VALUES 
('John Doe', 'NY Street', '1234567890', 'john@example.com'),
('Jane Smith', 'LA Street', '9876543210', 'jane@example.com'),
('Alice Brown', 'TX Street', '1111111111', 'alice@example.com'),
('Bob White', 'FL Street', '2222222222', 'bob@example.com'),
('Charlie Black', 'CA Street', '3333333333', 'charlie@example.com');
END
GO

IF OBJECT_ID('usp_GetCustomerById', 'P') IS NOT NULL DROP PROCEDURE usp_GetCustomerById;
GO
CREATE PROCEDURE usp_GetCustomerById
    @CustomerId INT
AS
BEGIN
    SELECT * FROM Customers
    WHERE CustomerId = @CustomerId AND IsDeleted = 0;
END

GO
IF OBJECT_ID('usp_UpdateCustomer', 'P') IS NOT NULL DROP PROCEDURE usp_UpdateCustomer;
GO
CREATE PROCEDURE usp_UpdateCustomer
    @CustomerId INT,
    @Name NVARCHAR(100),
    @Address NVARCHAR(255),
    @PhoneNumber NVARCHAR(20),
    @Email NVARCHAR(150)
AS
BEGIN
    UPDATE Customers
    SET Name = @Name,
        Address = @Address,
        PhoneNumber = @PhoneNumber,
        Email = @Email,
        UpdatedAt = GETDATE()
    WHERE CustomerId = @CustomerId AND IsDeleted = 0;
END
GO 
IF OBJECT_ID('usp_GetCustomerDropdown', 'P') IS NOT NULL DROP PROCEDURE usp_GetCustomerDropdown;
GO
CREATE PROCEDURE usp_GetCustomerDropdown
AS
BEGIN
    SELECT CustomerId, Name  FROM Customers WHERE IsDeleted = 0;
END

GO
IF OBJECT_ID('usp_GetInvoices', 'P') IS NOT NULL DROP PROCEDURE usp_GetInvoices;
GO
CREATE PROCEDURE usp_GetInvoices
    @Search NVARCHAR(100) = NULL,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDir NVARCHAR(4) = 'DESC',
    @Page INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SELECT i.*, c.Name AS CustomerName
    FROM Invoices i
    INNER JOIN Customers c ON i.CustomerId = c.CustomerId
    WHERE i.IsDeleted = 0
    AND (@Search IS NULL OR i.InvoiceNumber LIKE '%' + @Search + '%')
    ORDER BY
        CASE WHEN @SortBy = 'InvoiceNumber' AND @SortDir = 'ASC' THEN i.InvoiceNumber END ASC,
        CASE WHEN @SortBy = 'InvoiceNumber' AND @SortDir = 'DESC' THEN i.InvoiceNumber END DESC,
        CASE WHEN @SortBy = 'CreatedAt' AND @SortDir = 'ASC' THEN i.CreatedAt END ASC,
        CASE WHEN @SortBy = 'CreatedAt' AND @SortDir = 'DESC' THEN i.CreatedAt END DESC,
        i.CreatedAt DESC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;

    SELECT COUNT(*) FROM Invoices WHERE IsDeleted = 0;
END

GO 
IF OBJECT_ID('usp_GetInvoiceById', 'P') IS NOT NULL DROP PROCEDURE usp_GetInvoiceById;
GO
CREATE PROCEDURE usp_GetInvoiceById
    @InvoiceId INT
AS
BEGIN
    -- Invoice
    SELECT * FROM Invoices 
    WHERE InvoiceId = @InvoiceId AND IsDeleted = 0;

    -- Lines
    SELECT * FROM InvoiceLines 
    WHERE InvoiceId = @InvoiceId;
END
GO
IF OBJECT_ID('usp_UpdateInvoice', 'P') IS NOT NULL DROP PROCEDURE usp_UpdateInvoice;
GO
CREATE PROCEDURE usp_UpdateInvoice
    @InvoiceId INT,
    @CustomerId INT,
    @InvoiceDate DATE,
    @DueDate DATE,
    @Terms INT,
    @TotalAmount DECIMAL(18,2)
AS
BEGIN
    UPDATE Invoices
    SET CustomerId = @CustomerId,
        InvoiceDate = @InvoiceDate,
        DueDate = @DueDate,
        Terms = @Terms,
        TotalAmount = @TotalAmount,
        UpdatedAt = GETDATE()
    WHERE InvoiceId = @InvoiceId AND IsDeleted = 0;
END
GO
IF OBJECT_ID('usp_SoftDeleteInvoice', 'P') IS NOT NULL DROP PROCEDURE usp_SoftDeleteInvoice;
GO
CREATE PROCEDURE usp_SoftDeleteInvoice
    @InvoiceId INT
AS
BEGIN
    UPDATE Invoices
    SET IsDeleted = 1,
        UpdatedAt = GETDATE()
    WHERE InvoiceId = @InvoiceId;
END
GO
IF OBJECT_ID('usp_DeleteInvoiceLines', 'P') IS NOT NULL DROP PROCEDURE usp_DeleteInvoiceLines;
GO
CREATE PROCEDURE usp_DeleteInvoiceLines
    @InvoiceId INT
AS
BEGIN
    DELETE FROM InvoiceLines WHERE InvoiceId = @InvoiceId;
END
GO
IF OBJECT_ID('usp_GetNextInvoiceNumber', 'P') IS NOT NULL DROP PROCEDURE usp_GetNextInvoiceNumber;
GO
CREATE PROCEDURE usp_GetNextInvoiceNumber
AS
BEGIN
    DECLARE @NextNumber INT;

    SELECT @NextNumber = ISNULL(MAX(InvoiceId), 0) + 1 FROM Invoices;

    SELECT @NextNumber;
END
GO 
IF OBJECT_ID('usp_CheckCustomerEmailExists', 'P') IS NOT NULL DROP PROCEDURE usp_CheckCustomerEmailExists;
GO
CREATE PROCEDURE usp_CheckCustomerEmailExists
    @Email NVARCHAR(150)
AS
BEGIN
    SELECT COUNT(1) FROM Customers WHERE Email = @Email AND IsDeleted = 0;
END
GO 

INSERT INTO Invoices (InvoiceNumber, CustomerId, InvoiceDate, DueDate, Terms, TotalAmount)
VALUES
('INV-0001', 1, GETDATE(), DATEADD(DAY, 10, GETDATE()), 10, 200),
('INV-0002', 2, GETDATE(), DATEADD(DAY, 5, GETDATE()), 5, 150),
('INV-0003', 3, GETDATE(), DATEADD(DAY, 7, GETDATE()), 7, 300),
('INV-0004', 4, GETDATE(), DATEADD(DAY, 3, GETDATE()), 3, 100),
('INV-0005', 5, GETDATE(), DATEADD(DAY, 15, GETDATE()), 15, 500);


GO
INSERT INTO InvoiceLines (InvoiceId, Description, Quantity, UnitPrice, LineTotal)
VALUES
(1, 'Item A', 2, 100, 200),
(2, 'Item B', 3, 50, 150),
(3, 'Item C', 5, 60, 300),
(4, 'Item D', 1, 100, 100),
(5, 'Item E', 10, 50, 500);

GO
IF OBJECT_ID('usp_GetDashboard', 'P') IS NOT NULL DROP PROCEDURE usp_GetDashboard;
GO
CREATE PROCEDURE usp_GetDashboard
AS
BEGIN
    SELECT 
        (SELECT COUNT(*) FROM Customers WHERE IsDeleted = 0) AS TotalCustomers,
        (SELECT COUNT(*) FROM Invoices WHERE IsDeleted = 0) AS TotalInvoices,
        (SELECT ISNULL(SUM(TotalAmount),0) FROM Invoices WHERE IsDeleted = 0) AS TotalAmount
END
GO