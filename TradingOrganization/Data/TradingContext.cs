using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace TradingOrganization
{
    /// <summary>
    /// Контекст Entity Framework для базы данных TradingOrganization.
    /// Схема БД создаётся вручную (SQL-скриптом), поэтому инициализатор отключён.
    /// </summary>
    public class TradingContext : DbContext
    {
        static TradingContext()
        {
            Database.SetInitializer<TradingContext>(null);
        }

        public TradingContext()
            : base("name=TradingContext")
        {
        }

        /// <summary>
        /// Контекст с явно заданной строкой подключения
        /// (используется диагностикой и тестами).
        /// </summary>
        public TradingContext(string connectionString)
            : base(connectionString)
        {
        }

        public virtual DbSet<Position> Positions { get; set; }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<Organization> Organizations { get; set; }

        public virtual DbSet<Store> Stores { get; set; }

        public virtual DbSet<Supplier> Suppliers { get; set; }

        public virtual DbSet<Order> Orders { get; set; }

        public virtual DbSet<ChangeRequest> ChangeRequests { get; set; }

        public virtual DbSet<OrderRequest> OrderRequests { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder.Entity<Position>()
                .ToTable("Positions")
                .HasKey(p => p.PositionID);
            modelBuilder.Entity<Position>()
                .Property(p => p.Name).IsRequired().HasMaxLength(50);

            modelBuilder.Entity<User>()
                .ToTable("Users")
                .HasKey(u => u.UserID);
            modelBuilder.Entity<User>()
                .Property(u => u.LastName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.FirstName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Patronymic).HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Login).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<User>()
                .Property(u => u.Password).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<User>()
                .Property(u => u.Gender).HasMaxLength(1);
            modelBuilder.Entity<User>()
                .Property(u => u.City).HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Address).HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Photo).HasMaxLength(100);
            modelBuilder.Entity<User>()
                .Property(u => u.Role).IsRequired().HasMaxLength(20);

            modelBuilder.Entity<Organization>()
                .ToTable("Organizations")
                .HasKey(o => o.OrganizationID);
            modelBuilder.Entity<Organization>()
                .Property(o => o.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Organization>()
                .Property(o => o.Address).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<Store>()
                .ToTable("Stores")
                .HasKey(s => s.StoreID);
            modelBuilder.Entity<Store>()
                .Property(s => s.Type).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Store>()
                .Property(s => s.Address).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<Supplier>()
                .ToTable("Suppliers")
                .HasKey(s => s.SupplierID);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.ActivityType).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Country).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.City).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Supplier>()
                .Property(s => s.Address).IsRequired().HasMaxLength(100);

            modelBuilder.Entity<Order>()
                .ToTable("Orders")
                .HasKey(o => o.OrderID);
            modelBuilder.Entity<Order>()
                .Property(o => o.ProductName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Order>()
                .Property(o => o.Quantity).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Order>()
                .Property(o => o.Price).HasPrecision(18, 2);

            modelBuilder.Entity<ChangeRequest>()
                .ToTable("ChangeRequests")
                .HasKey(r => r.RequestID);
            modelBuilder.Entity<ChangeRequest>()
                .Property(r => r.Message).IsRequired().HasMaxLength(1000);
            modelBuilder.Entity<ChangeRequest>()
                .Property(r => r.Status).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<ChangeRequest>()
                .Property(r => r.ManagerComment).HasMaxLength(1000);

            // Связи
            modelBuilder.Entity<OrderRequest>()
                .ToTable("OrderRequests")
                .HasKey(r => r.OrderRequestID);
            modelBuilder.Entity<OrderRequest>()
                .Property(r => r.ProductName).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<OrderRequest>()
                .Property(r => r.Unit).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<OrderRequest>()
                .Property(r => r.Status).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<OrderRequest>()
                .Property(r => r.DirectorComment).HasMaxLength(500);
            modelBuilder.Entity<OrderRequest>()
                .Property(r => r.Quantity).HasPrecision(10, 2);
            modelBuilder.Entity<OrderRequest>()
                .Property(r => r.Price).HasPrecision(10, 2);

            modelBuilder.Entity<User>()
                .HasOptional(u => u.Position)
                .WithMany(p => p.Users)
                .HasForeignKey(u => u.PositionID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<User>()
                .HasOptional(u => u.Store)
                .WithMany(s => s.Employees)
                .HasForeignKey(u => u.StoreID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store>()
                .HasRequired(s => s.Organization)
                .WithMany(o => o.Stores)
                .HasForeignKey(s => s.OrganizationID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store>()
                .HasRequired(s => s.Manager)
                .WithMany(u => u.ManagedStores)
                .HasForeignKey(s => s.ManagerID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Organization>()
                .HasRequired(o => o.Director)
                .WithMany(u => u.Organizations)
                .HasForeignKey(o => o.DirectorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StoreID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Order>()
                .HasRequired(o => o.Supplier)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.SupplierID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ChangeRequest>()
                .HasRequired(r => r.User)
                .WithMany(u => u.ChangeRequests)
                .HasForeignKey(r => r.UserID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ChangeRequest>()
                .HasOptional(r => r.Manager)
                .WithMany(u => u.ResolvedRequests)
                .HasForeignKey(r => r.ManagerID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderRequest>()
                .HasRequired(r => r.Store)
                .WithMany()
                .HasForeignKey(r => r.StoreID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderRequest>()
                .HasRequired(r => r.Manager)
                .WithMany()
                .HasForeignKey(r => r.ManagerID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderRequest>()
                .HasRequired(r => r.Director)
                .WithMany()
                .HasForeignKey(r => r.DirectorID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderRequest>()
                .HasRequired(r => r.Supplier)
                .WithMany()
                .HasForeignKey(r => r.SupplierID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderRequest>()
                .HasOptional(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderID)
                .WillCascadeOnDelete(false);
        }
    }
}
