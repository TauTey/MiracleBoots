/* ============================================================
   База данных TradingOrganization.
   Скрипт выполняет:
     1) заполнение пустых данных пользователей
        (дата рождения, пол, город, адрес проживания);
     2) перенумерацию первичных ключей: 1, 2, 3, ... ;
     3) «разброс» внешних ключей (ID связанных записей
        не идут подряд, а распределены по диапазону).
   Выполнять на базе TradingOrganization.
   ============================================================ */
USE TradingOrganization;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    /* ---------- 1. Заполнение пустых данных ---------- */

    -- Директора
    UPDATE dbo.Users SET BirthDate = '1979-03-14', Gender = N'М',
        City = N'Уфа', Address = N'ул. Достоевского, 12'
    WHERE Login = N'1234567890' AND BirthDate IS NULL;

    UPDATE dbo.Users SET BirthDate = '1983-07-22', Gender = N'Ж',
        City = N'Уфа', Address = N'ул. Аксакова, 24'
    WHERE Login = N'2345678901' AND BirthDate IS NULL;

    UPDATE dbo.Users SET BirthDate = '1975-11-09', Gender = N'М',
        City = N'Уфа', Address = N'ул. Коммунистическая, 30'
    WHERE Login = N'3456789012' AND BirthDate IS NULL;

    -- Заведующие точками
    UPDATE dbo.Users SET Gender = N'М', City = N'Уфа', Address = N'ул. Ленина, 18'
    WHERE Login = N'th_i_kuznecov1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'Ж', City = N'Уфа', Address = N'ул. Революционная, 41'
    WHERE Login = N'th_m_sokolova1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'М', City = N'Уфа', Address = N'ул. Цюрупы, 15'
    WHERE Login = N'th_s_petrov1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'Ж', City = N'Уфа', Address = N'ул. Кирова, 27'
    WHERE Login = N'th_a_sidorova1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'М', City = N'Уфа', Address = N'ул. Пушкина, 50'
    WHERE Login = N'th_a_mihaylov1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'Ж', City = N'Уфа', Address = N'ул. Аксакова, 11'
    WHERE Login = N'th_t_kozlova1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'М', City = N'Уфа', Address = N'ул. Комсомольская, 19'
    WHERE Login = N'th_p_nikolaev1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'Ж', City = N'Уфа', Address = N'ул. Белякова, 90'
    WHERE Login = N'th_o_alekseeva1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'М', City = N'Уфа', Address = N'ул. Степана Халтурина, 40'
    WHERE Login = N'th_e_dmitriev1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'Ж', City = N'Уфа', Address = N'ул. Социалистическая, 14'
    WHERE Login = N'th_n_egorova1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'М', City = N'Уфа', Address = N'ул. Ленина, 44'
    WHERE Login = N'th_m_andreev1' AND Gender IS NULL;
    UPDATE dbo.Users SET Gender = N'Ж', City = N'Уфа', Address = N'ул. Революционная, 35'
    WHERE Login = N'th_i_vasilyeva1' AND Gender IS NULL;

    /* ---------- 2. Подготовка: копии данных ---------- */

    -- Тестовые заявки не переносим
    DELETE FROM dbo.OrderRequests;

    SELECT * INTO #Pos     FROM dbo.Positions;
    SELECT * INTO #Users   FROM dbo.Users;
    SELECT * INTO #Orgs    FROM dbo.Organizations;
    SELECT * INTO #Stores  FROM dbo.Stores;
    SELECT * INTO #Sups    FROM dbo.Suppliers;
    SELECT * INTO #Orders  FROM dbo.Orders;

    /* ---------- 3. Отключение связей и очистка таблиц ---------- */

    DECLARE @sql nvarchar(max) = N'';

    SELECT @sql += N'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + N'.'
                 + QUOTENAME(OBJECT_NAME(parent_object_id)) + N' DROP CONSTRAINT '
                 + QUOTENAME(name) + N';' + CHAR(10)
    FROM sys.foreign_keys;

    EXEC sp_executesql @sql;

    TRUNCATE TABLE dbo.Users;
    TRUNCATE TABLE dbo.Organizations;
    TRUNCATE TABLE dbo.Stores;
    TRUNCATE TABLE dbo.Suppliers;
    TRUNCATE TABLE dbo.Orders;
    TRUNCATE TABLE dbo.Positions;
    TRUNCATE TABLE dbo.ChangeRequests;
    TRUNCATE TABLE dbo.OrderRequests;

    /* ---------- 4. Соответствие старых и новых ID ----------
       Новые ID — непрерывный диапазон 1..N.
       Порядок назначения «перемешан», поэтому значения
       внешних ключей идут не подряд, а в разброс.        */

    CREATE TABLE #MapPos (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #MapPos (OldID, NewID)
    SELECT OldID, ((rn - 1) * 7) % cnt + 1
    FROM (SELECT PositionID AS OldID,
                 ROW_NUMBER() OVER (ORDER BY PositionID) AS rn,
                 COUNT(*) OVER () AS cnt
          FROM #Pos) q;

    CREATE TABLE #MapUser (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #MapUser (OldID, NewID)
    SELECT OldID, ((rn - 1) * 7) % cnt + 1
    FROM (SELECT UserID AS OldID,
                 ROW_NUMBER() OVER (ORDER BY UserID) AS rn,
                 COUNT(*) OVER () AS cnt
          FROM #Users) q;

    CREATE TABLE #MapOrg (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #MapOrg (OldID, NewID)
    SELECT OldID, ((rn - 1) * 7) % cnt + 1
    FROM (SELECT OrganizationID AS OldID,
                 ROW_NUMBER() OVER (ORDER BY OrganizationID) AS rn,
                 COUNT(*) OVER () AS cnt
          FROM #Orgs) q;

    CREATE TABLE #MapStore (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #MapStore (OldID, NewID)
    SELECT OldID, ((rn - 1) * 7) % cnt + 1
    FROM (SELECT StoreID AS OldID,
                 ROW_NUMBER() OVER (ORDER BY StoreID) AS rn,
                 COUNT(*) OVER () AS cnt
          FROM #Stores) q;

    CREATE TABLE #MapSup (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #MapSup (OldID, NewID)
    SELECT OldID, ((rn - 1) * 7) % cnt + 1
    FROM (SELECT SupplierID AS OldID,
                 ROW_NUMBER() OVER (ORDER BY SupplierID) AS rn,
                 COUNT(*) OVER () AS cnt
          FROM #Sups) q;

    CREATE TABLE #MapOrder (OldID int PRIMARY KEY, NewID int NOT NULL);
    INSERT INTO #MapOrder (OldID, NewID)
    SELECT OldID, ((rn - 1) * 7) % cnt + 1
    FROM (SELECT OrderID AS OldID,
                 ROW_NUMBER() OVER (ORDER BY OrderID) AS rn,
                 COUNT(*) OVER () AS cnt
          FROM #Orders) q;

    /* ---------- 5. Загрузка данных с новыми ID ---------- */

    SET IDENTITY_INSERT dbo.Positions ON;
    INSERT INTO dbo.Positions (PositionID, Name)
    SELECT m.NewID, s.Name
    FROM #Pos s JOIN #MapPos m ON m.OldID = s.PositionID;
    SET IDENTITY_INSERT dbo.Positions OFF;

    SET IDENTITY_INSERT dbo.Users ON;
    INSERT INTO dbo.Users (UserID, LastName, FirstName, Patronymic, Login, Password,
                           BirthDate, Gender, City, Address, Photo, Role, PositionID, StoreID)
    SELECT m.NewID, s.LastName, s.FirstName, s.Patronymic, s.Login, s.Password,
           s.BirthDate, s.Gender, s.City, s.Address, s.Photo, s.Role,
           pm.NewID, sm.NewID
    FROM #Users s
    JOIN #MapUser m ON m.OldID = s.UserID
    LEFT JOIN #MapPos pm ON pm.OldID = s.PositionID
    LEFT JOIN #MapStore sm ON sm.OldID = s.StoreID;
    SET IDENTITY_INSERT dbo.Users OFF;

    SET IDENTITY_INSERT dbo.Organizations ON;
    INSERT INTO dbo.Organizations (OrganizationID, Name, Address, DirectorID)
    SELECT m.NewID, s.Name, s.Address, um.NewID
    FROM #Orgs s
    JOIN #MapOrg m ON m.OldID = s.OrganizationID
    JOIN #MapUser um ON um.OldID = s.DirectorID;
    SET IDENTITY_INSERT dbo.Organizations OFF;

    SET IDENTITY_INSERT dbo.Stores ON;
    INSERT INTO dbo.Stores (StoreID, Type, OrganizationID, Address, ManagerID)
    SELECT m.NewID, s.Type, om.NewID, s.Address, um.NewID
    FROM #Stores s
    JOIN #MapStore m ON m.OldID = s.StoreID
    JOIN #MapOrg om ON om.OldID = s.OrganizationID
    JOIN #MapUser um ON um.OldID = s.ManagerID;
    SET IDENTITY_INSERT dbo.Stores OFF;

    SET IDENTITY_INSERT dbo.Suppliers ON;
    INSERT INTO dbo.Suppliers (SupplierID, Name, ActivityType, Country, City, Address)
    SELECT m.NewID, s.Name, s.ActivityType, s.Country, s.City, s.Address
    FROM #Sups s JOIN #MapSup m ON m.OldID = s.SupplierID;
    SET IDENTITY_INSERT dbo.Suppliers OFF;

    SET IDENTITY_INSERT dbo.Orders ON;
    INSERT INTO dbo.Orders (OrderID, OrderDate, StoreID, SupplierID, ProductName, Quantity, Price)
    SELECT m.NewID, s.OrderDate, sm.NewID, pm.NewID, s.ProductName, s.Quantity, s.Price
    FROM #Orders s
    JOIN #MapOrder m ON m.OldID = s.OrderID
    JOIN #MapStore sm ON sm.OldID = s.StoreID
    JOIN #MapSup pm ON pm.OldID = s.SupplierID;
    SET IDENTITY_INSERT dbo.Orders OFF;

    /* ---------- 6. Счётчики IDENTITY ---------- */

    DBCC CHECKIDENT ('dbo.Positions', RESEED)     WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Users', RESEED)         WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Organizations', RESEED) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Stores', RESEED)        WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Suppliers', RESEED)     WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.Orders', RESEED)        WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.ChangeRequests', RESEED, 1) WITH NO_INFOMSGS;
    DBCC CHECKIDENT ('dbo.OrderRequests', RESEED, 1)  WITH NO_INFOMSGS;

    /* ---------- 7. Восстановление связей ---------- */

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

    ALTER TABLE dbo.OrderRequests ADD CONSTRAINT FK_OrderRequests_Store
        FOREIGN KEY (StoreID) REFERENCES dbo.Stores (StoreID);
    ALTER TABLE dbo.OrderRequests ADD CONSTRAINT FK_OrderRequests_Manager
        FOREIGN KEY (ManagerID) REFERENCES dbo.Users (UserID);
    ALTER TABLE dbo.OrderRequests ADD CONSTRAINT FK_OrderRequests_Director
        FOREIGN KEY (DirectorID) REFERENCES dbo.Users (UserID);
    ALTER TABLE dbo.OrderRequests ADD CONSTRAINT FK_OrderRequests_Supplier
        FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers (SupplierID);
    ALTER TABLE dbo.OrderRequests ADD CONSTRAINT FK_OrderRequests_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.Orders (OrderID);

    COMMIT TRANSACTION;
    PRINT N'Перенумерация и заполнение данных завершены.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    THROW;
END CATCH;
GO
