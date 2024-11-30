using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.WebAPI.Controllers;
using GestorPedidoAPI.Application.Exceptions;
using GestorPedidoAPI.Application.Commons;
using GestorPedidoAPI.Application.DTOs;

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

        SeedDatabase();
    }

    private void SeedDatabase()
    {
        ResetDatabase();

        // Cria um pedido com produto
        var pedido = new Pedido { Id = 1, Fechado = false };
        var produto = new Produto { Id = 1, Nome = "Produto 1", Preco = 10.0m };
        var pedidoProduto = new PedidoProduto { PedidoId = 1, ProdutoId = 1, Produto = produto, Quantidade = 2 };

        _context.Pedidos.Add(pedido);
        _context.Produtos.Add(produto);
        _context.PedidoProdutos.Add(pedidoProduto);
        _context.SaveChanges();
    }

    private void ResetDatabase()
    {
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void IniciarPedido_DeveCriarPedido()
    {
        // Act
        var result = _controller.IniciarPedido() as CreatedAtActionResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(201, result?.StatusCode);
    }

    [Fact]
    public void AdicionarProduto_DeveAdicionarProdutoAoPedido()
    {
        // Act
        var result = _controller.AdicionarProduto(1, 1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Produto com ID 1 adicionado ao pedido 1.", result?.Value);
    }

    [Fact]
    public void AdicionarProduto_PedidoNaoEncontrado_DeveLancarExcecao()
    {
        // Act & Assert
        var exception = Assert.Throws<PedidoException>(() => _controller.AdicionarProduto(99, 1));
        Assert.Equal("Pedido com ID 99 não encontrado.", exception.Message);
    }

    [Fact]
    public void RemoverProduto_DeveRemoverProdutoDoPedido()
    {
        // Arrange
        var pedidoId = 1;
        var produtoId = 1;

        // Act
        var result = _controller.RemoverProduto(pedidoId, produtoId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal($"Produto com ID {produtoId} removido completamente do pedido {pedidoId}.", result?.Value);

        // Verifica se o produto foi completamente removido
        var pedidoProduto = _context.PedidoProdutos.FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoId);
        Assert.Null(pedidoProduto);
    }

    [Fact]
    public void FecharPedido_DeveFecharPedidoComProdutos()
    {
        // Act
        var result = _controller.FecharPedido(1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Pedido com ID 1 fechado com sucesso.", result?.Value);

        // Verifica se o pedido foi fechado
        var pedido = _context.Pedidos.Find(1);
        Assert.NotNull(pedido);
        Assert.True(pedido.Fechado);
    }

    [Fact]
    public void FecharPedido_SemProdutos_DeveLancarExcecao()
    {
        // Arrange
        var pedidoSemProdutos = new Pedido { Id = 2, Fechado = false };
        _context.Pedidos.Add(pedidoSemProdutos);
        _context.SaveChanges();

        // Act & Assert
        var exception = Assert.Throws<PedidoException>(() => _controller.FecharPedido(2));
        Assert.Equal("Pedido com ID 2 não pode ser fechado sem produtos.", exception.Message);
    }

    [Fact]
    public void ListarPedidos_DeveRetornarPedidosPaginados()
    {
        // Act
        var result = _controller.ListarPedidosPaginados() as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        // Valida o formato do retorno
        var response = result?.Value as PaginacaoResponse<object>;
        Assert.NotNull(response);
        Assert.Equal(1, response?.TotalItems);
        Assert.Single(response?.Items);
    }

    [Fact]
    public void ObterPedido_DeveRetornarPedidoPorId()
    {
        // Act
        var result = _controller.ObterPedido(1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        // Verifica os detalhes do pedido
        var pedido = result?.Value as PedidoDto;
        Assert.NotNull(pedido);
        Assert.Equal(1, pedido!.Id); // Confirma que o ID é o esperado
        Assert.NotEmpty(pedido.Produtos); // Garante que o pedido contém produtos
    }

    [Fact]
    public void ObterPedido_PedidoNaoEncontrado_DeveRetornarNotFound()
    {
        // Act
        var result = _controller.ObterPedido(99) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result?.StatusCode);
        Assert.Equal("Pedido não encontrado.", result?.Value);
    }
}
