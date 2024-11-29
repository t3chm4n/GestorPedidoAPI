using Xunit;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace GestorPedidoAPI.Tests;

public class PedidoControllerTests : IClassFixture<WebApplicationFactory<GestorPedidoAPI.WebAPI.Program>>
{
    private readonly HttpClient _client;

    public PedidoControllerTests(WebApplicationFactory<GestorPedidoAPI.WebAPI.Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_Root_Returns_OK()
    {
        // Arrange
        var url = "/";

        // Act
        var response = await _client.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("API Gestor de Pedidos Rodando!");
    }
}
