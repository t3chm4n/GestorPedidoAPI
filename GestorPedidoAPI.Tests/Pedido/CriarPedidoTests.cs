using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Application.DTOs;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class CriarPedidoTests : TestBase
{
    [Fact]
    public void CriarPedido_DeveCriarPedidoComProdutos()
    {
        // Arrange
        var criarPedidoDto = new CriarPedidoDto
        {
            Produtos = new List<ProdutoPedidoDto>
        {
            new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 2 },
            new ProdutoPedidoDto { ProdutoId = 2, Quantidade = 1 }
        }
        };

        // Act
        var result = PedidoController.CriarPedido(criarPedidoDto) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var mensagemRetornada = result?.Value?.ToString();
        Assert.StartsWith("Pedido com ID ", mensagemRetornada);
    }

    [Fact]
    public void CriarPedido_DeveRetornarBadRequest_SeNaoHouverProdutos()
    {
        // Arrange
        var criarPedidoDto = new CriarPedidoDto { Produtos = new List<ProdutoPedidoDto>() };

        // Act
        var result = PedidoController.CriarPedido(criarPedidoDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("A lista de produtos não pode estar vazia.", result?.Value);
    }

    [Fact]
    public void CriarPedido_DeveRetornarNotFound_SeProdutoNaoExistir()
    {
        // Arrange
        var criarPedidoDto = new CriarPedidoDto
        {
            Produtos = new List<ProdutoPedidoDto>
            {
                new ProdutoPedidoDto { ProdutoId = 99, Quantidade = 1 } // Produto não existente
            }
        };

        // Act
        var result = PedidoController.CriarPedido(criarPedidoDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Produto com ID 99 não encontrado.", result?.Value);
    }

    [Fact]
    public void CriarPedido_DeveRetornarBadRequest_SeQuantidadeForMenorOuIgualAZero()
    {
        // Arrange
        var criarPedidoDto = new CriarPedidoDto
        {
            Produtos = new List<ProdutoPedidoDto>
            {
                new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 0 }, // Quantidade inválida
            }
        };

        // Act
        var result = PedidoController.CriarPedido(criarPedidoDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Quantidade inválida para o produto com ID 1. Deve ser maior que zero.", result?.Value);

        criarPedidoDto = new CriarPedidoDto
        {
            Produtos = new List<ProdutoPedidoDto>
            {
                new ProdutoPedidoDto { ProdutoId = 2, Quantidade = -1 }, // Quantidade inválida
            }
        };

        // Act
        result = PedidoController.CriarPedido(criarPedidoDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Quantidade inválida para o produto com ID 2. Deve ser maior que zero.", result?.Value);
    }

    [Fact]
    public void CriarPedido_DeveRetornarBadRequest_SeProdutoForDuplicado()
    {
        // Arrange
        var criarPedidoDto = new CriarPedidoDto
        {
            Produtos = new List<ProdutoPedidoDto>
            {
                new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 2 },
                new ProdutoPedidoDto { ProdutoId = 1, Quantidade = 3 } // Produto duplicado
            }
        };

        // Act
        var result = PedidoController.CriarPedido(criarPedidoDto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("O produto com ID 1 foi adicionado mais de uma vez ao pedido.", result?.Value);
    }
}
