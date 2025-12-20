using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // The Correct Namespace
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Models;
using System.Collections.Generic;

namespace RealEstateApp.Data
{
    // Now this refers to the correct Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Property> Properties { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

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