using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Pedido;

public class RemoverProdutoTests : TestBase
{
    [Fact]
    public void RemoverProduto_DeveRemoverProdutoDoPedido()
    {
        // Arrange
        var pedidoId = 1;
        var produtoId = 1;

        // Act
        var result = Controller.RemoverProduto(pedidoId, produtoId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal($"Produto com ID {produtoId} removido completamente do pedido {pedidoId}.", result?.Value);

        // Verifica se o produto foi removido do pedido
        var pedidoProduto = Context.PedidoProdutos.FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoId);
        Assert.Null(pedidoProduto);
    }

    [Fact]
    public void RemoverProduto_PedidoFechado_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 3; // Pedido fechado no SeedDatabase
        var produtoId = 1;

        // Act
        var result = Controller.RemoverProduto(pedidoId, produtoId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal($"Pedido com ID {pedidoId} está fechado e não pode ser modificado.", result?.Value);
    }

    [Fact]
    public void RemoverProduto_PedidoSemProdutos_DeveRetornarBadRequest()
    {
        // Arrange
        var pedidoId = 1;

        // Remove todos os produtos do pedido, exceto o último
        var produtos = Context.PedidoProdutos.Where(pp => pp.PedidoId == pedidoId).ToList();
        foreach (var produto in produtos.SkipLast(1))
        {
            Controller.RemoverProduto(pedidoId, produto.ProdutoId);
        }

        // Tenta remover o último produto
        var ultimoProdutoId = produtos.Last().ProdutoId;

        // Act
        var result = Controller.RemoverProduto(pedidoId, ultimoProdutoId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal($"O pedido com ID {pedidoId} não pode ficar sem produtos.", result?.Value);
    }


    [Fact]
    public void RemoverProduto_ProdutoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var pedidoId = 1;
        var produtoId = 99; // Produto inexistente

        // Act
        var result = Controller.RemoverProduto(pedidoId, produtoId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result?.StatusCode);
        Assert.Equal($"Produto com ID {produtoId} não encontrado no pedido {pedidoId}.", result?.Value);
    }
}
