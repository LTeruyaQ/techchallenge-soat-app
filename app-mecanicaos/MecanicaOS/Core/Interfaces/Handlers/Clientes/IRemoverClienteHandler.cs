
namespace Core.Interfaces.Handlers.Clientes
{
    /// <summary>
    /// Interface para o handler de remoção de cliente
    /// </summary>
    public interface IRemoverClienteHandler
    {
        /// <summary>
        /// Manipula a operação de remoção de cliente
        /// </summary>
        /// <param name="id">ID do cliente a ser removido</param>
        /// <returns>Indica se a operação foi bem-sucedida</returns>
        Task<bool> Handle(Guid id);
    }
}
