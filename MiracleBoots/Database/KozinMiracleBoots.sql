/*
    База данных "Чудо Обувь" (система оформления заказа обуви)
    СУБД: Microsoft SQL Server
    Скрипт создаёт базу данных KozinMiracleBoots, все объекты и загружает данные
    из файлов Products_import.xlsx, Sizes_import.xlsx, Users_import.xlsx,
    Orders_import.xlsx, Stock_Items_import.xlsx.

    Даты заказов сохранены с тем же интервалом, что и в исходном файле,
    но сдвинуты на 195 дн. вперёд, чтобы последний заказ приходился на текущую дату.
*/

IF DB_ID(N'KozinMiracleBoots') IS NULL
BEGIN
    CREATE DATABASE [KozinMiracleBoots];
END
GO

USE [KozinMiracleBoots];
GO

/* Удаление существующих таблиц для повторного запуска скрипта */
IF OBJECT_ID(N'dbo.OrderProducts', N'U') IS NOT NULL DROP TABLE dbo.OrderProducts;
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID(N'dbo.StockItems', N'U') IS NOT NULL DROP TABLE dbo.StockItems;
IF OBJECT_ID(N'dbo.Products', N'U') IS NOT NULL DROP TABLE dbo.Products;
IF OBJECT_ID(N'dbo.Sizes', N'U') IS NOT NULL DROP TABLE dbo.Sizes;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL DROP TABLE dbo.Roles;
IF OBJECT_ID(N'dbo.Subcategories', N'U') IS NOT NULL DROP TABLE dbo.Subcategories;
IF OBJECT_ID(N'dbo.Manufactures', N'U') IS NOT NULL DROP TABLE dbo.Manufactures;
IF OBJECT_ID(N'dbo.Categories', N'U') IS NOT NULL DROP TABLE dbo.Categories;
GO

/* Справочник ролей */
CREATE TABLE dbo.Roles (
    ID_Role   int           IDENTITY(1,1) NOT NULL,
    RoleName  nvarchar(50)  NOT NULL,
    CONSTRAINT PK_Roles PRIMARY KEY (ID_Role),
    CONSTRAINT UQ_Roles_RoleName UNIQUE (RoleName)
);
GO

/* Категории обуви */
CREATE TABLE dbo.Categories (
    ID_Category    int           IDENTITY(1,1) NOT NULL,
    CategoryName   nvarchar(100) NOT NULL,
    CONSTRAINT PK_Categories PRIMARY KEY (ID_Category),
    CONSTRAINT UQ_Categories_CategoryName UNIQUE (CategoryName)
);
GO

/* Подкатегории (виды обуви) */
CREATE TABLE dbo.Subcategories (
    ID_Subcategory   int           IDENTITY(1,1) NOT NULL,
    SubcategoryName  nvarchar(100) NOT NULL,
    CONSTRAINT PK_Subcategories PRIMARY KEY (ID_Subcategory),
    CONSTRAINT UQ_Subcategories_SubcategoryName UNIQUE (SubcategoryName)
);
GO

/* Производители */
CREATE TABLE dbo.Manufactures (
    ID_Manufacture   int           IDENTITY(1,1) NOT NULL,
    ManufactureName  nvarchar(100) NOT NULL,
    CONSTRAINT PK_Manufactures PRIMARY KEY (ID_Manufacture),
    CONSTRAINT UQ_Manufactures_ManufactureName UNIQUE (ManufactureName)
);
GO

/* Размеры обуви */
CREATE TABLE dbo.Sizes (
    ID_Size  int          IDENTITY(1,1) NOT NULL,
    Size     decimal(3,1) NOT NULL,
    CONSTRAINT PK_Sizes PRIMARY KEY (ID_Size),
    CONSTRAINT UQ_Sizes_Size UNIQUE (Size)
);
GO

