namespace GestorPedidoAPI.Domain.Entities;

public class PedidoProduto
{
    public int PedidoId { get; set; }
    public PedidoEntity Pedido { get; set; } = null!;

    public int ProdutoId { get; set; }
    public ProdutoEntity Produto { get; set; } = null!;

    public int Quantidade { get; set; } = 1;
}
