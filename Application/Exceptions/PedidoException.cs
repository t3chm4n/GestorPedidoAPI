namespace GestorPedidoAPI.Application.Exceptions
{
    /// <summary>
    /// Exceção personalizada para operações relacionadas a pedidos.
    /// </summary>
    public class PedidoException : Exception
    {
        /// <summary>
        /// Cria uma nova instância de <see cref="PedidoException"/> com uma mensagem de erro personalizada.
        /// </summary>
        /// <param name="message">A mensagem de erro que descreve o motivo da exceção.</param>
        public PedidoException(string message) : base(message) { }

        /// <summary>
        /// Cria uma nova instância de <see cref="PedidoException"/> com uma mensagem de erro personalizada
        /// e uma exceção interna que originou esta exceção.
        /// </summary>
        /// <param name="message">A mensagem de erro que descreve o motivo da exceção.</param>
        /// <param name="innerException">A exceção interna que causou esta exceção.</param>
        public PedidoException(string message, Exception innerException) : base(message, innerException) { }
    }
}
