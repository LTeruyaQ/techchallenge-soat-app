using Core.Entidades;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de obtenção de ordem de serviço por ID
    /// </summary>
    public interface IObterOrdemServicoHandler
    {
        /// <summary>
        /// Manipula a operação de obtenção de ordem de serviço por ID
        /// </summary>
        /// <param name="id">ID da ordem de serviço a ser obtida</param>
        /// <returns>Ordem de serviço encontrada ou null se não encontrada</returns>
        Task<OrdemServico?> Handle(Guid id);
    }
}
