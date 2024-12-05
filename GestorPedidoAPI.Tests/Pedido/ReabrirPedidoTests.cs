using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Tests.Base;
using GestorPedidoAPI.Domain.Enums;
using GestorPedidoAPI.WebAPI.Controllers;

namespace GestorPedidoAPI.Tests.Pedido;

public class ReabrirPedidoTests : TestBase
{
    [Fact]
    public void ReabrirPedido_Sucesso_DeveReabrirPedidoFechado()
    {
        // Arrange
        var pedidoId = 3; // Pedido fechado no SeedDatabase

        // Act
        var result = PedidoController.ReabrirPedido(pedidoId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal($"Pedido com ID {pedidoId} reaberto com sucesso.", result?.Value);

        // Verifica se o status do pedido foi alterado para 'Aberto'
        var pedido = Context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);
        Assert.NotNull(pedido);
        Assert.Equal(PedidoStatus.Aberto.ToString(), pedido?.Status);
    }

    [Fact]
    public void ReabrirPedido_PedidoNaoEncontrado_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 99; // Pedido inexistente

        // Act
        var result = PedidoController.ReabrirPedido(pedidoId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal($"Pedido com ID {pedidoId} não encontrado.", result?.Value);
    }

    [Fact]
    public void ReabrirPedido_PedidoComStatusInvalido_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 2; // Modificar manualmente o status do pedido para um inválido
        var pedido = Context.Pedidos.FirstOrDefault(p => p.Id == pedidoId);
        Assert.NotNull(pedido);
        pedido!.Status = "Cancelado";
        Context.SaveChanges();

        // Act
        var result = PedidoController.ReabrirPedido(pedidoId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal($"Somente pedidos com status 'Fechado' podem ser reabertos. Status atual: Cancelado.", result?.Value);
    }
}