/* Пользователи системы */
CREATE TABLE dbo.Users (
    ID_User         int          IDENTITY(1,1) NOT NULL,
    UserName        nvarchar(50) NOT NULL,
    UserSurname     nvarchar(50) NOT NULL,
    UserPatronymic  nvarchar(50) NULL,
    UserLogin       nvarchar(50) NOT NULL,
    ID_Role         int          NOT NULL,
    CONSTRAINT PK_Users PRIMARY KEY (ID_User),
    CONSTRAINT UQ_Users_UserLogin UNIQUE (UserLogin),
    CONSTRAINT FK_Users_Roles FOREIGN KEY (ID_Role) REFERENCES dbo.Roles (ID_Role)
);
GO

/* Модели обуви */
CREATE TABLE dbo.Products (
    ID_Product         int            IDENTITY(1,1) NOT NULL,
    ProductName        nvarchar(200)  NOT NULL,
    ID_Subcategory     int            NOT NULL,
    ID_Manufacture     int            NOT NULL,
    ProductDescription nvarchar(max)  NULL,
    ProductComposition nvarchar(max)  NULL,
    ProductCost        decimal(10,2)  NOT NULL,
    ID_Category        int            NOT NULL,
    ProductImage       nvarchar(100)  NULL,
    CONSTRAINT PK_Products PRIMARY KEY (ID_Product),
    CONSTRAINT FK_Products_Categories FOREIGN KEY (ID_Category) REFERENCES dbo.Categories (ID_Category),
    CONSTRAINT FK_Products_Subcategories FOREIGN KEY (ID_Subcategory) REFERENCES dbo.Subcategories (ID_Subcategory),
    CONSTRAINT FK_Products_Manufactures FOREIGN KEY (ID_Manufacture) REFERENCES dbo.Manufactures (ID_Manufacture),
    CONSTRAINT CK_Products_ProductCost CHECK (ProductCost >= 0)
);
GO

/* Товарные позиции: сочетание модели и размера (склад) */
CREATE TABLE dbo.StockItems (
    ID_StockItem   int IDENTITY(1,1) NOT NULL,
    ID_Product     int NOT NULL,
    ID_Size        int NOT NULL,
    ItemsQuantity  int NOT NULL,
    CONSTRAINT PK_StockItems PRIMARY KEY (ID_StockItem),
    CONSTRAINT UQ_StockItems_Product_Size UNIQUE (ID_Product, ID_Size),
    CONSTRAINT FK_StockItems_Products FOREIGN KEY (ID_Product) REFERENCES dbo.Products (ID_Product),
    CONSTRAINT FK_StockItems_Sizes FOREIGN KEY (ID_Size) REFERENCES dbo.Sizes (ID_Size),
    CONSTRAINT CK_StockItems_ItemsQuantity CHECK (ItemsQuantity >= 0)
);
GO

/* Заказы */
CREATE TABLE dbo.Orders (
    ID_Order     int           IDENTITY(1,1) NOT NULL,
    OrderDate    date          NOT NULL,
    ID_User      int           NOT NULL,
    TotalAmount  decimal(10,2) NULL,
    CONSTRAINT PK_Orders PRIMARY KEY (ID_Order),
    CONSTRAINT FK_Orders_Users FOREIGN KEY (ID_User) REFERENCES dbo.Users (ID_User),
    CONSTRAINT CK_Orders_TotalAmount CHECK (TotalAmount IS NULL OR TotalAmount >= 0)
);
GO

/* Позиции заказа */
CREATE TABLE dbo.OrderProducts (
    ID_OrderItem       int           IDENTITY(1,1) NOT NULL,
    ID_Order           int           NOT NULL,
    ID_StockItem       int           NOT NULL,
    Quantity           int           NOT NULL,
    PriceAtOrderTime   decimal(18,4) NOT NULL,
    CONSTRAINT PK_OrderProducts PRIMARY KEY (ID_OrderItem),
    CONSTRAINT FK_OrderProducts_Orders FOREIGN KEY (ID_Order) REFERENCES dbo.Orders (ID_Order) ON DELETE CASCADE,
    CONSTRAINT FK_OrderProducts_StockItems FOREIGN KEY (ID_StockItem) REFERENCES dbo.StockItems (ID_StockItem),
    CONSTRAINT CK_OrderProducts_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderProducts_PriceAtOrderTime CHECK (PriceAtOrderTime >= 0)
);
GO

