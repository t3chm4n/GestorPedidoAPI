using Microsoft.EntityFrameworkCore;
using WebAPI.Models;

namespace WebAPI.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<PedidoProduto> PedidoProdutos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração do relacionamento muitos-para-muitos
        modelBuilder.Entity<PedidoProduto>()
            .HasKey(pp => new { pp.PedidoId, pp.ProdutoId });

        modelBuilder.Entity<PedidoProduto>()
            .HasOne(pp => pp.Pedido)
            .WithMany(p => p.PedidoProdutos)
            .HasForeignKey(pp => pp.PedidoId);

        modelBuilder.Entity<PedidoProduto>()
            .HasOne(pp => pp.Produto)
            .WithMany(p => p.PedidoProdutos)
            .HasForeignKey(pp => pp.ProdutoId);
    }
}
