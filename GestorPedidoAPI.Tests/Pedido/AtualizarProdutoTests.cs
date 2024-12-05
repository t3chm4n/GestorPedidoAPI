using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Application.DTOs;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class AtualizarProdutoTests : TestBase
{
    [Fact]
    public void AtualizarProduto_Sucesso_DeveAtualizarProdutosNoPedido()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 5 }, // Produto existente
            new ProdutoPedidoDto { ProdutoId = 2, Quantidade = 2 }  // Produto existente
        };

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal($"Produtos atualizados com sucesso no pedido {pedidoId}.", result.Value);

        // Verifique os produtos no pedido
        var pedidoProdutos = Context.PedidoProdutos.Where(pp => pp.PedidoId == pedidoId).ToList();
        Assert.Equal(2, pedidoProdutos.Count); // Deve conter os mesmos dois produtos

        var produto1 = pedidoProdutos.First(pp => pp.ProdutoId == 1);
        Assert.Equal(5, produto1.Quantidade); // Quantidade atualizada para 5

        var produto2 = pedidoProdutos.First(pp => pp.ProdutoId == 2);
        Assert.Equal(2, produto2.Quantidade); // Quantidade atualizada para 2
    }

    [Fact]
    public void AtualizarProduto_ProdutoDuplicado_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 1 },
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 1 }, // Duplicado
            new ProdutoPedidoDto { ProdutoId = 3, Quantidade = 1 },
            new ProdutoPedidoDto { ProdutoId = 3, Quantidade = 1 } // Duplicado
        };

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("Produtos com IDs {1, 3} duplicados no request.", result.Value.ToString());
    }

    [Fact]
    public void AtualizarProduto_PedidoFechado_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 3; // Pedido fechado no Seed
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 2, Quantidade = 3 }
        };

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal($"Pedido com ID {pedidoId} está fechado e não pode ser modificado.", result.Value);
    }

    [Fact]
    public void AtualizarProduto_PedidoNaoEncontrado_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 99; // Pedido inexistente
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 5 }
        };

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal($"Pedido com ID {pedidoId} não encontrado.", result.Value.ToString());
    }

    [Fact]
    public void AtualizarProduto_QuantidadeInvalida_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 0 } // Quantidade inválida
        };

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("A quantidade para o produto com ID 1 deve ser maior que zero.", result.Value.ToString());
    }

    [Fact]
    public void AtualizarProduto_ListaProdutosVazia_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;
        var produtosDto = new List<ProdutoPedidoDto>(); // Lista vazia

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("A lista de produtos não pode estar vazia.", result.Value.ToString());
    }

    [Fact]
    public void AtualizarProduto_ListaProdutosNula_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;
        List<ProdutoPedidoDto>? produtosDto = null; // Lista nula

        // Act
        var result = PedidoController.AtualizarProduto(pedidoId, produtosDto!) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
        Assert.NotNull(result.Value);
        Assert.Equal("A lista de produtos não pode estar vazia.", result.Value.ToString());
    }
}
