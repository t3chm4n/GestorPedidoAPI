using GestorPedidoAPI.Domain.Enums;

namespace GestorPedidoAPI.Application.DTOs;

public class PedidoDto
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public PedidoStatus Status { get; set; } // Atualiza para usar o enumerador
    public IEnumerable<PedidoProdutoDto> Produtos { get; set; } = new List<PedidoProdutoDto>();
}