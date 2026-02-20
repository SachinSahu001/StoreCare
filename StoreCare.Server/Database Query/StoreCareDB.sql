-- ============================================
-- STORE CARE DATABASE - COMPLETE ENTERPRISE SQL
-- WITH INT PRIMARY KEYS FOR MASTER_TABLE AND PROPER NVARCHAR
-- Date: 12/02/2026
-- ============================================

-- Create Database
CREATE DATABASE StoreCareDB;
GO

USE StoreCareDB;
GO

-- ============================================
-- 1. MASTER TABLE (Lookup Values) - INT IDENTITY
-- ============================================
CREATE TABLE Master_Table (
    Id INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    TableValue NVARCHAR(200) NOT NULL,
    TableSequence INT DEFAULT 0,
    Item_Description NVARCHAR(500),
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1,
    CONSTRAINT UK_Master_TableName_Value UNIQUE (TableName, TableValue)
);
GO

-- ============================================
-- 2. STORE TABLE
-- ============================================
CREATE TABLE Store (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    StoreCode NVARCHAR(50) UNIQUE NOT NULL,
    StoreName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500),
    ContactNumber NVARCHAR(15),
    Email NVARCHAR(100),
    StoreLogo NVARCHAR(MAX),
    StatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

-- Foreign Key for Store
ALTER TABLE Store 
ADD CONSTRAINT FK_Store_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 3. USER TABLE (All Users)
-- ============================================
CREATE TABLE [User] (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    UserCode NVARCHAR(50) UNIQUE NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Phone NVARCHAR(15),
    PasswordHash NVARCHAR(MAX) NOT NULL,
    RoleId INT NOT NULL, -- Changed to INT to match Master_Table Id
    StoreId NVARCHAR(50),
    ProfilePicture NVARCHAR(MAX),
    StatusId INT, -- Changed to INT to match Master_Table Id
    LastLogin DATETIME,
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

-- Foreign Keys for User
ALTER TABLE [User] 
ADD CONSTRAINT FK_User_RoleId 
FOREIGN KEY (RoleId) 
REFERENCES Master_Table(Id);

ALTER TABLE [User] 
ADD CONSTRAINT FK_User_StoreId 
FOREIGN KEY (StoreId) 
REFERENCES Store(Id);

ALTER TABLE [User] 
ADD CONSTRAINT FK_User_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 4. PRODUCT CATEGORY
-- ============================================
CREATE TABLE ProductCategory (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    CategoryCode NVARCHAR(50) UNIQUE NOT NULL,
    CategoryName NVARCHAR(200) NOT NULL,
    CategoryDescription NVARCHAR(1000),
    CategoryImage NVARCHAR(MAX),
    DisplayOrder INT DEFAULT 0,
    StatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE ProductCategory 
ADD CONSTRAINT FK_ProductCategory_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 5. PRODUCT
-- ============================================
CREATE TABLE Product (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    ProductCode NVARCHAR(50) UNIQUE NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    ProductDescription NVARCHAR(MAX),
    ProductImage NVARCHAR(MAX),
    CategoryId NVARCHAR(50) NOT NULL,
    BrandName NVARCHAR(100),
    StatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE Product 
ADD CONSTRAINT FK_Product_CategoryId 
FOREIGN KEY (CategoryId) 
REFERENCES ProductCategory(Id);

ALTER TABLE Product 
ADD CONSTRAINT FK_Product_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 6. STORE PRODUCT ASSIGNMENT
-- ============================================
CREATE TABLE StoreProductAssignment (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    StoreId NVARCHAR(50) NOT NULL,
    ProductId NVARCHAR(50) NOT NULL,
    CanManage BIT DEFAULT 1,
    StatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1,
    CONSTRAINT UK_Store_Product UNIQUE (StoreId, ProductId)
);
GO

ALTER TABLE StoreProductAssignment 
ADD CONSTRAINT FK_StoreProductAssignment_StoreId 
FOREIGN KEY (StoreId) 
REFERENCES Store(Id);

ALTER TABLE StoreProductAssignment 
ADD CONSTRAINT FK_StoreProductAssignment_ProductId 
FOREIGN KEY (ProductId) 
REFERENCES Product(Id);

ALTER TABLE StoreProductAssignment 
ADD CONSTRAINT FK_StoreProductAssignment_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 7. SPECIFICATION (Dynamic Fields)
-- ============================================
CREATE TABLE Specification (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    SpecCode NVARCHAR(50) UNIQUE NOT NULL,
    SpecName NVARCHAR(200) NOT NULL,
    SpecDescription NVARCHAR(500),
    DataType NVARCHAR(20) DEFAULT 'Text',
    IsRequired BIT DEFAULT 0,
    ProductId NVARCHAR(50) NOT NULL,
    DisplayOrder INT DEFAULT 0,
    StatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE Specification 
ADD CONSTRAINT FK_Specification_ProductId 
FOREIGN KEY (ProductId) 
REFERENCES Product(Id);

ALTER TABLE Specification 
ADD CONSTRAINT FK_Specification_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 8. ITEM (Actual Store Items)
-- ============================================
CREATE TABLE Item (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    ItemCode NVARCHAR(100) UNIQUE NOT NULL,
    ItemName NVARCHAR(200) NOT NULL,
    ItemDescription NVARCHAR(MAX),
    ItemImage NVARCHAR(MAX),
    ProductId NVARCHAR(50) NOT NULL,
    StoreId NVARCHAR(50) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    TaxPercent DECIMAL(5,2) DEFAULT 0,
    StatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    IsFeatured BIT DEFAULT 0,
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE Item 
ADD CONSTRAINT FK_Item_ProductId 
FOREIGN KEY (ProductId) 
REFERENCES Product(Id);

ALTER TABLE Item 
ADD CONSTRAINT FK_Item_StoreId 
FOREIGN KEY (StoreId) 
REFERENCES Store(Id);

ALTER TABLE Item 
ADD CONSTRAINT FK_Item_StatusId 
FOREIGN KEY (StatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 9. ITEM SPECIFICATION VALUES
-- ============================================
CREATE TABLE ItemSpecificationValue (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    ItemId NVARCHAR(50) NOT NULL,
    SpecificationId NVARCHAR(50) NOT NULL,
    SpecValue NVARCHAR(MAX),
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1,
    CONSTRAINT UK_Item_Spec UNIQUE (ItemId, SpecificationId)
);
GO

ALTER TABLE ItemSpecificationValue 
ADD CONSTRAINT FK_ItemSpecificationValue_ItemId 
FOREIGN KEY (ItemId) 
REFERENCES Item(Id);

ALTER TABLE ItemSpecificationValue 
ADD CONSTRAINT FK_ItemSpecificationValue_SpecificationId 
FOREIGN KEY (SpecificationId) 
REFERENCES Specification(Id);
GO

-- ============================================
-- 10. INVENTORY (Stock Management)
-- ============================================
CREATE TABLE Inventory (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    ItemId NVARCHAR(50) NOT NULL,
    CurrentStock INT DEFAULT 0,
    MinimumStock INT DEFAULT 10,
    MaximumStock INT DEFAULT 1000,
    LastStockInDate DATETIME,
    LastStockOutDate DATETIME,
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1,
    CONSTRAINT UK_Inventory_Item UNIQUE (ItemId)
);
GO

ALTER TABLE Inventory 
ADD CONSTRAINT FK_Inventory_ItemId 
FOREIGN KEY (ItemId) 
REFERENCES Item(Id);
GO

-- ============================================
-- 11. CART
-- ============================================
CREATE TABLE Cart (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    CartSessionId NVARCHAR(100) NOT NULL,
    UserId NVARCHAR(50) NOT NULL,
    ItemId NVARCHAR(50) NOT NULL,
    Quantity INT DEFAULT 1,
    AddedDate DATETIME DEFAULT GETDATE(),
    IsCheckedOut BIT DEFAULT 0,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE Cart 
ADD CONSTRAINT FK_Cart_UserId 
FOREIGN KEY (UserId) 
REFERENCES [User](Id);

ALTER TABLE Cart 
ADD CONSTRAINT FK_Cart_ItemId 
FOREIGN KEY (ItemId) 
REFERENCES Item(Id);
GO

-- ============================================
-- 12. ORDER
-- ============================================
CREATE TABLE [Order] (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    OrderNumber NVARCHAR(50) UNIQUE NOT NULL,
    UserId NVARCHAR(50) NOT NULL,
    StoreId NVARCHAR(50) NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    NetAmount DECIMAL(18,2) NOT NULL,
    PaymentModeId INT NOT NULL, -- Changed to INT to match Master_Table Id
    PaymentStatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    OrderStatusId INT NOT NULL, -- Changed to INT to match Master_Table Id
    ShippingAddress NVARCHAR(MAX),
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE [Order] 
ADD CONSTRAINT FK_Order_UserId 
FOREIGN KEY (UserId) 
REFERENCES [User](Id);

ALTER TABLE [Order] 
ADD CONSTRAINT FK_Order_StoreId 
FOREIGN KEY (StoreId) 
REFERENCES Store(Id);

ALTER TABLE [Order] 
ADD CONSTRAINT FK_Order_PaymentModeId 
FOREIGN KEY (PaymentModeId) 
REFERENCES Master_Table(Id);

ALTER TABLE [Order] 
ADD CONSTRAINT FK_Order_PaymentStatusId 
FOREIGN KEY (PaymentStatusId) 
REFERENCES Master_Table(Id);

ALTER TABLE [Order] 
ADD CONSTRAINT FK_Order_OrderStatusId 
FOREIGN KEY (OrderStatusId) 
REFERENCES Master_Table(Id);
GO

-- ============================================
-- 13. ORDER DETAILS
-- ============================================
CREATE TABLE OrderDetail (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    OrderId NVARCHAR(50) NOT NULL,
    ItemId NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountPercent DECIMAL(5,2) DEFAULT 0,
    TotalPrice DECIMAL(18,2) NOT NULL,
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy NVARCHAR(200), -- Store Full Name
    ModifiedDate DATETIME,
    Active BIT DEFAULT 1
);
GO

ALTER TABLE OrderDetail 
ADD CONSTRAINT FK_OrderDetail_OrderId 
FOREIGN KEY (OrderId) 
REFERENCES [Order](Id);

ALTER TABLE OrderDetail 
ADD CONSTRAINT FK_OrderDetail_ItemId 
FOREIGN KEY (ItemId) 
REFERENCES Item(Id);
GO

-- ============================================
-- 14. STOCK TRANSACTION (Audit Trail)
-- ============================================
CREATE TABLE StockTransaction (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    ItemId NVARCHAR(50) NOT NULL,
    TransactionType NVARCHAR(50),
    Quantity INT NOT NULL,
    PreviousStock INT NOT NULL,
    NewStock INT NOT NULL,
    ReferenceId INT,
    Remarks NVARCHAR(500),
    TransactionDate DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(200) NOT NULL, -- Store Full Name
    CreatedDate DATETIME DEFAULT GETDATE(),
    Active BIT DEFAULT 1
);
GO

ALTER TABLE StockTransaction 
ADD CONSTRAINT FK_StockTransaction_ItemId 
FOREIGN KEY (ItemId) 
REFERENCES Item(Id);
GO

-- ============================================
-- 15. LOGIN HISTORY TABLE
-- ============================================
CREATE TABLE LoginHistory (
    Id NVARCHAR(50) PRIMARY KEY DEFAULT NEWID(),
    UserId NVARCHAR(50) NOT NULL,
    LoginTime DATETIME DEFAULT GETDATE(),
    LogoutTime DATETIME,
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(MAX),
    Browser NVARCHAR(500),
    Platform NVARCHAR(100),
    DeviceType NVARCHAR(50),
    Status NVARCHAR(50),
    FailureReason NVARCHAR(500),
    SessionId NVARCHAR(200),
    CreatedDate DATETIME DEFAULT GETDATE(),
    Active BIT DEFAULT 1
);
GO

ALTER TABLE LoginHistory 
ADD CONSTRAINT FK_LoginHistory_UserId 
FOREIGN KEY (UserId) 
REFERENCES [User](Id);
GO

-- ============================================
-- CREATE INDEXES FOR PERFORMANCE
-- ============================================

-- Master_Table Indexes
CREATE INDEX IX_MasterTable_TableName ON Master_Table(TableName);
CREATE INDEX IX_MasterTable_TableValue ON Master_Table(TableValue);
CREATE INDEX IX_MasterTable_Active ON Master_Table(Active);
GO

-- Store Indexes
CREATE INDEX IX_Store_StoreCode ON Store(StoreCode);
CREATE INDEX IX_Store_StatusId ON Store(StatusId);
CREATE INDEX IX_Store_Active ON Store(Active);
CREATE INDEX IX_Store_CreatedDate ON Store(CreatedDate);
GO

-- User Indexes
CREATE INDEX IX_User_UserCode ON [User](UserCode);
CREATE INDEX IX_User_Email ON [User](Email);
CREATE INDEX IX_User_RoleId ON [User](RoleId);
CREATE INDEX IX_User_StoreId ON [User](StoreId);
CREATE INDEX IX_User_StatusId ON [User](StatusId);
CREATE INDEX IX_User_Active ON [User](Active);
CREATE INDEX IX_User_CreatedDate ON [User](CreatedDate);
CREATE INDEX IX_User_FullName ON [User](FullName);
GO

-- ProductCategory Indexes
CREATE INDEX IX_ProductCategory_CategoryCode ON ProductCategory(CategoryCode);
CREATE INDEX IX_ProductCategory_CategoryName ON ProductCategory(CategoryName);
CREATE INDEX IX_ProductCategory_StatusId ON ProductCategory(StatusId);
CREATE INDEX IX_ProductCategory_DisplayOrder ON ProductCategory(DisplayOrder);
CREATE INDEX IX_ProductCategory_Active ON ProductCategory(Active);
GO

-- Product Indexes
CREATE INDEX IX_Product_ProductCode ON Product(ProductCode);
CREATE INDEX IX_Product_ProductName ON Product(ProductName);
CREATE INDEX IX_Product_CategoryId ON Product(CategoryId);
CREATE INDEX IX_Product_StatusId ON Product(StatusId);
CREATE INDEX IX_Product_Active ON Product(Active);
GO

-- StoreProductAssignment Indexes
CREATE INDEX IX_StoreProductAssignment_StoreId ON StoreProductAssignment(StoreId);
CREATE INDEX IX_StoreProductAssignment_ProductId ON StoreProductAssignment(ProductId);
CREATE INDEX IX_StoreProductAssignment_StatusId ON StoreProductAssignment(StatusId);
GO

-- Specification Indexes
CREATE INDEX IX_Specification_SpecCode ON Specification(SpecCode);
CREATE INDEX IX_Specification_ProductId ON Specification(ProductId);
CREATE INDEX IX_Specification_StatusId ON Specification(StatusId);
GO

-- Item Indexes
CREATE INDEX IX_Item_ItemCode ON Item(ItemCode);
CREATE INDEX IX_Item_ItemName ON Item(ItemName);
CREATE INDEX IX_Item_ProductId ON Item(ProductId);
CREATE INDEX IX_Item_StoreId ON Item(StoreId);
CREATE INDEX IX_Item_StatusId ON Item(StatusId);
CREATE INDEX IX_Item_IsFeatured ON Item(IsFeatured);
CREATE INDEX IX_Item_Price ON Item(Price);
CREATE INDEX IX_Item_Active ON Item(Active);
GO

-- ItemSpecificationValue Indexes
CREATE INDEX IX_ItemSpecValue_ItemId ON ItemSpecificationValue(ItemId);
CREATE INDEX IX_ItemSpecValue_SpecificationId ON ItemSpecificationValue(SpecificationId);
GO

-- Inventory Indexes
CREATE INDEX IX_Inventory_ItemId ON Inventory(ItemId);
CREATE INDEX IX_Inventory_CurrentStock ON Inventory(CurrentStock);
GO

-- Order Indexes
CREATE INDEX IX_Order_OrderNumber ON [Order](OrderNumber);
CREATE INDEX IX_Order_UserId ON [Order](UserId);
CREATE INDEX IX_Order_StoreId ON [Order](StoreId);
CREATE INDEX IX_Order_OrderDate ON [Order](OrderDate);
CREATE INDEX IX_Order_OrderStatusId ON [Order](OrderStatusId);
CREATE INDEX IX_Order_PaymentStatusId ON [Order](PaymentStatusId);
CREATE INDEX IX_Order_Active ON [Order](Active);
GO

-- OrderDetail Indexes
CREATE INDEX IX_OrderDetail_OrderId ON OrderDetail(OrderId);
CREATE INDEX IX_OrderDetail_ItemId ON OrderDetail(ItemId);
GO

-- Cart Indexes
CREATE INDEX IX_Cart_CartSessionId ON Cart(CartSessionId);
CREATE INDEX IX_Cart_UserId ON Cart(UserId);
CREATE INDEX IX_Cart_ItemId ON Cart(ItemId);
CREATE INDEX IX_Cart_IsCheckedOut ON Cart(IsCheckedOut);
CREATE INDEX IX_Cart_Active ON Cart(Active);
GO

-- StockTransaction Indexes
CREATE INDEX IX_StockTransaction_ItemId ON StockTransaction(ItemId);
CREATE INDEX IX_StockTransaction_TransactionDate ON StockTransaction(TransactionDate);
CREATE INDEX IX_StockTransaction_TransactionType ON StockTransaction(TransactionType);
GO

-- LoginHistory Indexes
CREATE INDEX IX_LoginHistory_UserId ON LoginHistory(UserId);
CREATE INDEX IX_LoginHistory_LoginTime ON LoginHistory(LoginTime);
CREATE INDEX IX_LoginHistory_Status ON LoginHistory(Status);
CREATE INDEX IX_LoginHistory_Active ON LoginHistory(Active);
GO

-- ============================================
-- INSERT MASTER DATA
-- ============================================

DECLARE @SystemUser NVARCHAR(200) = 'System';
DECLARE @CurrentDate DATETIME = GETDATE();

-- Insert Default Master Data - ID will auto-increment from 1
SET IDENTITY_INSERT Master_Table ON;

INSERT INTO Master_Table (Id, TableName, TableValue, TableSequence, Item_Description, CreatedBy, CreatedDate, Active) 
VALUES 
-- ROLES (1-3)
(1, 'Role', 'SuperAdmin', 1, 'System Administrator', @SystemUser, @CurrentDate, 1),
(2, 'Role', 'StoreAdmin', 2, 'Store Manager', @SystemUser, @CurrentDate, 1),
(3, 'Role', 'Customer', 3, 'End Customer', @SystemUser, @CurrentDate, 1),

-- STATUS (4-7)
(4, 'Status', 'Active', 1, 'Active Status', @SystemUser, @CurrentDate, 1),
(5, 'Status', 'Inactive', 2, 'Inactive Status', @SystemUser, @CurrentDate, 1),
(6, 'Status', 'Pending', 3, 'Pending Status', @SystemUser, @CurrentDate, 1),
(7, 'Status', 'Suspended', 4, 'Suspended Status', @SystemUser, @CurrentDate, 1),

-- ORDER STATUS (8-14)
(8, 'OrderStatus', 'Placed', 1, 'Order Placed', @SystemUser, @CurrentDate, 1),
(9, 'OrderStatus', 'Confirmed', 2, 'Order Confirmed', @SystemUser, @CurrentDate, 1),
(10, 'OrderStatus', 'Processing', 3, 'Order Processing', @SystemUser, @CurrentDate, 1),
(11, 'OrderStatus', 'Shipped', 4, 'Order Shipped', @SystemUser, @CurrentDate, 1),
(12, 'OrderStatus', 'Delivered', 5, 'Order Delivered', @SystemUser, @CurrentDate, 1),
(13, 'OrderStatus', 'Cancelled', 6, 'Order Cancelled', @SystemUser, @CurrentDate, 1),
(14, 'OrderStatus', 'Returned', 7, 'Order Returned', @SystemUser, @CurrentDate, 1),

-- PAYMENT MODE (15-20)
(15, 'PaymentMode', 'COD', 1, 'Cash on Delivery', @SystemUser, @CurrentDate, 1),
(16, 'PaymentMode', 'CreditCard', 2, 'Credit Card', @SystemUser, @CurrentDate, 1),
(17, 'PaymentMode', 'DebitCard', 3, 'Debit Card', @SystemUser, @CurrentDate, 1),
(18, 'PaymentMode', 'UPI', 4, 'Unified Payments Interface', @SystemUser, @CurrentDate, 1),
(19, 'PaymentMode', 'NetBanking', 5, 'Net Banking', @SystemUser, @CurrentDate, 1),
(20, 'PaymentMode', 'Wallet', 6, 'Digital Wallet', @SystemUser, @CurrentDate, 1),

-- ITEM STATUS (21-24)
(21, 'ItemStatus', 'Available', 1, 'Item Available', @SystemUser, @CurrentDate, 1),
(22, 'ItemStatus', 'OutOfStock', 2, 'Item Out of Stock', @SystemUser, @CurrentDate, 1),
(23, 'ItemStatus', 'Discontinued', 3, 'Item Discontinued', @SystemUser, @CurrentDate, 1),
(24, 'ItemStatus', 'LowStock', 4, 'Item Low Stock', @SystemUser, @CurrentDate, 1),

-- PAYMENT STATUS (25-29)
(25, 'PaymentStatus', 'Pending', 1, 'Payment Pending', @SystemUser, @CurrentDate, 1),
(26, 'PaymentStatus', 'Paid', 2, 'Payment Completed', @SystemUser, @CurrentDate, 1),
(27, 'PaymentStatus', 'Failed', 3, 'Payment Failed', @SystemUser, @CurrentDate, 1),
(28, 'PaymentStatus', 'Refunded', 4, 'Payment Refunded', @SystemUser, @CurrentDate, 1),
(29, 'PaymentStatus', 'PartiallyRefunded', 5, 'Payment Partially Refunded', @SystemUser, @CurrentDate, 1),

-- TRANSACTION TYPE (30-33)
(30, 'TransactionType', 'StockIn', 1, 'Stock Inward', @SystemUser, @CurrentDate, 1),
(31, 'TransactionType', 'StockOut', 2, 'Stock Outward', @SystemUser, @CurrentDate, 1),
(32, 'TransactionType', 'Adjustment', 3, 'Stock Adjustment', @SystemUser, @CurrentDate, 1),
(33, 'TransactionType', 'Return', 4, 'Stock Return', @SystemUser, @CurrentDate, 1),

-- DATA TYPE FOR SPECIFICATIONS (34-38)
(34, 'DataType', 'Text', 1, 'Text Field', @SystemUser, @CurrentDate, 1),
(35, 'DataType', 'Number', 2, 'Numeric Field', @SystemUser, @CurrentDate, 1),
(36, 'DataType', 'Boolean', 3, 'Yes/No Field', @SystemUser, @CurrentDate, 1),
(37, 'DataType', 'Date', 4, 'Date Field', @SystemUser, @CurrentDate, 1),
(38, 'DataType', 'Dropdown', 5, 'Dropdown Field', @SystemUser, @CurrentDate, 1);

SET IDENTITY_INSERT Master_Table OFF;
GO

-- Date: 13/02/2026

-- Add columns to Store table for better tracking
ALTER TABLE Store
ADD 
    GSTNumber NVARCHAR(50),
    PANNumber NVARCHAR(50),
    ContactPersonName NVARCHAR(200),
    ContactPersonPhone NVARCHAR(15),
    ContactPersonEmail NVARCHAR(100),
    LicenseNumber NVARCHAR(100),
    EstablishedDate DATE;
GO

-- Add index columns to Product table
ALTER TABLE Product
ADD 
    IsFeatured BIT DEFAULT 0,
    ViewCount INT DEFAULT 0;
GO

-- Add index columns to ProductCategory table
ALTER TABLE ProductCategory
ADD 
    IsPopular BIT DEFAULT 0,
    IconClass NVARCHAR(100);
GO