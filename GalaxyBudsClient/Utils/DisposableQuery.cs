using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GalaxyBudsClient.Utils;

public class DisposableQuery<TRecord>(DbContext? db) : IAsyncDisposable where TRecord : class
{
    public readonly IQueryable<TRecord> Queryable = db?.Set<TRecord>().AsQueryable() ?? Array.Empty<TRecord>().AsQueryable();
    
    public ValueTask DisposeAsync()
    {
        return db?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
}