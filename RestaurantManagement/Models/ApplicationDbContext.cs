using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Models;

namespace RestaurantManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tài khoản đăng nhập
        public DbSet<Account> Accounts { get; set; }

        // Khách hàng, phản hồi
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        // Bàn ăn và đặt bàn
        public DbSet<DingningTable> DingningTables { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<OrderItemHistory> OrderItemHistories { get; set; }

        // Món ăn, loại món, order, chi tiết order
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        // Thanh toán
        public DbSet<Payment> Payments { get; set; }

        // Nguyên liệu và sử dụng kho
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<InventoryUsage> InventoryUsages { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

        // Nhân viên, ca làm việc, chấm công
        public DbSet<Staffs> Staffs { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Reply> Replies { get; set; }

        // Báo cáo tổng hợp
        public DbSet<Report> Reports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
               .Property(r => r.DepositAmount)
               .HasPrecision(18, 2); // 18 digits, 2 decimal places

            // MenuItem.Price
            modelBuilder.Entity<MenuItem>()
                .Property(m => m.Price)
                .HasPrecision(18, 2);

            // Payment.TotalAmount
            modelBuilder.Entity<Payment>()
                .Property(p => p.TotalAmount)
                .HasPrecision(18, 2);

            // InventoryUsage
            modelBuilder.Entity<InventoryUsage>()
                .HasOne(iu => iu.MenuItem)
                .WithMany(mi => mi.InventoryUsages)
                .HasForeignKey(iu => iu.MenuItemId);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<InventoryUsage>()
                .HasOne(iu => iu.InventoryItem)
                .WithMany(ii => ii.Usages)
                .HasForeignKey(iu => iu.InventoryItemId);

            // Allow null Customer for guest order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Reply>()
                .HasOne(r => r.Feedback)
                .WithOne(f => f.Reply)
                .HasForeignKey<Reply>(r => r.FeedbackId);
        }

    }
}
