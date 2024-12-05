using System.Collections.Generic;

namespace GestorPedidoAPI.Application.Commons
{
    /// <summary>
    /// Representa a estrutura de resposta paginada para APIs.
    /// </summary>
    /// <typeparam name="T">O tipo dos itens na resposta.</typeparam>
    public class PaginacaoResponse<T>
    {
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public IEnumerable<T> Pedidos { get; set; } = Enumerable.Empty<T>();
    }
}