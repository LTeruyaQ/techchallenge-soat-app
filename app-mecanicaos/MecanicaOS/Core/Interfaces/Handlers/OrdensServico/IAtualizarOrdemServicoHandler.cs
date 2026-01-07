using Core.DTOs.UseCases.OrdemServico;
using Core.Entidades;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de atualização de ordens de serviço
    /// </summary>
    public interface IAtualizarOrdemServicoHandler
    {
        /// <summary>
        /// Manipula a operação de atualização de ordem de serviço
        /// </summary>
        /// <param name="id">ID da ordem de serviço a ser atualizada</param>
        /// <param name="request">DTO com os dados atualizados da ordem de serviço</param>
        /// <returns>Ordem de serviço atualizada</returns>
        Task<OrdemServico> Handle(Guid id, AtualizarOrdemServicoUseCaseDto request);
    }
}
