using farmmanager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Search;

namespace farmmanager.Data
{
    public class PlantationDbContext : DbContext
    {
        public DbSet<Worker> Workers => Set<Worker>();
        public DbSet<PlantationParcel> Parcels => Set<PlantationParcel>();
        public DbSet<Activite> Activities => Set<Activite>();
        public DbSet<WorkEntry> WorkEntries => Set<WorkEntry>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = Path.Combine(folder, "PlantationManager", "plantation.db");
            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
            options.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // WorkEntry relationships
            modelBuilder.Entity<WorkEntry>()
                .HasOne(w => w.Worker)
                .WithMany(w => w.WorkEntries)
                .HasForeignKey(w => w.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkEntry>()
                .HasOne(w => w.Parcel)
                .WithMany(p => p.WorkEntries)
                .HasForeignKey(w => w.ParcelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkEntry>()
                .HasOne(w => w.Activity)
                .WithMany(a => a.WorkEntries)
                .HasForeignKey(w => w.ActivityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision
            modelBuilder.Entity<WorkEntry>()
                .Property(w => w.AmountEarned)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Activite>()
                .Property(a => a.UnitRate)
                .HasColumnType("decimal(18,2)");

            // Seed default activities
            modelBuilder.Entity<Activite>().HasData(
                new Activite { Id = 1, Name = "Weeding", Unit = "hectare", UnitRate = 15000, Description = "Manual weeding of plantation" },
                new Activite { Id = 2, Name = "Harvesting", Unit = "kg", UnitRate = 50, Description = "Fruit harvesting" },
                new Activite { Id = 3, Name = "Fertilizing", Unit = "hectare", UnitRate = 20000, Description = "Fertilizer application" },
                new Activite { Id = 4, Name = "Pruning", Unit = "tree", UnitRate = 500, Description = "Tree pruning and maintenance" },
                new Activite { Id = 5, Name = "General Labour", Unit = "day", UnitRate = 3500, Description = "General daily labour" }
            );
        }
    }
}
