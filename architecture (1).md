using GroupSchedulerApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupSchedulerApp.Data;

// Layer 4: the database. Each DbSet becomes a table.
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<WorkSession> WorkSessions => Set<WorkSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Busy blocks are stored in their own table, owned by a Schedule
        modelBuilder.Entity<Schedule>().OwnsMany(s => s.BusyBlocks);
    }
}
