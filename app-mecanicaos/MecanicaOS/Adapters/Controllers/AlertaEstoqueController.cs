using Core.DTOs.Requests.Estoque;
using Core.DTOs.UseCases.AlertaEstoque;
using Core.Interfaces.Controllers;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class AlertaEstoqueController : IAlertaEstoqueController
    {
        private readonly IAlertaEstoqueUseCases _alertaEstoqueUseCases;

        public AlertaEstoqueController(ICompositionRoot compositionRoot)
        {
            _alertaEstoqueUseCases = compositionRoot.CriarAlertaEstoqueUseCases();
        }

        public async Task CadastrarAlertas(IEnumerable<CadastrarAlertaEstoqueRequest> request)
        {
            if (request == null || !request.Any())
                return;

            var useCaseDtos = request.Select(r => new CadastrarAlertaEstoqueUseCaseDto
            {
                EstoqueId = r.EstoqueId,
                DataEnvio = r.DataEnvio
            });

            await _alertaEstoqueUseCases.CadastrarVariosAsync(useCaseDtos);
        }

        public async Task<bool> VerificarAlertaEnviadoHoje(Guid estoqueId)
        {
            return await _alertaEstoqueUseCases.VerificarAlertaEnviadoHojeAsync(estoqueId);
        }
    }
}
