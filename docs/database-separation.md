# Tach database cho tung microservice

## Muc tieu
- Moi service chi truy cap va so huu schema rieng.
- Giam phu thuoc chéo, chuan bi cho phan tac event-driven/queuing.

## Ten database de xuat
| Service         | DbContext               | Ten database goi y |
|-----------------|------------------------|---------------------|
| AccountAPI      | `AccountDbContext`      | `DOSAccountDb`      |
| CartAPI         | `CartDbContext`         | `DOSCartDb`         |
| CategoriesAPI   | `CatalogDbContext`      | `DOSCatalogDb`      |
| OrderAPI        | `OrderDbContext`        | `DOSOrderDb`        |
| PaymentAPI      | `PaymentDbContext`      | `DOSPaymentDb`      |
| FeedbackAPI     | `DosfeedbackDbContext`  | `DOSFeedbackDb`     |

## Tao database & schema co ban
Thuc thi cac doan SQL sau trong SQL Server Management Studio (hoac Azure Data Studio). Co the chay tung khoi phan theo nhu cau.

```sql
-- 1. Account service
IF DB_ID('DOSAccountDb') IS NULL
    CREATE DATABASE DOSAccountDb;
GO
USE DOSAccountDb;
GO
IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserID       INT IDENTITY(1,1) PRIMARY KEY,
        Username     NVARCHAR(50)  NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        FullName     NVARCHAR(100) NULL,
        Email        NVARCHAR(100) NULL,
        Phone        NVARCHAR(20)  NULL,
        Role         NVARCHAR(20)  NOT NULL,
        IsBanned     BIT           NOT NULL CONSTRAINT DF_Users_IsBanned DEFAULT(0),
        AvatarUrl    NVARCHAR(255) NULL,
        CreatedAt    DATETIME      NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt    DATETIME      NULL
    );
END
GO

-- 2. Cart service
IF DB_ID('DOSCartDb') IS NULL
    CREATE DATABASE DOSCartDb;
GO
USE DOSCartDb;
GO
IF OBJECT_ID('dbo.Carts','U') IS NULL
BEGIN
    CREATE TABLE dbo.Carts (
        CartID    INT IDENTITY(1,1) PRIMARY KEY,
        UserID    INT          NOT NULL,
        CreatedAt DATETIME     NOT NULL CONSTRAINT DF_Carts_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt DATETIME     NULL
    );
END
IF OBJECT_ID('dbo.CartItems','U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems (
        CartItemID INT IDENTITY(1,1) PRIMARY KEY,
        CartID     INT          NOT NULL,
        ProductID  INT          NOT NULL,
        Quantity   INT          NOT NULL,
        CreatedAt  DATETIME     NOT NULL CONSTRAINT DF_CartItems_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt  DATETIME     NULL,
        CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartID) REFERENCES dbo.Carts(CartID)
    );
END
GO

-- 3. Catalog service (Categories + Products)
IF DB_ID('DOSCatalogDb') IS NULL
    CREATE DATABASE DOSCatalogDb;
GO
USE DOSCatalogDb;
GO
IF OBJECT_ID('dbo.Categories','U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories (
        CategoryID   INT IDENTITY(1,1) PRIMARY KEY,
        CategoryName NVARCHAR(100) NOT NULL,
        Description  NVARCHAR(255) NULL,
        CreatedAt    DATETIME      NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt    DATETIME      NULL
    );
END
IF OBJECT_ID('dbo.Products','U') IS NULL
BEGIN
    CREATE TABLE dbo.Products (
        ProductID   INT IDENTITY(1,1) PRIMARY KEY,
        CategoryID  INT          NOT NULL,
        ProductName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(255) NULL,
        ImageUrl    NVARCHAR(255) NULL,
        Price       DECIMAL(10,2) NOT NULL,
        Stock       INT           NULL,
        CreatedAt   DATETIME      NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt   DATETIME      NULL,
        CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryID) REFERENCES dbo.Categories(CategoryID)
    );
END
GO

-- 4. Order service
IF DB_ID('DOSOrderDb') IS NULL
    CREATE DATABASE DOSOrderDb;
GO
USE DOSOrderDb;
GO
IF OBJECT_ID('dbo.Orders','U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders (
        OrderID       INT IDENTITY(1,1) PRIMARY KEY,
        UserID        INT           NOT NULL,
        OrderStatus   NVARCHAR(20)  NULL,
        OrderDate     DATETIME      NOT NULL CONSTRAINT DF_Orders_OrderDate DEFAULT(GETDATE()),
        TotalAmount   DECIMAL(10,2) NULL,
        CancelReason  NVARCHAR(255) NULL,
        CreatedAt     DATETIME      NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt     DATETIME      NULL
    );
END
IF OBJECT_ID('dbo.OrderItems','U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
        OrderID     INT           NOT NULL,
        ProductID   INT           NOT NULL,
        Quantity    INT           NOT NULL,
        UnitPrice   DECIMAL(10,2) NOT NULL,
        CreatedAt   DATETIME      NOT NULL CONSTRAINT DF_OrderItems_CreatedAt DEFAULT(GETDATE()),
        UpdatedAt   DATETIME      NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderID) REFERENCES dbo.Orders(OrderID)
    );
END
GO

-- 5. Payment service
IF DB_ID('DOSPaymentDb') IS NULL
    CREATE DATABASE DOSPaymentDb;
GO
USE DOSPaymentDb;
GO
IF OBJECT_ID('dbo.Payments','U') IS NULL
BEGIN
    CREATE TABLE dbo.Payments (
        PaymentID     INT IDENTITY(1,1) PRIMARY KEY,
        OrderID       INT           NOT NULL,
        PaidAmount    DECIMAL(10,2) NOT NULL,
        PaymentMethod NVARCHAR(50)  NULL,
        PaymentDate   DATETIME      NULL CONSTRAINT DF_Payments_PaymentDate DEFAULT(GETDATE()),
        PaymentStatus NVARCHAR(20)  NULL CONSTRAINT DF_Payments_Status DEFAULT('pending')
    );
END
GO

-- 6. Feedback service
IF DB_ID('DOSFeedbackDb') IS NULL
    CREATE DATABASE DOSFeedbackDb;
GO
USE DOSFeedbackDb;
GO
IF OBJECT_ID('dbo.Feedbacks','U') IS NULL
BEGIN
    CREATE TABLE dbo.Feedbacks (
        FeedbackID   INT IDENTITY(1,1) PRIMARY KEY,
        OrderID      INT           NOT NULL,
        Rating       INT           NOT NULL,
        Comment      NVARCHAR(1000) NULL,
        FeedbackDate DATETIME      NULL CONSTRAINT DF_Feedbacks_FeedbackDate DEFAULT(GETDATE()),
        CONSTRAINT UQ_Feedbacks_Order UNIQUE (OrderID)
    );
END
GO
```

