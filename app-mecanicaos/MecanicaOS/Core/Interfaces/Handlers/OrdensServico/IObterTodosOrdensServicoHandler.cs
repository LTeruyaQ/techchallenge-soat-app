using Core.Entidades;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de obtenção de todas as ordens de serviço
    /// </summary>
    public interface IObterTodosOrdensServicoHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de todas as ordens de serviço
        /// </summary>
        /// <returns>Lista de ordens de serviço</returns>
        Task<IEnumerable<OrdemServico>> Handle();
    }
}
