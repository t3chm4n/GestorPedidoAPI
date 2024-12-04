using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.Application.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace GestorPedidoAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutoController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProdutoController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    /// <param name="produto">Objeto do produto a ser criado.</param>
    /// <response code="201">Produto criado com sucesso.</response>
    /// <response code="400">Dados do produto são inválidos.</response>
    [HttpPost("criar")]
    [SwaggerOperation(
        Summary = "Cria um novo produto",
        Description = @"
        Cria um novo produto básico e retorna os detalhes do produto criado.

        Regras de validação:
        - O nome do produto é obrigatório.
        - O preço do produto deve ser maior que zero.
        - O nome do produto deve ser único na base de dados.

        Exemplo de requisição:
        POST /api/produto/criar
        {
            ""nome"": ""Produto A"",
            ""preco"": 100.50
        }

        Possíveis respostas:
        - 201: Produto criado com sucesso.
        - 400: Dados do produto inválidos."
    )]
    [SwaggerResponse(201, "Produto criado com sucesso.", typeof(ProdutoDto))]
    [SwaggerResponse(400, "Dados do produto são inválidos.")]
    public IActionResult CriarProduto([FromBody] ProdutoEntity produto)
    {
        // Validação: Produto enviado
        if (produto == null)
        {
            return BadRequest("Dados do produto são obrigatórios.");
        }

        // Validação: Nome do produto
        if (string.IsNullOrWhiteSpace(produto.Nome))
        {
            return BadRequest("O nome do produto é obrigatório.");
        }

        // Validação: Preço do produto
        if (produto.Preco <= 0)
        {
            return BadRequest("O preço do produto deve ser maior que zero.");
        }

        // Validação: Nome único
        var produtoExistente = _context.Produtos
            .FirstOrDefault(p => p.Nome.Equals(produto.Nome, StringComparison.OrdinalIgnoreCase));
        if (produtoExistente != null)
        {
            return BadRequest($"Já existe um produto com o nome '{produto.Nome}'.");
        }

        // Adiciona e salva o produto
        _context.Produtos.Add(produto);
        _context.SaveChanges();

        // Retorna os detalhes do produto criado
        var produtoDto = new ProdutoDto
        {
            ProdutoId = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco
        };

        return StatusCode(201, produtoDto);
    }

    /// <summary>
    /// Remove um produto pelo ID.
    /// </summary>
    /// <param name="id">ID do produto.</param>
    /// <response code="200">Produto removido com sucesso.</response>
    /// <response code="400">
    /// Retornado em caso de:
    /// - Produto associado a pedidos.
    /// - Produto não encontrado.
    /// </response>
    /// <response code="404">Produto não encontrado.</response>
    [HttpDelete("{id}/excluir")]
    [SwaggerOperation(
        Summary = "Exclui um produto existente",
        Description = @"
            Remove um produto pelo ID especificado.

            Regras de validação:
            - O produto deve existir na base de dados.
            - O produto não pode estar associado a nenhum pedido.

            Exemplo de requisição:
            DELETE /api/produto/{id}/excluir

            Possíveis respostas:
            - 200: Produto removido com sucesso.
            - 400: Produto está associado a pedidos e não pode ser removido.
            - 404: Produto não encontrado."
    )]
    [SwaggerResponse(200, "Produto excluído com sucesso.")]
    [SwaggerResponse(400, "Produto está associado a pedidos e não pode ser removido.")]
    [SwaggerResponse(404, "Produto não encontrado.")]
    public IActionResult DeletarProduto(
        [SwaggerParameter(Description = "ID do produto a ser excluído.", Required = true)] int id
    )
    {
        var produto = _context.Produtos.Find(id);
        if (produto == null)
        {
            return NotFound($"Produto com ID {id} não encontrado.");
        }

        // Verificar se o produto está associado a algum pedido
        var produtoEmPedidos = _context.PedidoProdutos.Any(pp => pp.ProdutoId == id);
        if (produtoEmPedidos)
        {
            return BadRequest($"Produto com ID {id} está associado a pedidos e não pode ser removido.");
        }

        _context.Produtos.Remove(produto);
        _context.SaveChanges();

        return Ok($"Produto com ID {id} removido com sucesso.");
    }

}
