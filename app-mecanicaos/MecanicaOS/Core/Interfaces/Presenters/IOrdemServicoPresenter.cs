using Core.DTOs.Requests.OrdemServico;
using Core.DTOs.Requests.OrdemServico.InsumoOS;
using Core.DTOs.Responses.OrdemServico;
using Core.DTOs.UseCases.OrdemServico;
using Core.DTOs.UseCases.OrdemServico.InsumoOS;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IOrdemServicoPresenter
    {
        CadastrarOrdemServicoUseCaseDto? ParaUseCaseDto(CadastrarOrdemServicoRequest request);
        AtualizarOrdemServicoUseCaseDto? ParaUseCaseDto(AtualizarOrdemServicoRequest request);
        CadastrarInsumoOSUseCaseDto? ParaUseCaseDto(CadastrarInsumoOSRequest request);
        OrdemServicoResponse? ParaResponse(OrdemServico ordemServico);
        IEnumerable<OrdemServicoResponse?> ParaResponse(IEnumerable<OrdemServico> ordensServico);
    }
}
