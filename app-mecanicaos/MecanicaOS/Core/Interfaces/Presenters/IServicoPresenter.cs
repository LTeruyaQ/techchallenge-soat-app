using Core.DTOs.Requests.Servico;
using Core.DTOs.Responses.Servico;
using Core.DTOs.UseCases.Servico;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IServicoPresenter
    {
        IEnumerable<ServicoResponse?> ParaResponse(IEnumerable<Servico> enumerable);
        ServicoResponse? ParaResponse(Servico servico);
        CadastrarServicoUseCaseDto? ParaUseCaseDto(CadastrarServicoRequest request);
        EditarServicoUseCaseDto? ParaUseCaseDto(EditarServicoRequest request);
    }
}
