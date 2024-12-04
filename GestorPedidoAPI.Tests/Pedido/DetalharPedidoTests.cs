using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Application.DTOs;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class DetalharPedidoTests : TestBase
{
    [Fact]
    public void DetalharPedido_DeveRetornarDetalhesDoPedido()
    {
        // Act
        var result = PedidoController.DetalharPedido(1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var pedidoDto = result?.Value as PedidoDto;
        Assert.NotNull(pedidoDto);
        Assert.Equal(1, pedidoDto?.Id);

        Assert.NotNull(pedidoDto?.Produtos);
        Assert.NotEmpty(pedidoDto.Produtos);
        var produto1 = pedidoDto?.Produtos.First(p => p.ProdutoId == 1);
        Assert.NotNull(produto1);
        Assert.Equal(2, produto1?.Quantidade);
    }

    [Fact]
    public void DetalharPedido_PedidoNaoEncontrado_DeveRetornarNotFound()
    {
        var result = PedidoController.DetalharPedido(99) as NotFoundObjectResult;

        Assert.NotNull(result);
        Assert.Equal(404, result?.StatusCode);
        Assert.Equal("Pedido com ID 99 não encontrado.", result?.Value);
    }
}
