using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.WebAPI.Controllers;

namespace GestorPedidoAPI.Tests;

public class PedidoControllerTests
{
    private readonly AppDbContext _context;
    private readonly PedidoController _controller;

    public PedidoControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        _context = new AppDbContext(options);
        _controller = new PedidoController(_context);

        InitializeTest();
    }

    private void InitializeTest()
    {
        ResetDatabase();
        SeedDatabase();
    }

    private void ResetDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    private void SeedDatabase()
    {
        CriarPedidoComProduto(1, 1, 2);
    }

    private void CriarPedidoComProduto(int pedidoId, int produtoId, int quantidade)
    {
        var pedido = new Pedido { Id = pedidoId };
        var produto = new Produto { Id = produtoId, Nome = $"Produto {produtoId}", Preco = 10.0m * produtoId };
        var pedidoProduto = new PedidoProduto { PedidoId = pedidoId, ProdutoId = produtoId, Produto = produto, Quantidade = quantidade };

        _context.Pedidos.Add(pedido);
        _context.Produtos.Add(produto);
        _context.PedidoProdutos.Add(pedidoProduto);
        _context.SaveChanges();
    }

    [Fact]
    public void IniciarPedido_DeveRetornarPedidoCriado()
    {
        var result = _controller.IniciarPedido() as CreatedAtActionResult;
        Assert.NotNull(result);
        Assert.Equal(201, result?.StatusCode);
    }

    [Fact]
    public void AdicionarProduto_DeveAdicionarProdutoAoPedido()
    {
        var result = _controller.AdicionarProduto(1, 1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Produto adicionado ao pedido.", result?.Value);

        var pedidoProduto = _context.PedidoProdutos.FirstOrDefault(pp => pp.PedidoId == 1 && pp.ProdutoId == 1);
        Assert.NotNull(pedidoProduto);
        Assert.Equal(2, pedidoProduto.Quantidade);
    }

    [Fact]
    public void AdicionarProduto_PedidoNaoEncontrado_DeveRetornarBadRequest()
    {
        var result = _controller.AdicionarProduto(99, 1) as BadRequestObjectResult;
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Pedido não encontrado.", result?.Value);
    }

    [Fact]
    public void RemoverProduto_DeveRemoverProdutoDoPedido()
    {
        var result = _controller.RemoverProduto(1, 1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Produto removido do pedido.", result?.Value);

        var pedidoProduto = _context.PedidoProdutos.FirstOrDefault(pp => pp.PedidoId == 1 && pp.ProdutoId == 1);
        Assert.Null(pedidoProduto);
    }

    [Fact]
    public void FecharPedido_DeveFecharPedidoComProdutos()
    {
        var result = _controller.FecharPedido(1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Pedido fechado com sucesso.", result?.Value);

        var pedido = _context.Pedidos.Find(1);
        Assert.NotNull(pedido);
        Assert.True(pedido.Fechado);

        var pedidoProdutos = _context.PedidoProdutos.Where(pp => pp.PedidoId == 1).ToList();
        Assert.NotEmpty(pedidoProdutos);
    }

    [Fact]
    public void ListarPedidos_DeveRetornarPedidosPaginados()
    {
        var result = _controller.ListarPedidosPaginados() as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var pedidos = result?.Value as IEnumerable<Pedido>;
        Assert.NotNull(pedidos);
        Assert.Equal(1, pedidos.Count());
    }

    [Fact]
    public void ObterPedido_DeveRetornarPedidoPorId()
    {
        var result = _controller.ObterPedido(1) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var pedido = result?.Value as Pedido;
        Assert.NotNull(pedido);
        Assert.Equal(1, pedido.Id);
    }

    [Fact]
    public void ObterPedido_PedidoNaoEncontrado_DeveRetornarNotFound()
    {
        var result = _controller.ObterPedido(99) as NotFoundObjectResult;
        Assert.NotNull(result);
        Assert.Equal(404, result?.StatusCode);
        Assert.Equal("Pedido não encontrado.", result?.Value);
    }
}
