using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.Domain.Enums;

namespace GestorPedidoAPI.Tests.Seed;

public static class SeedDatabaseHelper
{
    public static void Seed(AppDbContext context)
    {
        // Certifique-se de que o banco está limpo antes do seed
        ResetDatabase(context);

        // Produtos
        var produtos = new List<ProdutoEntity>
        {
            new ProdutoEntity { Id = 1, Nome = "Produto 1", Preco = 10.0m },
            new ProdutoEntity { Id = 2, Nome = "Produto 2", Preco = 20.0m },
            new ProdutoEntity { Id = 3, Nome = "Produto 3", Preco = 30.0m },
            new ProdutoEntity { Id = 4, Nome = "Produto 4", Preco = 40.0m }, // Não associado
            new ProdutoEntity { Id = 5, Nome = "Produto 5", Preco = 50.0m }  // Não associado
        };

        // Pedidos
        var pedidos = new List<PedidoEntity>
        {
            new PedidoEntity { Id = 1, Status = PedidoStatus.Aberto.ToString() },
            new PedidoEntity { Id = 2, Status = PedidoStatus.Aberto.ToString() },
            new PedidoEntity { Id = 3, Status = PedidoStatus.Fechado.ToString() } // Pedido fechado
        };

        // PedidoProdutos
        var pedidoProdutos = new List<PedidoProduto>
        {
            new PedidoProduto { PedidoId = 1, ProdutoId = 1, Quantidade = 2 },
            new PedidoProduto { PedidoId = 1, ProdutoId = 2, Quantidade = 1 },
            new PedidoProduto { PedidoId = 2, ProdutoId = 2, Quantidade = 3 },
            new PedidoProduto { PedidoId = 2, ProdutoId = 3, Quantidade = 1 }
        };

        // Adiciona ao contexto
        context.Produtos.AddRange(produtos);
        context.Pedidos.AddRange(pedidos);
        context.PedidoProdutos.AddRange(pedidoProdutos);

        // Salva as alterações
        context.SaveChanges();
    }

    public static void ResetDatabase(AppDbContext context)
    {
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}