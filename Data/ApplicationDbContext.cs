using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data.Entities;

namespace NerdwikiServer.Data;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Lesson> Lessons { get; set; }
    public virtual DbSet<Series> Series { get; set; }
    public virtual DbSet<SeriesLesson> SeriesLessons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07708AF6AC");

            entity.ToTable("Category");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Cover).HasMaxLength(50);
            entity.Property(e => e.Hex).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Lesson__3214EC07B685400A");

            entity.ToTable("Lesson");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.Cover).HasMaxLength(50);
            entity.Property(e => e.Hex).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lesson_ToCategory");
        });

        modelBuilder.Entity<Series>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Series__3214EC07A117A2FF");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.Cover).HasMaxLength(50);
            entity.Property(e => e.Hex).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Series)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Series_ToCategory");
        });

        modelBuilder.Entity<SeriesLesson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SeriesLe__3214EC075C2515EB");

            entity.ToTable("SeriesLesson");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.Cover).HasMaxLength(50);
            entity.Property(e => e.Hex).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.SeriesId).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.SeriesLessons)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeriesLesson_ToCategory");

            entity.HasOne(d => d.Series).WithMany(p => p.SeriesLessons)
                .HasForeignKey(d => d.SeriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeriesLesson_ToSeries");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}