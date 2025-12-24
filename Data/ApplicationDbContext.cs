using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Models;
using System.Collections.Generic;

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
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PropertyRequest> PropertyRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Property>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Properties)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Property)
                .WithMany(p => p.Transactions)
                .HasForeignKey(t => t.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyRequest>()
                .HasOne(pr => pr.Property)
                .WithMany()
                .HasForeignKey(pr => pr.PropertyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyRequest>()
                .HasOne(pr => pr.InterestedUser)
                .WithMany()
                .HasForeignKey(pr => pr.InterestedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PropertyRequest>()
                .HasOne(pr => pr.Seller)
                .WithMany()
                .HasForeignKey(pr => pr.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

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