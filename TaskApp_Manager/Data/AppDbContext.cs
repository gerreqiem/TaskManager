using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using TaskManagerApp.Models;

namespace TaskManagerApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<TaskManagerApp.Models.Task> Tasks { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=taskmanager.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = null
            };

            modelBuilder.Entity<TaskManagerApp.Models.Task>()
                .Property(t => t.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonOptions),
                    v => JsonSerializer.Deserialize<List<string>>(v, jsonOptions) ?? new List<string>()
                );
        }
    }
}