using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;

namespace Core.Interfaces.Controllers
{
    public interface IInsumoOSController
    {
        Task<IEnumerable<InsumoOSResponse>> CadastrarInsumos(Guid ordemServicoId, IEnumerable<CadastrarInsumoOSRequest> requests);
        Task DevolverInsumosAoEstoque(IEnumerable<DevolverInsumoOSRequest> insumosOS);
    }
}