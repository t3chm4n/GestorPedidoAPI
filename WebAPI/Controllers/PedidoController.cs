using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestorPedidoAPI.Domain.Entities;
using GestorPedidoAPI.Infrastructure.Persistence;

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
        var pedido = _context.Pedidos.Find(pedidoId);
        if (pedido == null || pedido.Fechado)
        {
            return BadRequest("Pedido não encontrado.");
        }
        if (pedido.Fechado)
        {
            return BadRequest("Pedido já fechado.");
        }

        var produto = _context.Produtos.Find(produtoId);
        if (produto == null)
        {
            return NotFound("Produto não encontrado.");
        }

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
        return Ok("Produto adicionado ao pedido.");
    }

    // 3. Remover produto do pedido
    [HttpPost("{pedidoId}/remover-produto")]
    public IActionResult RemoverProduto(int pedidoId, [FromBody] int produtoId)
    {
        var pedido = _context.Pedidos.Find(pedidoId);
        if (pedido == null || pedido.Fechado)
        {
            return BadRequest("Pedido não encontrado ou já fechado.");
        }

        var pedidoProduto = _context.PedidoProdutos
            .FirstOrDefault(pp => pp.PedidoId == pedidoId && pp.ProdutoId == produtoId);

        if (pedidoProduto == null)
        {
            return NotFound("Produto não encontrado no pedido.");
        }

        if (pedidoProduto.Quantidade > 1)
        {
            pedidoProduto.Quantidade--;
        }
        else
        {
            _context.PedidoProdutos.Remove(pedidoProduto);
        }

        _context.SaveChanges();
        return Ok("Produto removido do pedido.");
    }

    // 4. Fechar pedido
    [HttpPost("{pedidoId}/fechar")]
    public IActionResult FecharPedido(int pedidoId)
    {
        var pedido = _context.Pedidos.Find(pedidoId);
        if (pedido == null)
        {
            return NotFound("Pedido não encontrado.");
        }

        if (pedido.PedidoProdutos == null || !pedido.PedidoProdutos.Any())
        {
            return BadRequest("Pedido não pode ser fechado sem produtos.");
        }

        pedido.Fechado = true;
        _context.SaveChanges();
        return Ok("Pedido fechado com sucesso.");
    }

    // 5. Listar pedidos
    [HttpGet]
    public IActionResult ListarPedidos()
    {
        var pedidos = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto) // Inclua os produtos no carregamento
            .ToList() // Transfere os dados para a memória
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

        return Ok(pedidos);
    }

    // 6. Obter pedido pelo ID
    [HttpGet("{id}")]
    public IActionResult ObterPedido(int id)
    {
        var pedido = _context.Pedidos
            .Include(p => p.PedidoProdutos)
            .ThenInclude(pp => pp.Produto) // Inclua os produtos
            .Where(p => p.Id == id)
            .ToList() // Carrega os dados para memória
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
            })
            .FirstOrDefault();

        if (pedido == null)
        {
            return NotFound("Pedido não encontrado.");
        }

        return Ok(pedido);
    }
}
