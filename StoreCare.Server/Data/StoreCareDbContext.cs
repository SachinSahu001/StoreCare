using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StoreCare.Server.Models;

namespace StoreCare.Server.Data;

public partial class StoreCareDbContext : DbContext
{
    public StoreCareDbContext()
    {
    }

    public StoreCareDbContext(DbContextOptions<StoreCareDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<ItemSpecificationValue> ItemSpecificationValues { get; set; }

    public virtual DbSet<LoginHistory> LoginHistories { get; set; }

    public virtual DbSet<MasterTable> MasterTables { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductCategory> ProductCategories { get; set; }

    public virtual DbSet<Specification> Specifications { get; set; }

    public virtual DbSet<StockTransaction> StockTransactions { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StoreProductAssignment> StoreProductAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=DESKTOP-LQ65V9C;Database=StoreCareDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cart__3214EC071F24EC33");

            entity.ToTable("Cart");

            entity.HasIndex(e => e.Active, "IX_Cart_Active");

            entity.HasIndex(e => e.CartSessionId, "IX_Cart_CartSessionId");

            entity.HasIndex(e => e.IsCheckedOut, "IX_Cart_IsCheckedOut");

            entity.HasIndex(e => e.ItemId, "IX_Cart_ItemId");

            entity.HasIndex(e => e.UserId, "IX_Cart_UserId");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CartSessionId).HasMaxLength(100);
            entity.Property(e => e.IsCheckedOut).HasDefaultValue(false);
            entity.Property(e => e.ItemId).HasMaxLength(50);
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.Carts)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_ItemId");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cart_UserId");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__3214EC07267FB4F1");

            entity.ToTable("Inventory");

            entity.HasIndex(e => e.CurrentStock, "IX_Inventory_CurrentStock");

            entity.HasIndex(e => e.ItemId, "IX_Inventory_ItemId");