> Luu y: cac cot tham chieu sang service khac (`UserID`, `ProductID`, `OrderID`) duoc giu dang INT thuong, khong tao foreign key sang database khac de dam bao service doc lap.

## Copy du lieu tu database cu
Su dung script sau (chay tung khoi). Bao dam tat ca database moi khong co du lieu trung truoc khi chay.

```sql
-- Account data
SET IDENTITY_INSERT DOSAccountDb.dbo.Users ON;
INSERT INTO DOSAccountDb.dbo.Users (UserID, Username, PasswordHash, FullName, Email, Phone, Role, IsBanned, AvatarUrl, CreatedAt, UpdatedAt)
SELECT UserID, Username, PasswordHash, FullName, Email, Phone, Role, ISNULL(IsBanned, 0), AvatarUrl, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.Users;
SET IDENTITY_INSERT DOSAccountDb.dbo.Users OFF;
GO

-- Cart data
SET IDENTITY_INSERT DOSCartDb.dbo.Carts ON;
INSERT INTO DOSCartDb.dbo.Carts (CartID, UserID, CreatedAt, UpdatedAt)
SELECT CartID, UserID, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.Carts;
SET IDENTITY_INSERT DOSCartDb.dbo.Carts OFF;
GO
SET IDENTITY_INSERT DOSCartDb.dbo.CartItems ON;
INSERT INTO DOSCartDb.dbo.CartItems (CartItemID, CartID, ProductID, Quantity, CreatedAt, UpdatedAt)
SELECT CartItemID, CartID, ProductID, Quantity, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.CartItems;
SET IDENTITY_INSERT DOSCartDb.dbo.CartItems OFF;
GO

-- Catalog data
SET IDENTITY_INSERT DOSCatalogDb.dbo.Categories ON;
INSERT INTO DOSCatalogDb.dbo.Categories (CategoryID, CategoryName, Description, CreatedAt, UpdatedAt)
SELECT CategoryID, CategoryName, Description, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.Categories;
SET IDENTITY_INSERT DOSCatalogDb.dbo.Categories OFF;
GO
SET IDENTITY_INSERT DOSCatalogDb.dbo.Products ON;
INSERT INTO DOSCatalogDb.dbo.Products (ProductID, CategoryID, ProductName, Description, ImageUrl, Price, Stock, CreatedAt, UpdatedAt)
SELECT ProductID, CategoryID, ProductName, Description, ImageUrl, Price, Stock, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.Products;
SET IDENTITY_INSERT DOSCatalogDb.dbo.Products OFF;
GO

-- Order data
SET IDENTITY_INSERT DOSOrderDb.dbo.Orders ON;
INSERT INTO DOSOrderDb.dbo.Orders (OrderID, UserID, OrderStatus, OrderDate, TotalAmount, CancelReason, CreatedAt, UpdatedAt)
SELECT OrderID, UserID, OrderStatus, OrderDate, TotalAmount, CancelReason, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.Orders;
SET IDENTITY_INSERT DOSOrderDb.dbo.Orders OFF;
GO
SET IDENTITY_INSERT DOSOrderDb.dbo.OrderItems ON;
INSERT INTO DOSOrderDb.dbo.OrderItems (OrderItemID, OrderID, ProductID, Quantity, UnitPrice, CreatedAt, UpdatedAt)
SELECT OrderItemID, OrderID, ProductID, Quantity, UnitPrice, CreatedAt, UpdatedAt
FROM DrinkOrderDB.dbo.OrderItems;
SET IDENTITY_INSERT DOSOrderDb.dbo.OrderItems OFF;
GO

-- Payment data
SET IDENTITY_INSERT DOSPaymentDb.dbo.Payments ON;
INSERT INTO DOSPaymentDb.dbo.Payments (PaymentID, OrderID, PaidAmount, PaymentMethod, PaymentDate, PaymentStatus)
SELECT PaymentID, OrderID, PaidAmount, PaymentMethod, PaymentDate, PaymentStatus
FROM DrinkOrderDB.dbo.Payments;
SET IDENTITY_INSERT DOSPaymentDb.dbo.Payments OFF;
GO

-- Feedback data
SET IDENTITY_INSERT DOSFeedbackDb.dbo.Feedbacks ON;
INSERT INTO DOSFeedbackDb.dbo.Feedbacks (FeedbackID, OrderID, Rating, Comment, FeedbackDate)
SELECT FeedbackID, OrderID, Rating, Comment, FeedbackDate
FROM DrinkOrderDB.dbo.Feedbacks;
SET IDENTITY_INSERT DOSFeedbackDb.dbo.Feedbacks OFF;
GO
```

