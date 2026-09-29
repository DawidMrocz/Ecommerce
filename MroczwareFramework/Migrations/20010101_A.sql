
  IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'user')
	BEGIN
		EXEC('CREATE SCHEMA [user]');
	END;
    

GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'setting')
	BEGIN
		EXEC('CREATE SCHEMA [setting]');
	END;

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[setting].[Setting]'))
BEGIN
CREATE TABLE [setting].[Setting] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
    ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    [Key] NVARCHAR(255) NOT NULL,
    [Value] NVARCHAR(500) NOT NULL
);
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'crm')
	BEGIN
		EXEC('CREATE SCHEMA [crm]');
	END;


GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[crm].[Country]'))
BEGIN
CREATE TABLE [crm].[Country] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    Code NVARCHAR(255) NOT NULL,
    Name NVARCHAR(500) NOT NULL
);
END

IF NOT EXISTS (SELECT 1 FROM [crm].[Country])
BEGIN
    INSERT INTO [crm].[Country] 
        ([CreateUser_Id], [CreateDate], [ModifyUser_Id], [ModifyDate], [IsActive], [Code], [Name])
    VALUES
        (1, GETDATE(), NULL, NULL, 1, 'US', 'United States'), 
        (1, GETDATE(), NULL, NULL, 1, 'DE', 'Germany');
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[crm].[Currency]'))
BEGIN
CREATE TABLE [crm].[Currency] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Code] NVARCHAR(MAX) NOT NULL,
    [Rate] DECIMAL(18, 4) NOT NULL
);
END

IF NOT EXISTS (SELECT 1 FROM [crm].[Currency])
BEGIN
    INSERT INTO [crm].[Currency] 
    ([CreateUser_Id], [CreateDate], [ModifyUser_Id], [ModifyDate], [IsActive], [Name], [Code], [Rate])
    VALUES
    (1, GETDATE(), NULL, NULL, 1, 'US Dollar', 'USD', 1.0000),
    (1, GETDATE(), NULL, NULL, 1, 'Euro', 'EUR', 0.8500),
    (1, GETDATE(), NULL, NULL, 1, 'Z³oty', 'PLN', 0.8500);
END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[crm].[Culture]'))
BEGIN
CREATE TABLE [crm].[Culture] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Code] NVARCHAR(10) NOT NULL,
    [Description] NVARCHAR(MAX) NOT NULL,
    [Icon] VARBINARY(MAX) NULL, -- Opcjonalne pole na dane binarne
    [Currency_Id] INT NOT NULL FOREIGN KEY REFERENCES [crm].[Currency] ([Id])
);
END

IF NOT EXISTS (SELECT 1 FROM [crm].[Culture])
BEGIN
    INSERT INTO [crm].[Culture] 
        ([CreateUser_Id], [CreateDate], [ModifyUser_Id], [ModifyDate], [IsActive], [Name],[Code], [Description], [Icon], [Currency_Id])
    VALUES
        (1, GETDATE(), NULL, NULL, 1, 'Polska','pl-PL', 'Culture associated with Poland', NULL, 3), -- Zak³adaj¹c, ¿e '1' jest ID waluty (np. USD) w tabeli Currency
        (1, GETDATE(), NULL, NULL, 1, 'Angielska','en-GB', 'Culture associated with Europe', NULL, 2); -- Zak³adaj¹c, ¿e '2' jest ID waluty (np. EUR) w tabeli Currency
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[user].[User]'))
BEGIN
CREATE TABLE [user].[User] (
    Id INT PRIMARY KEY IDENTITY(1,1), -- Klucz g³ówny z autoinkrementacj¹
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
    ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    Email NVARCHAR(255) NOT NULL, -- Adres e-mail (z atrybutem EmailAddress)
    FirstName NVARCHAR(255) NULL, -- Imiê
    LastName NVARCHAR(255) NULL, -- Nazwisko
    PasswordHash VARBINARY(MAX) NOT NULL, -- Hash has³a
    PasswordSalt VARBINARY(MAX) NOT NULL, -- Salt has³a
    Blocked BIT NULL DEFAULT 0, -- Status blokady
    Activated BIT NOT NULL DEFAULT 0, -- Status aktywacji
    UnblockTime DATETIME NULL, -- Data odblokowania
    Street NVARCHAR(255) NULL,
    City NVARCHAR(255) NULL,
    [Country_Id] INT FOREIGN KEY REFERENCES [crm].[Country] ([Id]),
    PostalCode NVARCHAR(20) NULL,
    [Culture_Id] INT FOREIGN KEY REFERENCES [crm].[Culture] ([Id])
);
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[user].[Role]'))
BEGIN
CREATE TABLE [user].[Role] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    Name NVARCHAR(255) NOT NULL,
    StrongName NVARCHAR(255) NULL
);
END

