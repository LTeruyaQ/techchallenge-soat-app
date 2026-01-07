using Core.DTOs.Requests.Veiculo;
using Core.DTOs.Responses.Veiculo;
using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;

namespace Core.Interfaces.Presenters
{
    public interface IVeiculoPresenter
    {
        CadastrarVeiculoUseCaseDto? ParaUseCaseDto(CadastrarVeiculoRequest request);
        AtualizarVeiculoUseCaseDto? ParaUseCaseDto(AtualizarVeiculoRequest request);
        VeiculoResponse? ParaResponse(Veiculo veiculo);
        IEnumerable<VeiculoResponse?> ParaResponse(IEnumerable<Veiculo> veiculos);
    }
}
