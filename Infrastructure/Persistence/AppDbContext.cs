using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Domain.Entities;

namespace GestorPedidoAPI.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PedidoEntity> Pedidos { get; set; } = null!;
    public DbSet<ProdutoEntity> Produtos { get; set; } = null!;
    public DbSet<PedidoProduto> PedidoProdutos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PedidoProduto>()
            .HasKey(pp => new { pp.PedidoId, pp.ProdutoId });

        modelBuilder.Entity<PedidoProduto>()
            .HasOne(pp => pp.Pedido)
            .WithMany(p => p.PedidoProdutos)
            .HasForeignKey(pp => pp.PedidoId);
    }
}
