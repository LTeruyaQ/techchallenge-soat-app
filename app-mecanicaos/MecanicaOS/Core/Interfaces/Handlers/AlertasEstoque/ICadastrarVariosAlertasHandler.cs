using Core.DTOs.UseCases.AlertaEstoque;

namespace Core.Interfaces.Handlers.AlertasEstoque
{
    public interface ICadastrarVariosAlertasHandler
    {
        Task Handle(IEnumerable<CadastrarAlertaEstoqueUseCaseDto> alertas);
    }
}