> Neu co bang nao khong ton tai trong database cu thi bo qua khoi insert tuong ung.

## Cac buoc tiep theo de an toan du lieu
1. Cap nhat chuoi ket noi (da commit trong repo) cho phu hop moi moi truong (DEV/STG/PROD).
2. Back up database cu truoc khi tach.
3. Sau khi tach, khoa quyen ghi tren `DrinkOrderDB` hoac doi ten de dam bao ung dung khong vo tinh ket noi lai.
4. Thiet lap co che dong bo (event/queue) neu co truong hop phan tan can thong bao giua cac service.
5. Can nhac them migrations rieng cho tung service (`dotnet ef migrations add InitialCreate`) de tien cho viec evolve schema doc lap trong tuong lai.

## Kiem tra lai ung dung
- Chay tung service voi database moi, thuc hien loat tinh nang quan trong (dang ky/dang nhap, dat hang, gio hang, thanh toan) de chac chan du lieu van dong bo qua cac API.
- Gateways va MVCApplication khong can doi DB nhung can cap nhat lai BASE_URL/port neu service thay doi.

## Huong dan rollback nhanh
- Neu can quay lai DB cu, chi can chinh lai `appsettings.json` ve `DrinkOrderDB` va khoi dong lai service.
- Luon giu script tao DB moi de co the rebuild khi can.


## Xu ly loi thieu cot Stock
Neu log bao Invalid column name 'Stock' khi goi CategoriesAPI, chay doan SQL sau tren tung database chua bang Products (bao gom DrinkOrderDB cu va DOSCatalogDb moi) de bo sung cot bi thieu:

`sql
IF COL_LENGTH('dbo.Products', 'Stock') IS NULL
BEGIN
    ALTER TABLE dbo.Products
    ADD Stock INT NULL
        CONSTRAINT DF_Products_Stock DEFAULT(0);
END
`

Sau khi cap nhat, restart CategoriesAPI (va Gateway/MVC neu can) de EF nap lai schema.
