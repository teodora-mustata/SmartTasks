using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models.Data;

namespace SmartTasksAPI.Tests.Repository;

public sealed class RepositoryTestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public RepositoryTestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        ContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public DbContextOptions<ApplicationDbContext> ContextOptions { get; }

    public ApplicationDbContext CreateContext() => new(ContextOptions);

    public void Dispose()
    {
        _connection.Dispose();
    }
}
