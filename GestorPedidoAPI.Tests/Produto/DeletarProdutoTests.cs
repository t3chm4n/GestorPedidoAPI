using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Produto;

public class DeletarProdutoTests : TestBase
{
    [Fact]
    public void DeletarProduto_DeveRemoverProdutoComSucesso()
    {
        // Arrange
        var produtoId = 4; // Produto não associado a pedidos no seed

        // Act
        var result = ProdutoController.DeletarProduto(produtoId) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result?.StatusCode);
        Assert.Equal($"Produto com ID {produtoId} removido com sucesso.", result?.Value);

        // Verifica se o produto foi removido
        var produtoRemovido = Context.Produtos.FirstOrDefault(p => p.Id == produtoId);
        Assert.Null(produtoRemovido);
    }

    [Fact]
    public void DeletarProduto_ProdutoNaoEncontrado_DeveRetornarNotFound()
    {
        // Arrange
        var produtoId = 99; // ID inexistente

        // Act
        var result = ProdutoController.DeletarProduto(produtoId) as NotFoundObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(404, result?.StatusCode);
        Assert.Equal($"Produto com ID {produtoId} não encontrado.", result?.Value);
    }

    [Fact]
    public void DeletarProduto_ProdutoAssociadoAPedidos_DeveRetornarBadRequest()
    {
        // Arrange
        var produtoId = 1; // Produto associado a pedidos no seed

        // Act
        var result = ProdutoController.DeletarProduto(produtoId) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal($"Produto com ID {produtoId} está associado a pedidos e não pode ser removido.", result?.Value);
    }
}
