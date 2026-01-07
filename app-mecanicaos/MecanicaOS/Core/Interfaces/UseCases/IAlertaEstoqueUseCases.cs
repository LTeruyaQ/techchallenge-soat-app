using Core.DTOs.UseCases.AlertaEstoque;

namespace Core.Interfaces.UseCases
{
    public interface IAlertaEstoqueUseCases
    {
        Task CadastrarVariosAsync(IEnumerable<CadastrarAlertaEstoqueUseCaseDto> alertas);
        Task<bool> VerificarAlertaEnviadoHojeAsync(Guid estoqueId);
    }
}