/* Индексы по внешним ключам */
CREATE INDEX IX_Users_ID_Role ON dbo.Users (ID_Role);
CREATE INDEX IX_Products_ID_Category ON dbo.Products (ID_Category);
CREATE INDEX IX_Products_ID_Subcategory ON dbo.Products (ID_Subcategory);
CREATE INDEX IX_Products_ID_Manufacture ON dbo.Products (ID_Manufacture);
CREATE INDEX IX_StockItems_ID_Size ON dbo.StockItems (ID_Size);
CREATE INDEX IX_Orders_ID_User ON dbo.Orders (ID_User);
CREATE INDEX IX_Orders_OrderDate ON dbo.Orders (OrderDate);
CREATE INDEX IX_OrderProducts_ID_StockItem ON dbo.OrderProducts (ID_StockItem);
GO

/* ------------------------- Данные ------------------------- */
SET NOCOUNT ON;
GO

SET IDENTITY_INSERT dbo.Roles ON;
INSERT INTO dbo.Roles (ID_Role, RoleName) VALUES (1, N'Администратор');
INSERT INTO dbo.Roles (ID_Role, RoleName) VALUES (2, N'Менеджер');
INSERT INTO dbo.Roles (ID_Role, RoleName) VALUES (3, N'Авторизованный пользователь');
SET IDENTITY_INSERT dbo.Roles OFF;
GO

SET IDENTITY_INSERT dbo.Categories ON;
INSERT INTO dbo.Categories (ID_Category, CategoryName) VALUES (1, N'Детская обувь');
INSERT INTO dbo.Categories (ID_Category, CategoryName) VALUES (2, N'Женская обувь');
INSERT INTO dbo.Categories (ID_Category, CategoryName) VALUES (3, N'Мужская обувь');
SET IDENTITY_INSERT dbo.Categories OFF;
GO

SET IDENTITY_INSERT dbo.Subcategories ON;
INSERT INTO dbo.Subcategories (ID_Subcategory, SubcategoryName) VALUES (1, N'Кроссовки');
INSERT INTO dbo.Subcategories (ID_Subcategory, SubcategoryName) VALUES (2, N'Сапоги');
INSERT INTO dbo.Subcategories (ID_Subcategory, SubcategoryName) VALUES (3, N'Босоножки');
INSERT INTO dbo.Subcategories (ID_Subcategory, SubcategoryName) VALUES (4, N'Туфли');
INSERT INTO dbo.Subcategories (ID_Subcategory, SubcategoryName) VALUES (5, N'Ботинки');
SET IDENTITY_INSERT dbo.Subcategories OFF;
GO

SET IDENTITY_INSERT dbo.Manufactures ON;
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (1, N'Малыш-Спорт');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (2, N'Топ-Топ');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (3, N'Лапушка');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (4, N'Тёплые ножки');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (5, N'Северята');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (6, N'Весна');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (7, N'Здоровый шаг');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (8, N'Карапуз');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (9, N'Маленький франт');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (10, N'Барбари');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (11, N'Твой комфорт');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (12, N'Стиль и комфорт');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (13, N'Кожевенные традиции');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (14, N'Модный силуэт');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (15, N'Азимут');
INSERT INTO dbo.Manufactures (ID_Manufacture, ManufactureName) VALUES (16, N'Берестов');
SET IDENTITY_INSERT dbo.Manufactures OFF;
GO

