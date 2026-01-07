using Core.Entidades;

namespace Core.Interfaces.Handlers.Clientes
{
    /// <summary>
    /// Interface para o handler de obtenção de todos os clientes
    /// </summary>
    public interface IObterTodosClientesHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de todos os clientes
        /// </summary>
        /// <returns>Lista de clientes</returns>
        Task<IEnumerable<Cliente>> Handle();
    }
}
