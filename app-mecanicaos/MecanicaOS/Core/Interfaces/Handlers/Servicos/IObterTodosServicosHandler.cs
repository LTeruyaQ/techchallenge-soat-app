using Core.Entidades;

namespace Core.Interfaces.Handlers.Servicos
{
    /// <summary>
    /// Interface para o handler de obtenção de todos os serviços
    /// </summary>
    public interface IObterTodosServicosHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de todos os serviços
        /// </summary>
        /// <returns>Lista de serviços</returns>
        Task<IEnumerable<Servico>> Handle();
    }
}
