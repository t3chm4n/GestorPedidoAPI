namespace GestorPedidoAPI.Domain.Entities;

public class PedidoProduto
{
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int Quantidade { get; set; } = 1;
}
