/* ============================================================
   Миграция базы данных "Торговая организация"
   - английские имена таблиц и столбцов;
   - директора, заведующие и сотрудники объединены в Users;
   - добавлены должности;
   - ID первичных ключей "разбросаны" (не 1, 2, 3, ...);
   - добавлена таблица заявок на изменение данных.
   Выполнять на базе "Торговая организация".
   ============================================================ */
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* ---------- 1. Соответствие старых и новых ID ---------- */

    CREATE TABLE #PosMap (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #PosMap (OldID, NewID) VALUES
        (1, 24), (2, 31), (3, 36), (4, 43);

    CREATE TABLE #UserMap (SrcTable varchar(10) NOT NULL, SrcID int NOT NULL,
                           NewID int NOT NULL, PRIMARY KEY (SrcTable, SrcID));
    INSERT INTO #UserMap (SrcTable, SrcID, NewID) VALUES
        ('Director', 1, 5),  ('Director', 2, 9),   ('Director', 3, 14),
        ('Manager', 1, 18),  ('Manager', 2, 23),   ('Manager', 3, 27),
        ('Manager', 4, 32),  ('Manager', 5, 36),   ('Manager', 6, 41),
        ('Manager', 7, 45),  ('Manager', 8, 50),   ('Manager', 9, 54),
        ('Manager', 10, 59), ('Manager', 11, 63),  ('Manager', 12, 68),
        ('Employee', 1, 72), ('Employee', 2, 77),  ('Employee', 3, 81),
        ('Employee', 4, 86), ('Employee', 5, 90),  ('Employee', 6, 95),
        ('Employee', 7, 99), ('Employee', 8, 104), ('Employee', 9, 108),
        ('Employee', 10, 113), ('Employee', 11, 117), ('Employee', 12, 122),
        ('Employee', 13, 126), ('Employee', 14, 131), ('Employee', 15, 135),
        ('Employee', 16, 140), ('Employee', 17, 144), ('Employee', 18, 149),
        ('Employee', 19, 153), ('Employee', 20, 158), ('Employee', 21, 162),
        ('Employee', 22, 167), ('Employee', 23, 171);

    CREATE TABLE #OrgMap (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #OrgMap (OldID, NewID) VALUES (1, 3), (2, 9), (3, 16);

    CREATE TABLE #StoreMap (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #StoreMap (OldID, NewID) VALUES
        (1, 6), (2, 11), (3, 17), (4, 22), (5, 28), (6, 33),
        (7, 39), (8, 44), (9, 50), (10, 55), (11, 61), (12, 66);

    CREATE TABLE #SupMap (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #SupMap (OldID, NewID) VALUES
        (1, 4), (2, 10), (3, 15), (4, 21), (5, 26),
        (6, 32), (7, 37), (8, 43), (9, 48), (10, 54);

    CREATE TABLE #OrderMap (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #OrderMap (OldID, NewID) VALUES
        (1, 8), (2, 14), (3, 21), (4, 27), (5, 34), (6, 40),
        (7, 47), (8, 53), (9, 60), (10, 66), (11, 73), (12, 79),
        (13, 86), (14, 92), (15, 99), (16, 105), (17, 112), (18, 118);

    /* ---------- 2. Новые таблицы ---------- */

    CREATE TABLE dbo.Positions
    (
        PositionID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Positions PRIMARY KEY,
        Name nvarchar(50) NOT NULL
    );

    CREATE TABLE dbo.Users
    (
        UserID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        LastName nvarchar(100) NOT NULL,
        FirstName nvarchar(100) NOT NULL,
        Patronymic nvarchar(100) NULL,
        Login nvarchar(50) NOT NULL,
        Password nvarchar(50) NOT NULL,
        BirthDate date NULL,
        Gender nvarchar(1) NULL,
        City nvarchar(100) NULL,
        Address nvarchar(100) NULL,
        Photo nvarchar(100) NULL,
        Role nvarchar(20) NOT NULL,
        PositionID int NULL,
        StoreID int NULL
    );

    CREATE TABLE dbo.Organizations
    (
        OrganizationID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Organizations PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Address nvarchar(100) NOT NULL,
        DirectorID int NOT NULL
    );

    CREATE TABLE dbo.Stores
    (
        StoreID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Stores PRIMARY KEY,
        Type nvarchar(50) NOT NULL,
        OrganizationID int NOT NULL,
        Address nvarchar(100) NOT NULL,
        ManagerID int NOT NULL
    );

    CREATE TABLE dbo.Suppliers
    (
        SupplierID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Suppliers PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        ActivityType nvarchar(100) NOT NULL,
        Country nvarchar(50) NOT NULL,
        City nvarchar(100) NOT NULL,
        Address nvarchar(100) NOT NULL
    );

    CREATE TABLE dbo.Orders
    (
        OrderID int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        OrderDate date NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT (GETDATE()),
        StoreID int NOT NULL,
        SupplierID int NOT NULL,
        ProductName nvarchar(100) NOT NULL,
        Quantity nvarchar(100) NOT NULL,
        Price numeric(18,2) NOT NULL
    );

    CREATE TABLE dbo.ChangeRequests
    (
        RequestID int IDENTITY(101,1) NOT NULL CONSTRAINT PK_ChangeRequests PRIMARY KEY,
        UserID int NOT NULL,
        Message nvarchar(1000) NOT NULL,
        Status nvarchar(20) NOT NULL,
        CreatedAt datetime NOT NULL CONSTRAINT DF_ChangeRequests_CreatedAt DEFAULT (GETDATE()),
        ResolvedAt datetime NULL,
        ManagerID int NULL,
        ManagerComment nvarchar(1000) NULL
    );

    CREATE TABLE dbo.March2026OrdersArchive
    (
        ArchiveID int NULL,
        OrderDate date NULL,
        ProductName nvarchar(100) NULL,
        Price numeric(18,2) NULL
    );

    /* ---------- 3. Перенос данных ---------- */

    SET IDENTITY_INSERT dbo.Positions ON;
    INSERT INTO dbo.Positions (PositionID, Name)
    SELECT m.NewID, p.Название
    FROM dbo.Должности p
    JOIN #PosMap m ON m.OldID = p.ID_должности;

    INSERT INTO dbo.Positions (PositionID, Name) VALUES
        (2,  N'кассир'),
        (7,  N'кладовщик'),
        (12, N'мерчендайзер'),
        (19, N'администратор'),
        (48, N'грузчик'),
        (55, N'бухгалтер');
    SET IDENTITY_INSERT dbo.Positions OFF;

    SET IDENTITY_INSERT dbo.Users ON;

    INSERT INTO dbo.Users (UserID, LastName, FirstName, Patronymic, Login, Password,
                           BirthDate, Gender, City, Address, Photo, Role, PositionID, StoreID)
    SELECT m.NewID, d.Фамилия, d.Имя, d.Отчество, d.Налоговый_номер, d.Пароль,
           NULL, NULL, NULL, NULL, d.Фото, 'Director', NULL, NULL
    FROM dbo.Директора d
    JOIN #UserMap m ON m.SrcTable = 'Director' AND m.SrcID = d.ID_директора;

    INSERT INTO dbo.Users (UserID, LastName, FirstName, Patronymic, Login, Password,
                           BirthDate, Gender, City, Address, Photo, Role, PositionID, StoreID)
    SELECT m.NewID, z.Фамилия, z.Имя, z.Отчество, z.Логин, z.Пароль,
           z.Год_рождения, NULL, NULL, NULL, z.Фото, 'Manager', NULL, NULL
    FROM dbo.Заведующие_точкой z
    JOIN #UserMap m ON m.SrcTable = 'Manager' AND m.SrcID = z.ID_заведующего;

    INSERT INTO dbo.Users (UserID, LastName, FirstName, Patronymic, Login, Password,
                           BirthDate, Gender, City, Address, Photo, Role, PositionID, StoreID)
    SELECT m.NewID, s.Фамилия, s.Имя, s.Отчество, s.Логин, s.Пароль,
           s.Год_рождения, s.Пол, s.Город, s.Адрес_проживания, s.Фото, 'Employee',
           pm.NewID, sm.NewID
    FROM dbo.Сотрудники s
    JOIN #UserMap m ON m.SrcTable = 'Employee' AND m.SrcID = s.ID_сотрудника
    JOIN #PosMap pm ON pm.OldID = s.ID_должности
    JOIN #StoreMap sm ON sm.OldID = s.ID_торговой_точки;

    SET IDENTITY_INSERT dbo.Users OFF;

    SET IDENTITY_INSERT dbo.Organizations ON;
    INSERT INTO dbo.Organizations (OrganizationID, Name, Address, DirectorID)
    SELECT om.NewID, o.Название, o.Адрес, um.NewID
    FROM dbo.Торговая_организация o
    JOIN #OrgMap om ON om.OldID = o.ID_организации
    JOIN #UserMap um ON um.SrcTable = 'Director' AND um.SrcID = o.ID_директора;
    SET IDENTITY_INSERT dbo.Organizations OFF;

    SET IDENTITY_INSERT dbo.Stores ON;
    INSERT INTO dbo.Stores (StoreID, Type, OrganizationID, Address, ManagerID)
    SELECT sm.NewID, t.Тип, om.NewID, t.Адрес, um.NewID
    FROM dbo.Торговая_точка t
    JOIN #StoreMap sm ON sm.OldID = t.ID_торговой_точки
    JOIN #OrgMap om ON om.OldID = t.ID_организации
    JOIN #UserMap um ON um.SrcTable = 'Manager' AND um.SrcID = t.ID_заведующего;
    SET IDENTITY_INSERT dbo.Stores OFF;

    SET IDENTITY_INSERT dbo.Suppliers ON;
    INSERT INTO dbo.Suppliers (SupplierID, Name, ActivityType, Country, City, Address)
    SELECT m.NewID, p.Название, p.Тип_деятельности, p.Страна, p.Город, p.Адрес
    FROM dbo.Поставщики p
    JOIN #SupMap m ON m.OldID = p.ID_поставщика;
    SET IDENTITY_INSERT dbo.Suppliers OFF;

    SET IDENTITY_INSERT dbo.Orders ON;
    INSERT INTO dbo.Orders (OrderID, OrderDate, StoreID, SupplierID, ProductName, Quantity, Price)
    SELECT om.NewID, z.Дата_заказа, sm.NewID, pm.NewID, z.Название_товара, z.Количество, z.Цена
    FROM dbo.Заказы_поставщикам z
    JOIN #OrderMap om ON om.OldID = z.ID_заказа_поставщику
    JOIN #StoreMap sm ON sm.OldID = z.ID_торговой_точки
    JOIN #SupMap pm ON pm.OldID = z.ID_поставщика;
    SET IDENTITY_INSERT dbo.Orders OFF;

    INSERT INTO dbo.March2026OrdersArchive (ArchiveID, OrderDate, ProductName, Price)
    SELECT a.ID, a.Дата_заказа, a.Название_товара, a.Цена
    FROM dbo.Заказы_март_2026_архив a;

    /* ---------- 4. Ограничения ---------- */

    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Positions
        FOREIGN KEY (PositionID) REFERENCES dbo.Positions (PositionID);
    ALTER TABLE dbo.Users ADD CONSTRAINT FK_Users_Stores
        FOREIGN KEY (StoreID) REFERENCES dbo.Stores (StoreID);
    ALTER TABLE dbo.Organizations ADD CONSTRAINT FK_Organizations_Users
        FOREIGN KEY (DirectorID) REFERENCES dbo.Users (UserID);
    ALTER TABLE dbo.Stores ADD CONSTRAINT FK_Stores_Organizations
        FOREIGN KEY (OrganizationID) REFERENCES dbo.Organizations (OrganizationID);
    ALTER TABLE dbo.Stores ADD CONSTRAINT FK_Stores_Users
        FOREIGN KEY (ManagerID) REFERENCES dbo.Users (UserID);
    ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_Stores
        FOREIGN KEY (StoreID) REFERENCES dbo.Stores (StoreID);
    ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_Suppliers
        FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers (SupplierID);
    ALTER TABLE dbo.ChangeRequests ADD CONSTRAINT FK_ChangeRequests_Users
        FOREIGN KEY (UserID) REFERENCES dbo.Users (UserID);
    ALTER TABLE dbo.ChangeRequests ADD CONSTRAINT FK_ChangeRequests_Managers
        FOREIGN KEY (ManagerID) REFERENCES dbo.Users (UserID);

    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Role
        CHECK (Role IN ('Employee', 'Manager', 'Director'));
    ALTER TABLE dbo.Users ADD CONSTRAINT CK_Users_Gender
        CHECK (Gender IS NULL OR Gender IN ('М', 'Ж'));
    ALTER TABLE dbo.Stores ADD CONSTRAINT CK_Stores_Type
        CHECK (Type IN ('лоток', 'киоск', 'магазин', 'универмаг'));
    ALTER TABLE dbo.ChangeRequests ADD CONSTRAINT CK_ChangeRequests_Status
        CHECK (Status IN ('Pending', 'Applied', 'Rejected', 'Confirmed', 'NeedsFix'));

    CREATE UNIQUE INDEX UX_Users_Login ON dbo.Users (Login);
    CREATE INDEX IX_Users_StoreID ON dbo.Users (StoreID);
    CREATE INDEX IX_Orders_StoreID ON dbo.Orders (StoreID);
    CREATE INDEX IX_ChangeRequests_UserID ON dbo.ChangeRequests (UserID);

    /* ---------- 5. Удаление старых объектов ---------- */

    IF OBJECT_ID(N'dbo.Заказы_март_2026', N'V') IS NOT NULL DROP VIEW dbo.Заказы_март_2026;
    IF OBJECT_ID(N'dbo.Информация_о_продавцах', N'V') IS NOT NULL DROP VIEW dbo.Информация_о_продавцах;
    IF OBJECT_ID(N'dbo.Итоги_заказов_по_поставщикам', N'V') IS NOT NULL DROP VIEW dbo.Итоги_заказов_по_поставщикам;
    IF OBJECT_ID(N'dbo.Точки_без_заказов', N'V') IS NOT NULL DROP VIEW dbo.Точки_без_заказов;

    DROP TABLE dbo.Заказы_поставщикам;
    DROP TABLE dbo.Сотрудники;
    DROP TABLE dbo.Торговая_точка;
    DROP TABLE dbo.Торговая_организация;
    DROP TABLE dbo.Заведующие_точкой;
    DROP TABLE dbo.Директора;
    DROP TABLE dbo.Должности;
    DROP TABLE dbo.Поставщики;
    DROP TABLE dbo.Заказы_март_2026_архив;

    /* ---------- 6. Представления на английском ---------- */

    EXEC (N'CREATE VIEW dbo.SellerInfo AS
SELECT u.UserID AS SellerID, u.LastName, u.FirstName, u.Patronymic,
       p.Name AS PositionName, u.BirthDate, u.Gender,
       s.Type AS StoreType, s.Address AS StoreAddress,
       mgr.LastName + N'' '' + mgr.FirstName + ISNULL(N'' '' + mgr.Patronymic, N'''') AS ManagerName
FROM dbo.Users u
JOIN dbo.Positions p ON p.PositionID = u.PositionID
JOIN dbo.Stores s ON s.StoreID = u.StoreID
LEFT JOIN dbo.Users mgr ON mgr.UserID = s.ManagerID
WHERE u.Role = N''Employee''');

    EXEC (N'CREATE VIEW dbo.SupplierOrderTotals AS
SELECT sup.SupplierID, sup.Name AS SupplierName,
       COUNT(o.OrderID) AS OrderCount,
       ISNULL(SUM(o.Price), 0) AS TotalAmount,
       ISNULL(AVG(o.Price), 0) AS AverageAmount
FROM dbo.Suppliers sup
LEFT JOIN dbo.Orders o ON o.SupplierID = sup.SupplierID
GROUP BY sup.SupplierID, sup.Name');

    EXEC (N'CREATE VIEW dbo.StoresWithoutOrders AS
SELECT s.StoreID, s.Type, s.Address,
       mgr.LastName + N'' '' + mgr.FirstName + ISNULL(N'' '' + mgr.Patronymic, N'''') AS ManagerName,
       s.OrganizationID
FROM dbo.Stores s
LEFT JOIN dbo.Orders o ON o.StoreID = s.StoreID
LEFT JOIN dbo.Users mgr ON mgr.UserID = s.ManagerID
WHERE o.OrderID IS NULL');

    EXEC (N'CREATE VIEW dbo.March2026Orders AS
SELECT o.OrderID AS OrderNumber, o.OrderDate, o.ProductName, o.Quantity, o.Price,
       s.Type AS StoreType, s.Address AS StoreAddress, sup.Name AS SupplierName
FROM dbo.Orders o
JOIN dbo.Stores s ON s.StoreID = o.StoreID
JOIN dbo.Suppliers sup ON sup.SupplierID = o.SupplierID
WHERE o.OrderDate BETWEEN ''2026-03-01'' AND ''2026-03-31''');

    COMMIT TRANSACTION;
    PRINT N'Миграция завершена успешно';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
