using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using CakeFlow.Core;

namespace CakeFlow.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderComment> OrderComments { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=CakeFlow.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Уникальный логин для User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            // Добавляем администратора при первом запуске
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Login = "admin",
                    PasswordHash = HashPassword("admin123"),
                    FullName = "Главный Администратор",
                    Role = UserRole.Admin
                }
            );

            // Добавляем тестового кондитера
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    Login = "confectioner",
                    PasswordHash = HashPassword("confectioner123"),
                    FullName = "Иван Кондитер",
                    Role = UserRole.Confectioner
                }
            );

            // Добавляем тестового менеджера
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 3,
                    Login = "manager",
                    PasswordHash = HashPassword("manager123"),
                    FullName = "Мария Менеджер",
                    Role = UserRole.Manager
                }
            );
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}