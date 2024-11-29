namespace GestorPedidoAPI.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public bool Fechado { get; set; } = false;

    public ICollection<PedidoProduto> PedidoProdutos { get; set; } = new List<PedidoProduto>();
}
