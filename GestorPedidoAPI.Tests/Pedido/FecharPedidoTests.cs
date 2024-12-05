using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Domain.Enums;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class FecharPedidoTests : TestBase
{
    [Fact]
    public void FecharPedido_DeveFecharPedidoComProdutos()
    {
        // Act
        var result = PedidoController.FecharPedido(1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Pedido com ID 1 fechado com sucesso.", result?.Value);

        // Verifica se o status foi alterado corretamente no banco
        var pedido = Context.Pedidos.Find(1);
        Assert.NotNull(pedido);
        Assert.Equal(PedidoStatus.Fechado.ToString(), pedido?.Status);
    }

    [Fact]
    public void FecharPedido_SemProdutos_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoSemProdutos = new PedidoEntity
        {
            Id = 4,
            Status = PedidoStatus.Aberto.ToString()
        };
        Context.Pedidos.Add(pedidoSemProdutos);
        Context.SaveChanges();

        // Act
        var result = PedidoController.FecharPedido(4) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Pedido com ID 4 não pode ser fechado sem produtos.", result?.Value);

        // Verifica se o status permanece como "Aberto"
        var pedido = Context.Pedidos.Find(4);
        Assert.NotNull(pedido);
        Assert.Equal(PedidoStatus.Aberto.ToString(), pedido?.Status);
    }

    [Fact]
    public void FecharPedido_PedidoJaFechado_DeveRetornarBadRequest()
    {
        // Act
        var result = PedidoController.FecharPedido(3) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Pedido com ID 3 já está fechado.", result?.Value);

        // Verifica se o status permanece como "Fechado"
        var pedido = Context.Pedidos.Find(3);
        Assert.NotNull(pedido);
        Assert.Equal(PedidoStatus.Fechado.ToString(), pedido?.Status);
    }

    [Fact]
    public void FecharPedido_PedidoNaoEncontrado_DeveRetornarBadRequest()
    {
        // Act
        var result = PedidoController.FecharPedido(99) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Pedido com ID 99 não encontrado.", result?.Value);
    }
}
