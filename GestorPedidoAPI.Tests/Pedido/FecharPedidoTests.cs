using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Domain.Enums;
using GestorPedidoAPI.Application.Exceptions;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class FecharPedidoTests : TestBase
{
    [Fact]
    public void FecharPedido_DeveFecharPedidoComProdutos()
    {
        // Act
        var result = Controller.FecharPedido(1) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal("Pedido com ID 1 fechado com sucesso.", result?.Value);

        var pedido = Context.Pedidos.Find(1);
        Assert.NotNull(pedido);
        Assert.Equal(PedidoStatus.Fechado.ToString(), pedido?.Status); // Verifica se o status foi alterado corretamente
    }

    [Fact]
    public void FecharPedido_SemProdutos_DeveLancarExcecao()
    {
        // Arrange
        var pedidoSemProdutos = new GestorPedidoAPI.Domain.Entities.Pedido
        {
            Id = 4,
            Status = PedidoStatus.Aberto.ToString() // Define o status inicial como "Aberto"
        };
        Context.Pedidos.Add(pedidoSemProdutos);
        Context.SaveChanges();

        // Act & Assert
        var exception = Assert.Throws<PedidoException>(() => Controller.FecharPedido(4));
        Assert.Equal("Pedido com ID 4 não pode ser fechado sem produtos.", exception.Message);
    }

    [Fact]
    public void FecharPedido_PedidoJaFechado_DeveLancarExcecao()
    {
        var exception = Assert.Throws<PedidoException>(() => Controller.FecharPedido(3)); // Pedido já fechado
        Assert.Equal("Pedido com ID 3 não pode ser fechado sem produtos.", exception.Message);
    }
}