IF NOT EXISTS (SELECT 1 FROM [user].[Role])
BEGIN
    INSERT INTO [user].[Role] 
        ([CreateUser_Id], [CreateDate], [ModifyUser_Id], [ModifyDate], [IsActive], [Name], [StrongName])
    VALUES
        (1, GETDATE(), NULL, NULL, 1, 'Admin', 'Admin');
END

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[user].[UserXRole]'))
BEGIN
CREATE TABLE [user].[UserXRole] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    User_Id INT NOT NULL FOREIGN KEY REFERENCES [user].[User] ([Id]),
    Role_Id INT NOT NULL FOREIGN KEY REFERENCES [user].[Role] ([Id])
);
END

GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.objects
    WHERE object_id = OBJECT_ID(N'[user].[Token]')
          AND type = 'U' -- U oznacza tabelê u¿ytkownika (User Table)
)
BEGIN
    CREATE TABLE [user].[RefreshToken] (
        [Id] INT IDENTITY(1,1) PRIMARY KEY, -- Klucz g³ówny
        [CreateUser_Id] INT NOT NULL,            -- Twórca rekordu
        [CreateDate] DATETIME NOT NULL DEFAULT GETDATE(), -- Data utworzenia
        [ModifyUser_Id] INT NULL,               -- Edytuj¹cy rekord
        [ModifyDate] DATETIME NULL,             -- Data modyfikacji
        [IsActive] BIT NOT NULL DEFAULT 1,      -- Czy token jest aktywny
        [JwtTokenValue] NVARCHAR(MAX) NOT NULL,         -- Wartoœæ tokenu
        [RefreshTokenValue] NVARCHAR(MAX) NOT NULL,         -- Wartoœæ tokenu
        [ExpireDate] DATETIME NOT NULL,         -- Data wygaœniêcia
        [User_Id] INT NOT NULL  FOREIGN KEY REFERENCES [user].[User]([Id])
    );
END;

GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'file')
	BEGIN
		EXEC('CREATE SCHEMA [file]');
	END;

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[file].[File]'))
BEGIN
CREATE TABLE [file].[File] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    [Name] NVARCHAR(MAX) NOT NULL,
    [Guid] UNIQUEIDENTIFIER NOT NULL,
    [Role_Id] INT FOREIGN KEY REFERENCES [user].[Role] ([Id]),
    [User_Id] INT FOREIGN KEY REFERENCES [user].[User] ([Id]),
    [Content] VARBINARY(MAX) NULL
);
END

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'email')
	BEGIN
		EXEC('CREATE SCHEMA [email]');
	END;

GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[email].[Template]'))
BEGIN
CREATE TABLE [email].[Template] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CreateUser_Id INT NOT NULL DEFAULT 1,
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
    ModifyUser_Id INT,
     ModifyDate DATETIME,
    IsActive BIT NOT NULL DEFAULT 1,
    [Body] NVARCHAR(MAX) NOT NULL,
    [Subject] NVARCHAR(MAX) NOT NULL,
    [Name] NVARCHAR(MAX) NOT NULL,
    [StrongName] NVARCHAR(MAX) NOT NULL,
    [Description] NVARCHAR(MAX) NULL
);
END

--IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE Object_ID = OBJECT_ID(N'[email].[Message]'))
--BEGIN
--CREATE TABLE [email].[Message] (
--    Id INT PRIMARY KEY IDENTITY(1,1),
--    CreateUser_Id INT NOT NULL DEFAULT 1,
--    CreateDate DATETIME NOT NULL DEFAULT GETDATE(),
--    ModifyUser_Id INT,
--     ModifyDate DATETIME,
--    IsActive BIT NOT NULL DEFAULT 1,
--    User_Id INT NOT NULL FOREIGN KEY [User_Id] REFERENCES [user].[User] ([Id]),
--    Role_Id INT NOT NULL FOREIGN KEY [Role_Id] REFERENCES [user].[Role] ([Id])
--);
--END

