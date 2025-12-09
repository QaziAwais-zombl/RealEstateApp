using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RealEstateApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Property entity
            modelBuilder.Entity<Property>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Properties)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Residential" },
                new Category { Id = 2, Name = "Commercial" },
                new Category { Id = 3, Name = "Industrial" },
                new Category { Id = 4, Name = "Land" },
                new Category { Id = 5, Name = "Apartment" }
            );
        }
    }
}