namespace GestorPedidoAPI.Application.DTOs
{

    public class ProdutoDto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
    }
}