using HorariosFI.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HorariosFI.Core;

public class SchedulesDb : DbContext
{
    // public DbSet<ClassModel> Schedules { get; set; }

    public DbSet<FiTeacher> FiTeachers { get; set; }
    public DbSet<FiGroup> FiGroups { get; set; }
    public DbSet<FiClass> FiClasses { get; set; }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("Data Source=database.sql");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // modelBuilder.Entity<ClassModel>();
        modelBuilder.Entity<FiTeacher>()
            .HasMany(teacher => teacher.Groups)
            .WithOne(group => group.FiTeacher)
            .HasForeignKey(group => group.FiTeacherId)
            .IsRequired();

        modelBuilder.Entity<FiGroup>()
            .HasOne(group => group.FiTeacher)
            .WithMany(teacher => teacher.Groups)
            .HasForeignKey(group => group.FiTeacherId)
            .IsRequired();

        modelBuilder.Entity<FiClass>()
            .HasMany(fiClass => fiClass.FiGroups)
            .WithOne(group => group.FiClass)
            .HasForeignKey(group => group.FiClassId)
            .IsRequired();
    }
}