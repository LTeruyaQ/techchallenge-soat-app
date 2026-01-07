using Core.DTOs.UseCases.AlertaEstoque;
using Core.Interfaces.Handlers.AlertasEstoque;
using Core.Interfaces.UseCases;

namespace Core.UseCases.AlertasEstoque
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IAlertaEstoqueUseCases
    /// utilizando os handlers individuais
    /// </summary>
    public class AlertaEstoqueUseCasesFacade : IAlertaEstoqueUseCases
    {
        private readonly ICadastrarVariosAlertasHandler _cadastrarVariosAlertasHandler;
        private readonly IVerificarAlertaEnviadoHojeHandler _verificarAlertaEnviadoHojeHandler;

        public AlertaEstoqueUseCasesFacade(
            ICadastrarVariosAlertasHandler cadastrarVariosAlertasHandler,
            IVerificarAlertaEnviadoHojeHandler verificarAlertaEnviadoHojeHandler)
        {
            _cadastrarVariosAlertasHandler = cadastrarVariosAlertasHandler ?? throw new ArgumentNullException(nameof(cadastrarVariosAlertasHandler));
            _verificarAlertaEnviadoHojeHandler = verificarAlertaEnviadoHojeHandler ?? throw new ArgumentNullException(nameof(verificarAlertaEnviadoHojeHandler));
        }

        public async Task CadastrarVariosAsync(IEnumerable<CadastrarAlertaEstoqueUseCaseDto> alertas)
        {
            await _cadastrarVariosAlertasHandler.Handle(alertas);
        }

        public async Task<bool> VerificarAlertaEnviadoHojeAsync(Guid estoqueId)
        {
            return await _verificarAlertaEnviadoHojeHandler.Handle(estoqueId);
        }
    }
}
