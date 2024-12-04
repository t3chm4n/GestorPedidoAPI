using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.WebAPI.Controllers;
using GestorPedidoAPI.Tests.Seed;

namespace GestorPedidoAPI.Tests.Base;

public abstract class TestBase : IDisposable
{
    protected readonly AppDbContext Context;
    protected readonly PedidoController Controller;

    protected TestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Banco único por teste
            .Options;

        Context = new AppDbContext(options);

        // Restaura e preenche o banco para cada teste
        SeedDatabaseHelper.Seed(Context);

        Controller = new PedidoController(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
    }
}
