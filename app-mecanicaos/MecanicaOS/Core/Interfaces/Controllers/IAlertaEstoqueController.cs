using Core.DTOs.Requests.Estoque;

namespace Core.Interfaces.Controllers
{
    public interface IAlertaEstoqueController
    {
        Task CadastrarAlertas(IEnumerable<CadastrarAlertaEstoqueRequest> request);
        Task<bool> VerificarAlertaEnviadoHoje(Guid estoqueId);
    }
}
