public class PedidoProdutoDto
{
    public int ProdutoId { get; set; }
    public string Nome { get; set; } = string.Empty; // Nome do Produto
    public decimal Preco { get; set; }              // Preço do Produto
    public int Quantidade { get; set; }             // Quantidade no Pedido
}