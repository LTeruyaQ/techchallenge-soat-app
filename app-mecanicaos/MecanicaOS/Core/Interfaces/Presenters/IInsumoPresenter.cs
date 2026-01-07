using Core.DTOs.Responses.OrdemServico.InsumoOrdemServico;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IInsumoPresenter
    {
        IEnumerable<InsumoOSResponse> ToResponse(IEnumerable<InsumoOS> insumos);
    }
}
