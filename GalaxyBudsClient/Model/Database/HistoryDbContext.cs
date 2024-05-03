using System;
using Microsoft.EntityFrameworkCore;

namespace GalaxyBudsClient.Model.Database;

public class HistoryDbContext(string path) : DbContext
{
    [Obsolete("This constructor should not be used. It is required for the ef-core migrations tool to work correctly.")]
    public HistoryDbContext() : this(string.Empty) {}

    public DbSet<HistoryRecord> Records { get; set; } = null!;
    
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .UseSqlite($"DataSource={path}");
}