using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;
using GestorPedidoAPI.Application.Exceptions;
using GestorPedidoAPI.Application.Commons;
using GestorPedidoAPI.Application.DTOs;


namespace GestorPedidoAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidoController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidoController(AppDbContext context)
    {
        _context = context;
    }

    private Pedido ObterPedidoComValidacao(int pedidoId)
    {
        var pedido = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto)
            .FirstOrDefault(p => p.Id == pedidoId);

        if (pedido == null)
            throw new PedidoException($"Pedido com ID {pedidoId} não encontrado.");

        return pedido!;
    }

    private Produto ObterProdutoComValidacao(int produtoId)
    {
        var produto = _context.Produtos.Find(produtoId);
        if (produto == null)
            throw new PedidoException($"Produto com ID {produtoId} não encontrado.");

        return produto;
    }

    // 1. Iniciar um novo pedido
    [HttpPost("iniciar")]
    public IActionResult IniciarPedido()
    {
        var pedido = new Pedido();
        _context.Pedidos.Add(pedido);
        _context.SaveChanges();
        return CreatedAtAction(nameof(ObterPedido), new { id = pedido.Id }, pedido);
    }

    // 2. Adicionar produto ao pedido
    [HttpPost("{pedidoId}/adicionar-produto")]
    public IActionResult AdicionarProduto(int pedidoId, [FromBody] int produtoId)
    {
        var pedido = ObterPedidoComValidacao(pedidoId);

        if (pedido.Fechado)
            throw new PedidoException($"Pedido com ID {pedidoId} já está fechado.");

        var produto = ObterProdutoComValidacao(produtoId);

        var pedidoProduto = _context.PedidoProdutos
            .FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoId);

        if (pedidoProduto == null)
        {
            pedidoProduto = new PedidoProduto
            {
                PedidoId = pedidoId,
                ProdutoId = produtoId,
                Quantidade = 1
            };
            _context.PedidoProdutos.Add(pedidoProduto);
        }
        else
        {
            pedidoProduto.Quantidade++;
        }

        _context.SaveChanges();
        return Ok($"Produto com ID {produtoId} adicionado ao pedido {pedidoId}.");
    }

    // 3. Remover produto do pedido
    [HttpPost("{pedidoId}/remover-produto")]
    public IActionResult RemoverProduto(int pedidoId, [FromBody] int produtoId)
    {
        var pedido = ObterPedidoComValidacao(pedidoId);

        if (pedido.Fechado)
            throw new PedidoException($"Pedido com ID {pedidoId} já está fechado.");

        var pedidoProduto = _context.PedidoProdutos
            .FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoId);

        if (pedidoProduto == null)
            throw new PedidoException($"Produto com ID {produtoId} não encontrado no pedido {pedidoId}.");

        // Remove o produto completamente do pedido
        _context.PedidoProdutos.Remove(pedidoProduto);

        _context.SaveChanges();
        return Ok($"Produto com ID {produtoId} removido completamente do pedido {pedidoId}.");
    }

    // 4. Fechar pedido
    [HttpPost("{pedidoId}/fechar")]
    public IActionResult FecharPedido(int pedidoId)
    {
        var pedido = ObterPedidoComValidacao(pedidoId);

        if (!pedido.PedidoProdutos.Any())
            throw new PedidoException($"Pedido com ID {pedidoId} não pode ser fechado sem produtos.");

        pedido.Fechado = true;
        _context.SaveChanges();

        return Ok($"Pedido com ID {pedidoId} fechado com sucesso.");
    }

    // 5. Listar pedidos
    [HttpGet]
    public IActionResult ListarPedidosPaginados([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var query = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto);

        var totalItems = query.Count();

        var pedidos = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList()
            .Select(p => new
            {
                p.Id,
                p.DataCriacao,
                p.Fechado,
                Produtos = p.PedidoProdutos.Select(pp => new
                {
                    pp.ProdutoId,
                    Nome = pp.Produto?.Nome ?? "Produto não especificado",
                    Preco = pp.Produto?.Preco ?? 0m,
                    pp.Quantidade
                })
            });

        var response = new PaginacaoResponse<object>
        {
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
            CurrentPage = page,
            Items = pedidos
        };

        return Ok(response);
    }

    //6. Obter Os pedidos por Status (com paginação)
    [HttpGet("filtrar")]
    public IActionResult ListarPedidosPaginadosEPorStatus([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? status = null)
    {
        // Validação de entrada
        var validationResult = ValidarParametrosDePaginacao(page, size);
        if (validationResult != null)
        {
            return validationResult;
        }

        // Construção da query inicial
        IQueryable<Pedido> query = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto);

        // Aplicar filtro de status, se necessário
        query = AplicarFiltroDeStatus(query, status);
        if (query == null)
        {
            return BadRequest("Status deve ser 'aberto' ou 'fechado'.");
        }

        // Paginação e carregamento dos dados
        var pedidosPaginados = query
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        // Projeção dos dados
        var pedidosDto = ProjetarPedidosParaDto(pedidosPaginados);

        return Ok(new //deixando pronto para o frontend
        {
            TotalItems = query.Count(),
            TotalPages = (int)Math.Ceiling((double)query.Count() / size),
            CurrentPage = page,
            Items = pedidosDto
        });
    }

    private IActionResult? ValidarParametrosDePaginacao(int page, int size)
    {
        if (page <= 0 || size <= 0)
        {
            return BadRequest("Página e tamanho devem ser maiores que 0.");
        }
        return null;
    }
    private IQueryable<Pedido> AplicarFiltroDeStatus(IQueryable<Pedido> query, string? status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return query; // Sem filtro
        }

        if (status != "aberto" && status != "fechado")
        {
            return null; // Retorna null para indicar status inválido
        }

        bool fechado = status == "fechado";
        return query.Where(p => p.Fechado == fechado);
    }
    private IEnumerable<object> ProjetarPedidosParaDto(IEnumerable<Pedido> pedidos)
    {
        return pedidos.Select(p => new
        {
            p.Id,
            p.DataCriacao,
            p.Fechado,
            Produtos = p.PedidoProdutos.Select(pp => new
            {
                pp.ProdutoId,
                Nome = pp.Produto?.Nome ?? "Produto não especificado",
                Preco = pp.Produto?.Preco ?? 0m,
                pp.Quantidade
            }).ToList()
        });
    }


    // 7. Obter pedido pelo ID
    [HttpGet("{id}")]
    public IActionResult ObterPedido(int id)
    {
        var pedido = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto)
            .FirstOrDefault(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound("Pedido não encontrado.");
        }

        var pedidoDto = new PedidoDto
        {
            Id = pedido.Id,
            DataCriacao = pedido.DataCriacao,
            Fechado = pedido.Fechado,
            Produtos = pedido.PedidoProdutos.Select(pp => new ProdutoDto
            {
                ProdutoId = pp.ProdutoId,
                Nome = pp.Produto?.Nome ?? "Produto não especificado",
                Preco = pp.Produto?.Preco ?? 0m,
                Quantidade = pp.Quantidade
            }).ToList()
        };

        return Ok(pedidoDto);
    }
}