            entity.HasIndex(e => e.ItemId, "UK_Inventory_Item").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CurrentStock).HasDefaultValue(0);
            entity.Property(e => e.ItemId).HasMaxLength(50);
            entity.Property(e => e.LastStockInDate).HasColumnType("datetime");
            entity.Property(e => e.LastStockOutDate).HasColumnType("datetime");
            entity.Property(e => e.MaximumStock).HasDefaultValue(1000);
            entity.Property(e => e.MinimumStock).HasDefaultValue(10);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Item).WithOne(p => p.Inventory)
                .HasForeignKey<Inventory>(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Inventory_ItemId");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Item__3214EC07E7BF253C");

            entity.ToTable("Item");

            entity.HasIndex(e => e.Active, "IX_Item_Active");

            entity.HasIndex(e => e.IsFeatured, "IX_Item_IsFeatured");

            entity.HasIndex(e => e.ItemCode, "IX_Item_ItemCode");

            entity.HasIndex(e => e.ItemName, "IX_Item_ItemName");

            entity.HasIndex(e => e.Price, "IX_Item_Price");

            entity.HasIndex(e => e.ProductId, "IX_Item_ProductId");

            entity.HasIndex(e => e.StatusId, "IX_Item_StatusId");

            entity.HasIndex(e => e.StoreId, "IX_Item_StoreId");

            entity.HasIndex(e => e.ItemCode, "UQ__Item__3ECC0FEAD1228037").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountPercent)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.ItemCode).HasMaxLength(100);
            entity.Property(e => e.ItemName).HasMaxLength(200);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ProductId).HasMaxLength(50);
            entity.Property(e => e.StoreId).HasMaxLength(50);
            entity.Property(e => e.TaxPercent)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.Items)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_ProductId");

            entity.HasOne(d => d.Status).WithMany(p => p.Items)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_StatusId");

            entity.HasOne(d => d.Store).WithMany(p => p.Items)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item_StoreId");
        });

        modelBuilder.Entity<ItemSpecificationValue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ItemSpec__3214EC072B5A422D");

            entity.ToTable("ItemSpecificationValue");

            entity.HasIndex(e => e.ItemId, "IX_ItemSpecValue_ItemId");

            entity.HasIndex(e => e.SpecificationId, "IX_ItemSpecValue_SpecificationId");

            entity.HasIndex(e => new { e.ItemId, e.SpecificationId }, "UK_Item_Spec").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasMaxLength(50);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.SpecificationId).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.ItemSpecificationValues)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemSpecificationValue_ItemId");

            entity.HasOne(d => d.Specification).WithMany(p => p.ItemSpecificationValues)
                .HasForeignKey(d => d.SpecificationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItemSpecificationValue_SpecificationId");
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoginHis__3214EC07EE7CD8A1");

            entity.ToTable("LoginHistory");

            entity.HasIndex(e => e.Active, "IX_LoginHistory_Active");

            entity.HasIndex(e => e.LoginTime, "IX_LoginHistory_LoginTime");

            entity.HasIndex(e => e.Status, "IX_LoginHistory_Status");

            entity.HasIndex(e => e.UserId, "IX_LoginHistory_UserId");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Browser).HasMaxLength(500);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DeviceType).HasMaxLength(50);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.LoginTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.LogoutTime).HasColumnType("datetime");
            entity.Property(e => e.Platform).HasMaxLength(100);
            entity.Property(e => e.SessionId).HasMaxLength(200);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.User).WithMany(p => p.LoginHistories)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LoginHistory_UserId");
        });

        modelBuilder.Entity<MasterTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Master_T__3214EC079EC6DA5D");

            entity.ToTable("Master_Table");

            entity.HasIndex(e => e.Active, "IX_MasterTable_Active");

            entity.HasIndex(e => e.TableName, "IX_MasterTable_TableName");

            entity.HasIndex(e => e.TableValue, "IX_MasterTable_TableValue");

            entity.HasIndex(e => new { e.TableName, e.TableValue }, "UK_Master_TableName_Value").IsUnique();

            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemDescription)
                .HasMaxLength(500)
                .HasColumnName("Item_Description");
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.TableName).HasMaxLength(100);
            entity.Property(e => e.TableSequence).HasDefaultValue(0);
            entity.Property(e => e.TableValue).HasMaxLength(200);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Order__3214EC079306CC59");

            entity.ToTable("Order");

            entity.HasIndex(e => e.Active, "IX_Order_Active");

            entity.HasIndex(e => e.OrderDate, "IX_Order_OrderDate");

            entity.HasIndex(e => e.OrderNumber, "IX_Order_OrderNumber");

            entity.HasIndex(e => e.OrderStatusId, "IX_Order_OrderStatusId");

            entity.HasIndex(e => e.PaymentStatusId, "IX_Order_PaymentStatusId");

            entity.HasIndex(e => e.StoreId, "IX_Order_StoreId");

            entity.HasIndex(e => e.UserId, "IX_Order_UserId");

            entity.HasIndex(e => e.OrderNumber, "UQ__Order__CAC5E7433B542882").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NetAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.OrderNumber).HasMaxLength(50);
            entity.Property(e => e.StoreId).HasMaxLength(50);
            entity.Property(e => e.TaxAmount)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UserId).HasMaxLength(50);

            entity.HasOne(d => d.OrderStatus).WithMany(p => p.OrderOrderStatuses)
                .HasForeignKey(d => d.OrderStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_OrderStatusId");

            entity.HasOne(d => d.PaymentMode).WithMany(p => p.OrderPaymentModes)
                .HasForeignKey(d => d.PaymentModeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_PaymentModeId");

            entity.HasOne(d => d.PaymentStatus).WithMany(p => p.OrderPaymentStatuses)
                .HasForeignKey(d => d.PaymentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_PaymentStatusId");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_StoreId");

            entity.HasOne(d => d.User).WithMany(p => p.Orders)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_UserId");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__OrderDet__3214EC0751947F4C");

            entity.ToTable("OrderDetail");

            entity.HasIndex(e => e.ItemId, "IX_OrderDetail_ItemId");

            entity.HasIndex(e => e.OrderId, "IX_OrderDetail_OrderId");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountPercent)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.ItemId).HasMaxLength(50);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.OrderId).HasMaxLength(50);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Item).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_ItemId");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OrderDetail_OrderId");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Product__3214EC07A12FE780");

            entity.ToTable("Product");

            entity.HasIndex(e => e.Active, "IX_Product_Active");

            entity.HasIndex(e => e.CategoryId, "IX_Product_CategoryId");

            entity.HasIndex(e => e.ProductCode, "IX_Product_ProductCode");

            entity.HasIndex(e => e.ProductName, "IX_Product_ProductName");

            entity.HasIndex(e => e.StatusId, "IX_Product_StatusId");

            entity.HasIndex(e => e.ProductCode, "UQ__Product__2F4E024F9CFA2972").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.BrandName).HasMaxLength(100);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_CategoryId");

            entity.HasOne(d => d.Status).WithMany(p => p.Products)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Product_StatusId");
        });

        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductC__3214EC07D0AB5153");

            entity.ToTable("ProductCategory");

            entity.HasIndex(e => e.Active, "IX_ProductCategory_Active");

            entity.HasIndex(e => e.CategoryCode, "IX_ProductCategory_CategoryCode");

            entity.HasIndex(e => e.CategoryName, "IX_ProductCategory_CategoryName");

            entity.HasIndex(e => e.DisplayOrder, "IX_ProductCategory_DisplayOrder");

            entity.HasIndex(e => e.StatusId, "IX_ProductCategory_StatusId");

            entity.HasIndex(e => e.CategoryCode, "UQ__ProductC__371BA955F532EB82").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CategoryCode).HasMaxLength(50);
            entity.Property(e => e.CategoryDescription).HasMaxLength(1000);
            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IconClass).HasMaxLength(100);
            entity.Property(e => e.IsPopular).HasDefaultValue(false);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

            entity.HasOne(d => d.Status).WithMany(p => p.ProductCategories)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductCategory_StatusId");
        });

        modelBuilder.Entity<Specification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Specific__3214EC07A177B2B0");

            entity.ToTable("Specification");

            entity.HasIndex(e => e.ProductId, "IX_Specification_ProductId");

            entity.HasIndex(e => e.SpecCode, "IX_Specification_SpecCode");

            entity.HasIndex(e => e.StatusId, "IX_Specification_StatusId");

            entity.HasIndex(e => e.SpecCode, "UQ__Specific__BB4FDCAD2F98D466").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DataType)
                .HasMaxLength(20)
                .HasDefaultValue("Text");
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsRequired).HasDefaultValue(false);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasMaxLength(50);
            entity.Property(e => e.SpecCode).HasMaxLength(50);
            entity.Property(e => e.SpecDescription).HasMaxLength(500);
            entity.Property(e => e.SpecName).HasMaxLength(200);

            entity.HasOne(d => d.Product).WithMany(p => p.Specifications)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Specification_ProductId");

            entity.HasOne(d => d.Status).WithMany(p => p.Specifications)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Specification_StatusId");
        });

        modelBuilder.Entity<StockTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StockTra__3214EC07FFA64CC8");

            entity.ToTable("StockTransaction");

            entity.HasIndex(e => e.ItemId, "IX_StockTransaction_ItemId");

            entity.HasIndex(e => e.TransactionDate, "IX_StockTransaction_TransactionDate");

            entity.HasIndex(e => e.TransactionType, "IX_StockTransaction_TransactionType");

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ItemId).HasMaxLength(50);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Item).WithMany(p => p.StockTransactions)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransaction_ItemId");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3214EC073EDB19DF");

            entity.ToTable("Store");

            entity.HasIndex(e => e.Active, "IX_Store_Active");

            entity.HasIndex(e => e.CreatedDate, "IX_Store_CreatedDate");

            entity.HasIndex(e => e.StatusId, "IX_Store_StatusId");

            entity.HasIndex(e => e.StoreCode, "IX_Store_StoreCode");

            entity.HasIndex(e => e.StoreCode, "UQ__Store__02A384F89F7EFD3F").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ContactNumber).HasMaxLength(15);
            entity.Property(e => e.ContactPersonEmail).HasMaxLength(100);
            entity.Property(e => e.ContactPersonName).HasMaxLength(200);
            entity.Property(e => e.ContactPersonPhone).HasMaxLength(15);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Gstnumber)
                .HasMaxLength(50)
                .HasColumnName("GSTNumber");
            entity.Property(e => e.LicenseNumber).HasMaxLength(100);
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Pannumber)
                .HasMaxLength(50)
                .HasColumnName("PANNumber");
            entity.Property(e => e.StoreCode).HasMaxLength(50);
            entity.Property(e => e.StoreName).HasMaxLength(200);

            entity.HasOne(d => d.Status).WithMany(p => p.Stores)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Store_StatusId");
        });

        modelBuilder.Entity<StoreProductAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__StorePro__3214EC07CE97838D");

            entity.ToTable("StoreProductAssignment");

            entity.HasIndex(e => e.ProductId, "IX_StoreProductAssignment_ProductId");

            entity.HasIndex(e => e.StatusId, "IX_StoreProductAssignment_StatusId");

            entity.HasIndex(e => e.StoreId, "IX_StoreProductAssignment_StoreId");

            entity.HasIndex(e => new { e.StoreId, e.ProductId }, "UK_Store_Product").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CanManage).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.ProductId).HasMaxLength(50);
            entity.Property(e => e.StoreId).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.StoreProductAssignments)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreProductAssignment_ProductId");

            entity.HasOne(d => d.Status).WithMany(p => p.StoreProductAssignments)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreProductAssignment_StatusId");

            entity.HasOne(d => d.Store).WithMany(p => p.StoreProductAssignments)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StoreProductAssignment_StoreId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__User__3214EC07C98C0AAC");

            entity.ToTable("User");

            entity.HasIndex(e => e.Active, "IX_User_Active");

            entity.HasIndex(e => e.CreatedDate, "IX_User_CreatedDate");

            entity.HasIndex(e => e.Email, "IX_User_Email");

            entity.HasIndex(e => e.FullName, "IX_User_FullName");

            entity.HasIndex(e => e.RoleId, "IX_User_RoleId");

            entity.HasIndex(e => e.StatusId, "IX_User_StatusId");

            entity.HasIndex(e => e.StoreId, "IX_User_StoreId");

            entity.HasIndex(e => e.UserCode, "IX_User_UserCode");

            entity.HasIndex(e => e.UserCode, "UQ__User__1DF52D0CC94454BA").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534C3831BC3").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(50)
                .HasDefaultValueSql("(newid())");
            entity.Property(e => e.Active).HasDefaultValue(true);
            entity.Property(e => e.CreatedBy).HasMaxLength(200);
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.LastLogin).HasColumnType("datetime");
            entity.Property(e => e.ModifiedBy).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.StoreId).HasMaxLength(50);
            entity.Property(e => e.UserCode).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_RoleId");

            entity.HasOne(d => d.Status).WithMany(p => p.UserStatuses)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_User_StatusId");

            entity.HasOne(d => d.Store).WithMany(p => p.Users)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK_User_StoreId");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
