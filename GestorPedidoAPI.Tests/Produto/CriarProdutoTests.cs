using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Tests.Base;

namespace GestorPedidoAPI.Tests.Produto;

public class CriarProdutoTests : TestBase
{
    [Fact]
    public void CriarProduto_DeveCriarProdutoComSucesso()
    {
        // Arrange
        var novoProduto = new ProdutoEntity
        {
            Nome = "Produto Novo",
            Preco = 99.99m
        };

        // Act
        var result = ProdutoController.CriarProduto(novoProduto) as ObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(201, result?.StatusCode);

        var produtoCriado = Context.Produtos.FirstOrDefault(p => p.Nome == "Produto Novo");
        Assert.NotNull(produtoCriado);
        Assert.Equal(99.99m, produtoCriado.Preco);
    }

    [Fact]
    public void CriarProduto_SemNome_DeveRetornarBadRequest()
    {
        // Arrange
        var novoProduto = new ProdutoEntity
        {
            Nome = "",
            Preco = 50.00m
        };

        // Act
        var result = ProdutoController.CriarProduto(novoProduto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("O nome do produto é obrigatório.", result?.Value);
    }

    [Fact]
    public void CriarProduto_ComPrecoZero_DeveRetornarBadRequest()
    {
        // Arrange
        var novoProduto = new ProdutoEntity
        {
            Nome = "Produto Sem Preço",
            Preco = 0m
        };

        // Act
        var result = ProdutoController.CriarProduto(novoProduto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("O preço do produto deve ser maior que zero.", result?.Value);
    }

    [Fact]
    public void CriarProduto_NomeDuplicado_DeveRetornarBadRequest()
    {
        // Arrange
        var novoProduto = new ProdutoEntity
        {
            Nome = "Produto 1", // Nome já existente no seed
            Preco = 15.00m
        };

        // Act
        var result = ProdutoController.CriarProduto(novoProduto) as BadRequestObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(400, result?.StatusCode);
        Assert.Equal("Já existe um produto com o nome 'Produto 1'.", result?.Value);
    }
}
