using HorariosFI.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace HorariosFI.Core;

public class SchedulesDb : DbContext
{
    public DbSet<ClassModel> Schedules { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("Data Source=database.sql");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClassModel>();
    }
}