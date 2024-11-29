namespace WebAPI.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }

    // Relacionamento com PedidoProduto (muitos-para-muitos)
    public ICollection<PedidoProduto> PedidoProdutos { get; set; } = new List<PedidoProduto>();
}
