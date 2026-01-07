using Core.Entidades;
using Core.Enumeradores;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de obtenção de ordens de serviço por status
    /// </summary>
    public interface IObterOrdemServicoPorStatusHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de ordens de serviço por status
        /// </summary>
        /// <param name="status">Status das ordens de serviço a serem obtidas</param>
        /// <returns>Lista de ordens de serviço com o status especificado</returns>
        Task<IEnumerable<OrdemServico>> Handle(StatusOrdemServico status);
    }
}
