using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.UseCases.Estoque;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IEstoquePresenter
    {
        CadastrarEstoqueUseCaseDto? ParaUseCaseDto(CadastrarEstoqueRequest request);
        AtualizarEstoqueUseCaseDto? ParaUseCaseDto(AtualizarEstoqueRequest request);
        EstoqueResponse? ParaResponse(Estoque estoque);
        IEnumerable<EstoqueResponse?> ParaResponse(IEnumerable<Estoque> estoques);
    }
}
