using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.AlertasEstoque;

namespace Core.UseCases.AlertasEstoque.VerificarAlertaEnviadoHoje
{
    public class VerificarAlertaEnviadoHojeHandler : IVerificarAlertaEnviadoHojeHandler
    {
        private readonly IAlertaEstoqueGateway _alertaEstoqueGateway;

        public VerificarAlertaEnviadoHojeHandler(IAlertaEstoqueGateway alertaEstoqueGateway)
        {
            _alertaEstoqueGateway = alertaEstoqueGateway ?? throw new ArgumentNullException(nameof(alertaEstoqueGateway));
        }

        public async Task<bool> Handle(Guid estoqueId)
        {
            var dataAtual = DateTime.UtcNow.Date;
            var alertasHoje = await _alertaEstoqueGateway.ObterAlertaDoDiaPorEstoqueAsync(estoqueId, dataAtual);
            
            return alertasHoje.Any();
        }
    }
}
