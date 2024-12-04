using GestorPedidoAPI.Domain.Enums;

namespace GestorPedidoAPI.Domain.Entities
{
    public class PedidoEntity
    {
        public int Id { get; set; }
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = PedidoStatus.Aberto.ToString(); // Inicializa como "Aberto"

        public ICollection<PedidoProduto> PedidoProdutos { get; set; } = new List<PedidoProduto>();
    }
}