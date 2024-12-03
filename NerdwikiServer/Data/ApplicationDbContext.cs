using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NerdwikiServer.Data.Entities;

namespace NerdwikiServer.Data;

public partial class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonTag> LessonTags { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Topic> Topics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");

            entity.HasIndex(e => e.Name, "AK_Category_Name").IsUnique();

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable("Lesson");

            entity.HasIndex(e => e.Title, "AK_Lesson_Title").IsUnique();

            entity.HasIndex(e => e.CategoryId, "IX_Lesson_CategoryId");

            entity.HasIndex(e => e.TopicId, "IX_Lesson_TopicId");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Cover).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.TopicId).HasMaxLength(50);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Lesson_ToCategory");

            entity.HasOne(d => d.Topic).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.TopicId)
                .HasConstraintName("FK_Lesson_ToTopic");
        });

        modelBuilder.Entity<LessonTag>(entity =>
        {
            entity.HasKey(e => new { e.LessonId, e.TagId });

            entity.ToTable("LessonTag");

            entity.Property(e => e.LessonId).HasMaxLength(50);
            entity.Property(e => e.TagId).HasMaxLength(50);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tag");

            entity.HasIndex(e => e.Name, "AK_Tag_Name").IsUnique();

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.ToTable("Topic");

            entity.HasIndex(e => e.Name, "AK_Topic_Name").IsUnique();

            entity.HasIndex(e => e.CategoryId, "IX_Topic_CategoryId");

            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Category).WithMany(p => p.Topics)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Topic_ToCategory");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