SET IDENTITY_INSERT dbo.Sizes ON;
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (1, 18.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (2, 19.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (3, 20.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (4, 21.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (5, 22.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (6, 23.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (7, 24.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (8, 25.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (9, 26.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (10, 27.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (11, 28.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (12, 29.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (13, 30.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (14, 31.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (15, 32.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (16, 33.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (17, 34.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (18, 35.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (19, 36.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (20, 36.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (21, 37.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (22, 37.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (23, 38.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (24, 38.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (25, 39.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (26, 39.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (27, 40.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (28, 40.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (29, 41.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (30, 41.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (31, 42.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (32, 42.5);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (33, 43.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (34, 44.0);
INSERT INTO dbo.Sizes (ID_Size, Size) VALUES (35, 45.0);
SET IDENTITY_INSERT dbo.Sizes OFF;
GO

SET IDENTITY_INSERT dbo.Users ON;
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (1, N'Иван', N'Иванов', N'Сергеевич', N'isivanov', 1);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (2, N'Пётр', N'Петров', N'Алексеевич', N'papetrov', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (3, N'Анна', N'Сидорова', N'Дмитриевна', N'asidorova', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (4, N'Екатерина', N'Кузнецова', N'Андреевна', N'ekuznetsova', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (5, N'Дмитрий', N'Смирнов', N'Владимирович', N'dsmirnov', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (6, N'Алексей', N'Попов', N'Николаевич', N'apopov', 1);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (7, N'Ольга', N'Васильева', N'Игоревна', N'ovasileva', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (8, N'Павел', N'Соколов', N'Викторович', N'psokolov', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (9, N'Татьяна', N'Михайлова', N'Юрьевна', N'tmikhailova', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (10, N'Наталья', N'Новикова', N'Сергеевна', N'nnovikova', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (11, N'Андрей', N'Фёдоров', N'Максимович', N'afedorov', 1);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (12, N'Сергей', N'Морозов', N'Павлович', N'smorozov', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (13, N'Юлия', N'Волкова', N'Александровна', N'jvolkova', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (14, N'Артём', N'Алексеев', N'Евгеньевич', N'aalekseev', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (15, N'Ирина', N'Лебедева', N'Владимировна', N'ilebedeva', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (16, N'Никита', N'Егоров', N'Денисович', N'negorov', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (17, N'Елена', N'Павлова', N'Петровна', N'epavlova', 3);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (18, N'Роман', N'Козлов', N'Олегович', N'rkozlov', 1);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (19, N'Виктория', N'Степанова', N'Романовна', N'vstepanova', 2);
INSERT INTO dbo.Users (ID_User, UserName, UserSurname, UserPatronymic, UserLogin, ID_Role) VALUES (20, N'Владимир', N'Николаев', N'Иванович', N'vnikolaev', 3);
SET IDENTITY_INSERT dbo.Users OFF;
GO

SET IDENTITY_INSERT dbo.Products ON;
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (1, N'Кроссовки детские «Звёздочка» экокожа', 1, 1, N'Кроссовки детские «Звёздочка» экокожа. Производитель: ООО «Малыш-Спорт», г. Смоленск', N'Верх — экокожа (полиуретан на хлопковой основе), подкладка — хлопок 100%, подошва — ПВХ', 2190.00, 1, N'IMG_KS_195217.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (2, N'Кроссовки детские «Гонщик» экокожа', 1, 2, N'Кроссовки детские «Гонщик» экокожа. Производитель: Фабрика «Топ-Топ», г. Киров', N'Верх — экокожа (микрофибра), подкладка — текстиль сетчатый (полиэстер), подошва — термопластичная резина (ТЭП)', 2490.00, 1, N'IMG_KS_195213.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (3, N'Кроссовки детские «Радуга» экокожа перфорированная', 1, 3, N'Кроссовки детские «Радуга» экокожа перфорированная. Производитель: ИП Смирнова Л.В. (бренд «Лапушка»), г. Вологда', N'Верх — экокожа перфорированная, вставки — нейлон, стелька — ортопедическая с латексной подушкой, подошва — ЭВА', 1990.00, 1, N'IMG_KS_195222.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (4, N'Сапожки детские «Снежинка» зимние на меху', 2, 4, N'Сапожки детские «Снежинка» зимние на меху. Производитель: ООО «Тёплые ножки», г. Новосибирск', N'Верх — экокожа морозостойкая, утеплитель — искусственный мех (акрил 70%, полиэстер 30%), подкладка — флис, подошва — термоэластопласт (ТЭП)', 3490.00, 1, N'IMG_KB_185501.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (5, N'Сапожки детские «Медвежонок» демисезонные непромокаемые', 2, 5, N'Сапожки детские «Медвежонок» демисезонные непромокаемые. Производитель: Компания «Северята», г. Архангельск', N'Верх — плащёвая ткань с водоотталкивающей пропиткой (полиэстер 100%), отделка — экокожа, подкладка — микрофибра, мембрана — AirTex, подошва — резина', 3190.00, 1, N'IMG_KB_164816.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (6, N'Сапожки детские «Принцесса» кожаные', 2, 6, N'Сапожки детские «Принцесса» кожаные. Производитель: Мастерская «Весна», г. Тула', N'Верх — натуральная кожа (КРС), подкладка — шерсть натуральная (70%), байка (30%), стелька — войлочная, подошва — полиуретан', 4290.00, 1, N'IMG_KB_164811.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (7, N'Сандалии детские «Пчёлка» ортопедические летние', 3, 7, N'Сандалии детские «Пчёлка» ортопедические летние. Производитель: ООО «Здоровый шаг», г. Воронеж', N'Верх — нубук натуральный, подкладка — кожа растительного дубления, стелька — анатомическая с супинатором (латекс + пробка), подошва — каучук', 2790.00, 1, N'IMG_KSd_195207.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (8, N'Сандалии детские «Морячок» текстильные с фиксатором', 3, 8, N'Сандалии детские «Морячок» текстильные с фиксатором. Производитель: ИП Кузнецов А.Д. (бренд «Карапуз»), г. Краснодар', N'Верх — джинсовая ткань (хлопок 100%), окантовка — искусственная кожа, подкладка — хлопок, подошва — ЭВА лёгкая', 1690.00, 1, N'IMG_KSd_185508.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (9, N'Туфли детские «Бантик» лаковые праздничные', 4, 9, N'Туфли детские «Бантик» лаковые праздничные. Производитель: Дом обуви «Маленький франт», г. Москва', N'Верх — искусственная лаковая кожа (ПВХ), носок усиленный — термопластичный полиуретан, подкладка — кожа искусственная дышащая, стелька — анатомическая с амортизацией, подошва — полиуретан', 2390.00, 1, N'IMG_KSho_185512.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (10, N'Сапоги зимние натуральная кожа', 2, 10, N'Сапоги женские зимние, нескользящая, устойчивая подошва, натуральный мех. Производитель: Дом обуви «Барбари», г. Новороссийск', N'Верх — натуральная кожа, подкладка — евромех, натуральный мех, текстильный утеплитель, подошва — резина', 25000.00, 2, N'IMG_WB_170905.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (11, N'Сапоги осенние натуральная кожа', 2, 10, N'Сапоги женские демисезонные из высококачественной натуральной кожи. Производитель: Дом обуви «Барбари», г. Новороссийск', N'Верх — натуральная кожа, подкладка — текстильный утеплитель, стелька — текстиль, подошва — ТПР', 15000.00, 2, N'IMG_WB_170859.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (12, N'Сапоги трубы демисезонные замшевые', 2, 11, N'Сапоги женские демисезонные замшевые. Производитель: ОАО «Твой комфорт», г. Ярославль', N'Верх — натуральная замша, утепленные, тракторная подошва, стелька — байка, подошва — резина', 17000.00, 2, N'IMG_WB_170906.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (13, N'Босоножки «Песчаный берег» коричневые', 3, 12, N'Коричневая кожаная модель с открытым носком, боковой пряжкой и закрытым каблуком. Производитель: ИП «Стиль и комфорт», г. Санкт-Петербург', N'Верх — 100% козья кожа, подкладка — 100% кожа; подошва: 100% кожа', 7500.00, 2, N'IMG_WSd_174332.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (14, N'Женские босоножки «Черный кофе» со скульптурным каблуком', 3, 13, N'Открытый носок и ремешок со сборной пряжкой, низкий каблук и резиновая подошва гарантируют комфорт. Производитель: ЗАО «Кожевенные традиции», г. Вятка', N'Верх — кожа собственного производств, подкладка — кожа; подошва: резина', 5600.00, 2, N'IMG_WSd_185156.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (15, N'Босоножки женские «Летний бриз»', 3, 14, N'Босоножки женские кожаные белые, регулируемый ремешок, закрытый каблук и кожаная подошва. Производитель: ООО «Модный силуэт», г. Кострома', N'Верх — натуральная кожа, подкладка — натуральная кожа, подошва — кожа', 7130.00, 2, N'IMG_WSd_185551.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (16, N'Демисезонные туфли натуральная кожа', 4, 14, N'Комфортные лаконичные лодочки, классический дизайна с современными акцентами. Производитель: ООО «Модный силуэт», г. Кострома', N'Верх — натуральная овечья кожа, подкладка и стелька — натуральная овечья кожа, подошва — резина', 12340.00, 2, N'IMG_WSho_174409.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (17, N'Туфли женские натуральная кожа с металлическими вставками', 4, 13, N'Стильные туфли для весеннего и летнего сезона. Производитель: ЗАО «Кожевенные традиции», г. Вятка', N'Верх — натуральная телячьей кожа, металлическая пряжка, подкладка и стелька— натуральная овечья кожа, подошва — резина', 7757.00, 2, N'IMG_WSho_185202.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (18, N'Туфли классические лодочки натуральная кожа', 4, 12, N'Модель из натуральной кожи, классический острый носок и изящная шпилька. Производитель: ИП «Стиль и комфорт», г. Санкт-Петербург', N'Верх — натуральная кожа, подкладка и стелька — натуральная кожа, подошва — тунит', 7550.00, 2, N'IMG_WSho_185545.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (19, N'Кроссовки белые с оранжевым и красным', 1, 2, N'Многослойный верх из кожи и текстиля. Производитель: Фабрика «Топ-Топ», г. Киров', N'Верх — 70% кожа, 30% текстиль, подкладка — текстиль, подошва — резина', 9450.00, 2, N'IMG_WS_174110.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (20, N'Кроссовки летние кожаные', 1, 2, N'Уверенность на каждом шагу. Материал обеспечивает долговечность и комфорт при носке. Производитель: Фабрика «Топ-Топ», г. Киров', N'Верх — 87% кожа, 11% текстиль, 2% синтетика; подкладка: 100% текстиль; подошва: резина, пластик', 8565.00, 2, N'IMG_WS_185528.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (21, N'Кроссовки кожаные', 1, 12, N'Кроссовки женские из натуральной кожи. Производитель: ИП «Стиль и комфорт», г. Санкт-Петербург', N'Верх — натуральная кожа, подкладка — кожа, подошва — полиуретан', 8765.00, 2, N'IMG_WS_185535.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (22, N'Кроссовки «Стиль и комфорт»', 1, 12, N'Кроссовки женские из натуральной кожи и синтетических материалов, сетчатые вставки. Производитель: ИП «Стиль и комфорт», г. Санкт-Петербург', N'Верх — 70% кожа, 20% полиамид, 10% полиуретан; подкладка — текстиль, подошва — ТПУ', 7795.00, 2, N'IMG_WS_185538.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (23, N'Кроссовки для бега «Драйв»', 1, 15, N'Сочетание стиля, комфорта и современных технологий. Производитель: Фабрика «Азимут», г. Санкт-Петербург', N'Верх — сетка, подкладка — текстиль, подошва — резина', 6550.00, 3, N'IMG_MS_185531.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (24, N'Кроссовки «Азимут Бег»', 1, 15, N'Универсальный выбор для повседневного использования. Производитель: Фабрика «Азимут», г. Санкт-Петербург', N'Верх — 6% кожа, 29% текстиль, 15% полимерные материалы; подкладка — 100% текстиль; подошва — полимерные материалы, резина', 7890.00, 3, N'IMG_MS_185532.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (25, N'Кроссовки', 1, 2, N'Кроссовки для мужчин, идеальное сочетание стиля и комфорта. Производитель: Фабрика «Топ-Топ», г. Киров', N'Верх — 56% кожа, 29% текстиль, 15% полимерные материалы; подкладка — 100% текстиль; подошва — полимерные материалы, резина', 9567.00, 3, N'IMG_MS_185533.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (26, N'Черные туфли в классическом стиле — база для деловых образов', 4, 10, N'Модель выполнена из натуральной кожи, на резиновой подошве. Производитель: Дом обуви «Барбари», г. Новороссийск', N'Верх — натуральная кожа, подкладка — текстиль, стелька — натуральная кожа, подошва — резина', 24569.00, 3, N'IMG_MSho_190514.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (27, N'Туфли мужские светлые классика демисезонные', 4, 16, N'Туфли на низком каблуке кожаные', N'Верх — натуральная кожа, подкладка — текстиль, стелька — натуральная кожа, подошва — ТЭП', 10155.00, 3, N'IMG_MSho_190518.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (28, N'Туфли из натуральной кожи', 4, 16, N'Модель выполнена из натуральной кожи, на резиновой подошве, поддеживает классический и деловой стиль образов. Производитель: Дом обуви «Берестов», г. Москва', N'Верх — натуральная кожа, подкладка — натуральная кожа, стелька — натуральная кожа, подошва — резина', 25625.00, 3, N'IMG_MSho_191612.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (29, N'Ботинки мужские демисезонные', 5, 10, N'Ботинки из натуральной кожи темно-коричневого и черного цвета на шнурках. Производитель: Дом обуви «Берестов», г. Москва', N'Верх — натуральная кожа, подкладка — текстиль, подошва — искусственный материал', 21500.00, 3, N'IMG_MB_174349.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (30, N'Ботинки черные из натуральной кожи', 5, 13, N'Ботинки представляют баланс традиций и современности. Выполнены из натуральной кожи насыщенного черного цвета. Производитель: ЗАО «Кожевенные традиции», г. Вятка', N'Верх — натуральная кожа, подкладка — ворсин, стелька — ворсин, подошва — тунит', 14567.00, 3, N'IMG_MB_191200.png');
INSERT INTO dbo.Products (ID_Product, ProductName, ID_Subcategory, ID_Manufacture, ProductDescription, ProductComposition, ProductCost, ID_Category, ProductImage) VALUES (31, N'Ботинки зимние классические', 5, 10, N'Выбор на каждый день — натуральная кожа, натуральный мех. Производитель: Дом обуви «Барбари», г. Новороссийск', N'Верх — натуральная кожа, подкладка — натуральный мех, стелька — натуральный мех, подошва — ТЭП', 15670.00, 3, N'IMG_MB_191205.png');
SET IDENTITY_INSERT dbo.Products OFF;
GO

SET IDENTITY_INSERT dbo.StockItems ON;
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (1, 1, 3, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (2, 1, 4, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (3, 1, 5, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (4, 1, 6, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (5, 1, 7, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (6, 1, 8, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (7, 2, 16, 2);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (8, 2, 17, 2);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (9, 3, 2, 30);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (10, 3, 3, 30);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (11, 4, 2, 15);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (12, 4, 3, 15);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (13, 5, 4, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (14, 5, 5, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (15, 5, 6, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (16, 6, 1, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (17, 6, 2, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (18, 6, 3, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (19, 7, 4, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (20, 7, 5, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (21, 7, 6, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (22, 7, 7, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (23, 8, 10, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (24, 8, 11, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (25, 8, 12, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (26, 8, 13, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (27, 9, 1, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (28, 9, 2, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (29, 9, 3, 7);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (30, 10, 19, 12);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (31, 10, 21, 12);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (32, 11, 25, 2);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (33, 11, 27, 2);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (34, 12, 21, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (35, 12, 25, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (36, 12, 27, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (37, 13, 19, 10);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (38, 13, 21, 10);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (39, 13, 23, 10);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (40, 13, 25, 10);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (41, 13, 27, 10);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (42, 14, 18, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (43, 15, 21, 23);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (44, 15, 25, 23);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (45, 15, 27, 23);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (46, 16, 19, 25);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (47, 16, 21, 25);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (48, 16, 23, 25);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (49, 17, 19, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (50, 17, 23, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (51, 17, 25, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (52, 17, 27, 4);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (53, 18, 18, 9);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (54, 18, 23, 9);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (55, 19, 18, 2);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (56, 19, 19, 2);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (57, 20, 21, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (58, 20, 22, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (59, 20, 23, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (60, 20, 24, 5);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (61, 21, 18, 16);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (62, 21, 19, 16);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (63, 21, 23, 16);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (64, 21, 25, 16);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (65, 22, 18, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (66, 22, 23, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (67, 23, 30, 18);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (68, 23, 31, 18);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (69, 23, 32, 18);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (70, 23, 33, 18);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (71, 24, 23, 50);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (72, 24, 24, 50);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (73, 24, 25, 50);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (74, 24, 26, 50);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (75, 24, 27, 50);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (76, 24, 29, 50);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (77, 25, 29, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (78, 25, 31, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (79, 25, 34, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (80, 26, 27, 23);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (81, 26, 29, 23);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (82, 26, 33, 23);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (83, 27, 31, 15);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (84, 27, 33, 15);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (85, 27, 34, 15);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (86, 28, 34, 3);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (87, 29, 29, 1);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (88, 29, 31, 1);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (89, 30, 31, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (90, 30, 33, 20);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (91, 31, 29, 30);
INSERT INTO dbo.StockItems (ID_StockItem, ID_Product, ID_Size, ItemsQuantity) VALUES (92, 31, 31, 30);
SET IDENTITY_INSERT dbo.StockItems OFF;
GO

SET IDENTITY_INSERT dbo.Orders ON;
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (1, '2026-10-14', 3, 4980.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (2, '2026-10-17', 17, 32500.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (3, '2026-10-20', 7, 33360.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (4, '2026-10-24', 15, 50992.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (5, '2026-10-30', 12, 38957.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (6, '2026-11-03', 9, 54806.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (7, '2026-11-06', 14, 10360.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (8, '2026-11-09', 20, 62395.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (9, '2026-11-16', 5, 8870.00);
INSERT INTO dbo.Orders (ID_Order, OrderDate, ID_User, TotalAmount) VALUES (10, '2026-11-19', 17, 44550.00);
SET IDENTITY_INSERT dbo.Orders OFF;
GO

SET IDENTITY_INSERT dbo.OrderProducts ON;
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (1, 1, 1, 1, 2190.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (2, 1, 20, 1, 2790.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (3, 2, 30, 1, 25000.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (4, 2, 39, 1, 7500.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (5, 3, 34, 1, 17000.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (6, 3, 57, 1, 8565.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (7, 3, 65, 1, 7795.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (8, 4, 50, 1, 7757.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (9, 4, 48, 1, 12340.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (10, 4, 44, 1, 7130.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (11, 4, 62, 1, 8765.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (12, 4, 32, 1, 15000.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (13, 5, 77, 1, 9567.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (14, 5, 87, 1, 21500.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (15, 5, 73, 1, 7890.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (16, 6, 80, 1, 24569.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (17, 6, 89, 1, 14567.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (18, 6, 91, 1, 15670.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (19, 7, 16, 1, 4290.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (20, 7, 2, 2, 2190.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (21, 7, 24, 1, 1690.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (22, 8, 86, 1, 25625.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (23, 8, 75, 3, 7890.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (24, 8, 70, 2, 6550.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (25, 9, 7, 1, 2490.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (26, 9, 14, 2, 3190.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (27, 10, 53, 1, 7550.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (28, 10, 43, 1, 7130.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (29, 10, 46, 1, 12340.0000);
INSERT INTO dbo.OrderProducts (ID_OrderItem, ID_Order, ID_StockItem, Quantity, PriceAtOrderTime) VALUES (30, 10, 61, 2, 8765.0000);
SET IDENTITY_INSERT dbo.OrderProducts OFF;
GO

/* Сброс счётчиков IDENTITY после вставки явных значений */
DBCC CHECKIDENT (N'dbo.Roles', RESEED, 3) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Categories', RESEED, 3) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Subcategories', RESEED, 5) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Manufactures', RESEED, 16) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Sizes', RESEED, 35) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Users', RESEED, 20) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Products', RESEED, 31) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.StockItems', RESEED, 92) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.Orders', RESEED, 10) WITH NO_INFOMSGS;
DBCC CHECKIDENT (N'dbo.OrderProducts', RESEED, 30) WITH NO_INFOMSGS;
GO

PRINT N'База данных KozinMiracleBoots успешно создана и заполнена.';
GO
