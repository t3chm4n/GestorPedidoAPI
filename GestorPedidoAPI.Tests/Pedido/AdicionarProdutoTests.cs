using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Application.DTOs;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class AdicionarProdutoTests : TestBase
{
    [Fact]
    public void AdicionarProduto_Sucesso_DeveAdicionarProdutosAoPedido()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 3, Quantidade = 2 }  // Novo produto
        };

        // Act
        var result = PedidoController.AdicionarProduto(pedidoId, produtosDto) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal($"Produtos adicionados com sucesso ao pedido {pedidoId}.", result.Value);

        // Verifique os produtos no pedido
        var pedidoProdutos = Context.PedidoProdutos.Where(pp => pp.PedidoId == pedidoId).ToList();
        Assert.Equal(3, pedidoProdutos.Count); // 2 do Seed + 1 novo

        var produtoNovo = pedidoProdutos.First(pp => pp.ProdutoId == 3);
        Assert.Equal(2, produtoNovo.Quantidade); // Novo produto adicionado
    }

    [Fact]
    public void AdicionarProduto_PedidoFechado_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 3; // Pedido fechado no seed
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 2 }
        };

        // Act
        var result = PedidoController.AdicionarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal($"Pedido com ID {pedidoId} está fechado e não pode ser modificado.", result.Value);
    }

    [Fact]
    public void AdicionarProduto_ListaVazia_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>(); // Lista vazia

        // Act
        var result = PedidoController.AdicionarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("A lista de produtos não pode estar vazia.", result.Value);
    }

    [Fact]
    public void AdicionarProduto_ProdutoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 99, Quantidade = 2 } // Produto inexistente
        };

        // Act
        var result = PedidoController.AdicionarProduto(pedidoId, produtosDto) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Produto com ID 99 não encontrado.", result.Value);
    }

    [Fact]
    public void AdicionarProduto_PedidoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var pedidoId = 99; // Pedido inexistente
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 1 }
        };

        // Act
        var result = PedidoController.AdicionarProduto(pedidoId, produtosDto) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal($"Pedido com ID {pedidoId} não encontrado.", result.Value);
    }

    [Fact]
    public void AdicionarProduto_ProdutoDuplicadoNoPedido_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 2 }, // Produto já associado
        };

        // Act
        var result = PedidoController.AdicionarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal($"Produto com ID {produtosDto.First().ProdutoId} já existe no pedido e não pode ser duplicado.", result.Value);
    }
}
