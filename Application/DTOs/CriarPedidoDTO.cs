namespace GestorPedidoAPI.Application.DTOs;

public class CriarPedidoDto
{
    public List<ProdutoPedidoDto> Produtos { get; set; } = new List<ProdutoPedidoDto>();
}

public class ProdutoPedidoDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; } = 1;
}