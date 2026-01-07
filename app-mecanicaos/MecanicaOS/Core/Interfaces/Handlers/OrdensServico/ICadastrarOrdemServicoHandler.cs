using Core.DTOs.UseCases.OrdemServico;
using Core.Entidades;

namespace Core.Interfaces.Handlers.OrdensServico
{
    /// <summary>
    /// Interface para o handler de cadastro de ordens de serviço
    /// </summary>
    public interface ICadastrarOrdemServicoHandler
    {
        /// <summary>
        /// Manipula a operação de cadastro de ordem de serviço
        /// </summary>
        /// <param name="request">DTO com os dados da ordem de serviço a ser cadastrada</param>
        /// <returns>Ordem de serviço cadastrada</returns>
        Task<OrdemServico> Handle(CadastrarOrdemServicoUseCaseDto request);
    }
}
