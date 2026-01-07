using Core.Entidades;

namespace Core.Interfaces.Handlers.Clientes
{
    /// <summary>
    /// Interface para o handler de obtenção de cliente por ID
    /// </summary>
    public interface IObterClienteHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de cliente por ID
        /// </summary>
        /// <param name="id">ID do cliente a ser obtido</param>
        /// <returns>Cliente encontrado ou null se não existir</returns>
        Task<Cliente?> Handle(Guid id);
    }
}
