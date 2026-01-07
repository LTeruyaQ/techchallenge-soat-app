using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;

namespace Core.Interfaces.Handlers.InsumosOS
{
    /// <summary>
    /// Interface para o handler de cadastro de insumos em ordens de serviço
    /// </summary>
    public interface ICadastrarInsumosHandler
    {
        /// <summary>
        /// Manipula a operação de cadastro de insumos em uma ordem de serviço
        /// </summary>
        /// <param name="ordemServicoId">ID da ordem de serviço</param>
        /// <param name="request">Lista de DTOs com os dados dos insumos a serem cadastrados</param>
        /// <returns>Lista de insumos cadastrados</returns>
        Task<List<InsumoOS>> Handle(Guid ordemServicoId, List<CadastrarInsumoOSUseCaseDto> request);
    }
}
