namespace GestorPedidoAPI.Domain.Entities;

public class ProdutoEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
}
