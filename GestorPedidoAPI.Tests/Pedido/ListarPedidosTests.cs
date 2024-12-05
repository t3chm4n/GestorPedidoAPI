using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Application.Commons;
using GestorPedidoAPI.Domain.Enums;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class ListarPedidosTests : TestBase
{
    [Fact]
    public void ListarPedidos_DeveRetornarPedidosPaginados()
    {
        // Act
        var result = PedidoController.ListarPedidosPaginados(1, 10) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var response = result?.Value as PaginacaoResponse<object>;
        Assert.NotNull(response);

        // Após Assert.NotNull, não é necessário usar ?. mais
        Assert.Equal(3, response.TotalItems);
        Assert.Equal(3, response.Pedidos.Count());
    }

    [Fact]
    public void ListarPedidos_ComPaginacao_DeveRespeitarParametros()
    {
        // Act
        var result = PedidoController.ListarPedidosPaginados(1, 2) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var response = result?.Value as PaginacaoResponse<object>;
        Assert.NotNull(response);

        // Após Assert.NotNull, não é necessário usar ?. mais
        Assert.Equal(3, response.TotalItems);
        Assert.Equal(2, response.Pedidos.Count());
    }

    [Fact]
    public void ListarPedidos_PaginaInexistente_DeveRetornarSemItens()
    {
        // Act
        var result = PedidoController.ListarPedidosPaginados(5, 10) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var response = result?.Value as PaginacaoResponse<object>;
        Assert.NotNull(response);

        // Após Assert.NotNull, não é necessário usar ?. mais
        Assert.Empty(response.Pedidos);
        Assert.Equal(3, response.TotalItems);
    }

    [Fact]
    public void ListarPedidos_ComFiltroDeStatus_DeveRetornarApenasPedidosDoFiltro()
    {
        // Act
        var result = PedidoController.ListarPedidosPaginadosEPorStatus(1, 10, PedidoStatus.Fechado.ToString()) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);

        var response = result?.Value as PaginacaoResponse<object>;
        Assert.NotNull(response);

        // Após Assert.NotNull, não é necessário usar ?. mais
        Assert.Equal(1, response.TotalItems); // Apenas 1 pedido fechado no SeedDatabase
    }

    [Fact]
    public void ListarPedidos_StatusInvalido_DeveRetornarBadRequest()
    {
        // Act
        var result = PedidoController.ListarPedidosPaginadosEPorStatus(1, 10, "invalido") as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Status deve ser um dos valores definidos no enumerador PedidoStatus.", result?.Value);
    }

}
