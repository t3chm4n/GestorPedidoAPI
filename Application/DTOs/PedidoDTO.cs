namespace GestorPedidoAPI.Application.DTOs
{
    public class PedidoDto
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; set; }
        public bool Fechado { get; set; }
        public IEnumerable<ProdutoDto> Produtos { get; set; } = new List<ProdutoDto>();
    }

    public class ProdutoDto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}