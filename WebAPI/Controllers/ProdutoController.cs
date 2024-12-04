using Microsoft.AspNetCore.Mvc;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.Application.Commons;
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
    /// Lista todos os produtos com paginação.
    /// </summary>
    /// <param name="page">Número da página (opcional).</param>
    /// <param name="pageSize">Quantidade de itens por página (opcional).</param>
    /// <response code="200">Lista de produtos retornada com sucesso.</response>
    [HttpGet("/listar")]
    [SwaggerOperation(Summary = "Lista produtos com paginação", Description = "Retorna uma lista de produtos com suporte a paginação.")]
    [SwaggerResponse(200, "Lista de produtos paginados.", typeof(PaginacaoResponse<PedidoDto>))]
    public IActionResult ListarProdutos([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Produtos.AsQueryable();
        var totalItems = query.Count();

        var produtos = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            CurrentPage = page,
            Items = produtos
        });
    }

    /// <summary>
    /// Obtém os detalhes de um produto pelo ID.
    /// </summary>
    /// <param name="id">ID do produto.</param>
    /// <response code="200">Produto encontrado com sucesso.</response>
    /// <response code="404">Produto não encontrado.</response>
    [HttpGet("{produtoId}/detalhar")]
    [SwaggerOperation(
    Summary = "Obter detalhes de um produto",
    Description = "Retorna os detalhes básicos de um produto específico com base no ID fornecido."
)]
    [SwaggerResponse(200, "Detalhes do produto retornados com sucesso.", typeof(ProdutoDto))]
    [SwaggerResponse(404, "Produto não encontrado para o ID fornecido.")]
    public IActionResult ObterProduto(int produtoId)
    {
        var produto = _context.Produtos
            .FirstOrDefault(p => p.Id == produtoId);

        if (produto == null)
        {
            return NotFound($"Produto com ID {produtoId} não encontrado.");
        }

        var produtoDto = new ProdutoDto
        {
            ProdutoId = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco
        };

        return Ok(produtoDto);
    }

    /// <summary>
    /// Cria um novo produto.
    /// </summary>
    /// <param name="produto">Objeto do produto a ser criado.</param>
    /// <response code="201">Produto criado com sucesso.</response>
    [HttpPost("criar")]
    [SwaggerOperation(
    Summary = "Cria um novo produto",
    Description = "Cria um novo produto básico e retorna os detalhes do produto criado."
    )]
    [SwaggerResponse(201, "Produto criado com sucesso.", typeof(ProdutoDto))]
    [SwaggerResponse(400, "Dados do produto são obrigatórios.")]
    public IActionResult CriarProduto([FromBody] Produto produto)
    {
        if (produto == null)
        {
            return BadRequest("Dados do produto são obrigatórios.");
        }

        _context.Produtos.Add(produto);
        _context.SaveChanges();

        var produtoDto = new ProdutoDto
        {
            ProdutoId = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco
        };

        return StatusCode(201, produtoDto); // Retorna 201 Created com os dados do produto criado.
    }

    /// <summary>
    /// Atualiza um produto existente.
    /// </summary>
    /// <param name="id">ID do produto.</param>
    /// <param name="produtoAtualizado">Dados do produto a serem atualizados.</param>
    /// <response code="200">Produto atualizado com sucesso.</response>
    /// <response code="404">Produto não encontrado.</response>
    [HttpPut("{id}/atualizar")]
    [SwaggerOperation(
    Summary = "Atualiza um produto existente",
    Description = "Atualiza informações de um produto existente"
    )]
    [SwaggerResponse(201, "Produto atualizado com sucesso.", typeof(ProdutoDto))]
    [SwaggerResponse(400, "Produto não encontrado")]
    public IActionResult AtualizarProduto(
        [SwaggerParameter(Description = "ID do produto a ser atualizado.")] int id,
        [FromBody] Produto produtoAtualizado)
    {
        var produto = _context.Produtos.Find(id);
        if (produto == null)
        {
            return NotFound($"Produto com ID {id} não encontrado.");
        }

        produto.Nome = produtoAtualizado.Nome;
        produto.Preco = produtoAtualizado.Preco;

        _context.SaveChanges();

        return Ok(produto);
    }

    /// <summary>
    /// Remove um produto pelo ID.
    /// </summary>
    /// <param name="id">ID do produto.</param>
    /// <response code="200">Produto removido com sucesso.</response>
    /// <response code="404">Produto não encontrado.</response>
    [HttpDelete("{id}/excluir")]
    [SwaggerOperation(
    Summary = "Exclui um produto existente",
    Description = "Exclui um produto existente a partir da id"
    )]
    [SwaggerResponse(201, "Produto excluído com sucesso.", typeof(ProdutoDto))]
    [SwaggerResponse(400, "Produto não encontrado")]
    public IActionResult DeletarProduto(
    [SwaggerParameter(Description = "ID do produto a ser excluído.", Required = true)] int id
    )
    {
        var produto = _context.Produtos.Find(id);
        if (produto == null)
        {
            return NotFound($"Produto com ID {id} não encontrado.");
        }

        _context.Produtos.Remove(produto);
        _context.SaveChanges();

        return Ok($"Produto com ID {id} removido com sucesso.");
    }
}
