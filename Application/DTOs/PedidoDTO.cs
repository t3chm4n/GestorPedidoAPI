using GestorPedidoAPI.Domain.Enums;

namespace GestorPedidoAPI.Application.DTOs;

public class PedidoDto
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public string Status { get; set; } = string.Empty;
    public IEnumerable<PedidoProdutoDto> Produtos { get; set; } = new List<PedidoProdutoDto>();
}