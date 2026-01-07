using Core.Entidades;

namespace Core.Interfaces.Handlers.Servicos
{
    /// <summary>
    /// Interface para o handler de obtenção de serviços disponíveis
    /// </summary>
    public interface IObterServicosDisponiveisHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de serviços disponíveis
        /// </summary>
        /// <returns>Lista de serviços disponíveis</returns>
        Task<IEnumerable<Servico>> Handle();
    }
}
