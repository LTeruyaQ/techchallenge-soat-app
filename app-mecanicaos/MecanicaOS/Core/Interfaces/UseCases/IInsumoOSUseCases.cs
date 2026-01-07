using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;

namespace Core.Interfaces.UseCases
{
    public interface IInsumoOSUseCases
    {
        Task<List<InsumoOS>> CadastrarInsumosUseCaseAsync(Guid ordemServicoId, List<CadastrarInsumoOSUseCaseDto> insumos);
        Task<bool> DevolverInsumosAoEstoqueUseCaseAsync(IEnumerable<DevolverInsumoOSRequest> insumosOS);
    }
}