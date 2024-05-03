using System;
using Microsoft.EntityFrameworkCore;
using Serilog;

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

    public void ExecutePragmas()
    {
        ExecutePragma("PRAGMA journal_mode=TRUNCATE;");
    }
    
    private void ExecutePragma(string pragmaCommand)
    {
        try
        {
            var connection = Database.GetDbConnection();
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = pragmaCommand;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Unable to execute pragma command {pragmaCommand}");
        }
    }
}