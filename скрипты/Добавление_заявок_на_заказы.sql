/* ============================================================
   Заявки на заказы поставщикам.
   Заведующий составляет заявку, директор организации её
   подтверждает (создаётся заказ), отклоняет или отменяет
   уже подтверждённую заявку с указанием причины.
   ============================================================ */

IF OBJECT_ID('dbo.OrderRequests', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderRequests
    (
        OrderRequestID  INT            IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_OrderRequests PRIMARY KEY,
        StoreID         INT            NOT NULL,
        ManagerID       INT            NOT NULL,
        DirectorID      INT            NOT NULL,
        SupplierID      INT            NOT NULL,
        ProductName     NVARCHAR(100)  NOT NULL,
        Quantity        DECIMAL(10, 2) NOT NULL,
        Unit            NVARCHAR(20)   NOT NULL,
        Price           DECIMAL(10, 2) NOT NULL,
        Status          NVARCHAR(20)   NOT NULL
            CONSTRAINT DF_OrderRequests_Status DEFAULT (N'Pending'),
        CreatedAt       DATETIME       NOT NULL
            CONSTRAINT DF_OrderRequests_CreatedAt DEFAULT (GETDATE()),
        ResolvedAt      DATETIME       NULL,
        DirectorComment NVARCHAR(500)  NULL,
        OrderID         INT            NULL
    );

    ALTER TABLE dbo.OrderRequests
        ADD CONSTRAINT CK_OrderRequests_Status
        CHECK (Status IN (N'Pending', N'Approved', N'Rejected', N'Cancelled'));

    ALTER TABLE dbo.OrderRequests
        ADD CONSTRAINT FK_OrderRequests_Store
        FOREIGN KEY (StoreID) REFERENCES dbo.Stores(StoreID);

    ALTER TABLE dbo.OrderRequests
        ADD CONSTRAINT FK_OrderRequests_Manager
        FOREIGN KEY (ManagerID) REFERENCES dbo.Users(UserID);

    ALTER TABLE dbo.OrderRequests
        ADD CONSTRAINT FK_OrderRequests_Director
        FOREIGN KEY (DirectorID) REFERENCES dbo.Users(UserID);

    ALTER TABLE dbo.OrderRequests
        ADD CONSTRAINT FK_OrderRequests_Supplier
        FOREIGN KEY (SupplierID) REFERENCES dbo.Suppliers(SupplierID);

    ALTER TABLE dbo.OrderRequests
        ADD CONSTRAINT FK_OrderRequests_Order
        FOREIGN KEY (OrderID) REFERENCES dbo.Orders(OrderID);

    CREATE INDEX IX_OrderRequests_Manager
        ON dbo.OrderRequests(ManagerID, Status);

    CREATE INDEX IX_OrderRequests_Director
        ON dbo.OrderRequests(DirectorID, Status);
END
GO
