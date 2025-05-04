using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CarmodelAPI.Models;

public partial class CarModelContext : DbContext
{

    public static void SetConnectionString(string connectionString)
    {
        if (ConnectionString == null)
        {
            ConnectionString = connectionString;
        }
        else
        {
            throw new Exception();
        }
    }
    private static string ConnectionString { get; set; }

    public CarModelContext()
    {
    }

    public CarModelContext(DbContextOptions<CarModelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblBrand> TblBrands { get; set; }

    public virtual DbSet<TblCarModel> TblCarModels { get; set; }

    public virtual DbSet<TblClass> TblClasses { get; set; }

    public virtual DbSet<TblImage> TblImages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblBrand>(entity =>
        {
            entity.HasKey(e => e.BrandId);

            entity.ToTable("tblBrand");

            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.BrandName).HasMaxLength(50);
        });

        modelBuilder.Entity<TblCarModel>(entity =>
        {
            entity.HasKey(e => e.ModelId);

            entity.ToTable("tblCarModel");

            entity.Property(e => e.ModelId).HasColumnName("ModelID");
            entity.Property(e => e.BrandId).HasColumnName("BrandID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.DateofManufacturing).HasColumnType("datetime");
            entity.Property(e => e.ModelCode).HasMaxLength(50);
            entity.Property(e => e.ModelName).HasMaxLength(50);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<TblClass>(entity =>
        {
            entity.HasKey(e => e.ClassId);

            entity.ToTable("tblClass");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.ClassName).HasMaxLength(50);
        });

        modelBuilder.Entity<TblImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);

            entity.ToTable("tblImages");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.ModelId).HasColumnName("ModelID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
