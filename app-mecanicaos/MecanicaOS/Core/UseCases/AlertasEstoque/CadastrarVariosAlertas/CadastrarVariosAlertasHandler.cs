using Core.DTOs.UseCases.AlertaEstoque;
using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.AlertasEstoque;
using Core.UseCases.Abstrato;

namespace Core.UseCases.AlertasEstoque.CadastrarVariosAlertas
{
    public class CadastrarVariosAlertasHandler : UseCasesHandlerAbstrato<CadastrarVariosAlertasHandler>, ICadastrarVariosAlertasHandler
    {
        private readonly IAlertaEstoqueGateway _alertaEstoqueGateway;

        public CadastrarVariosAlertasHandler(
            IAlertaEstoqueGateway alertaEstoqueGateway,
            ILogGateway<CadastrarVariosAlertasHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _alertaEstoqueGateway = alertaEstoqueGateway ?? throw new ArgumentNullException(nameof(alertaEstoqueGateway));
        }
 
        public async Task Handle(IEnumerable<CadastrarAlertaEstoqueUseCaseDto> alertas)
        {
            if (alertas == null || !alertas.Any())
                return;
                
            // Handler (Core) cria as entidades a partir dos DTOs
            var entidades = alertas.Select(dto => new AlertaEstoque
            {
                EstoqueId = dto.EstoqueId,
                DataCadastro = dto.DataEnvio
            });

            await _alertaEstoqueGateway.CadastrarVariosAsync(entidades);
            await Commit();
        }
    }
}
