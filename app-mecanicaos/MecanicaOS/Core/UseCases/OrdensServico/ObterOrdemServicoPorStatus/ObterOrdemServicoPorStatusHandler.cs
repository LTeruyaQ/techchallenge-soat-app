using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.ObterOrdemServicoPorStatus
{
    public class ObterOrdemServicoPorStatusHandler : UseCasesHandlerAbstrato<ObterOrdemServicoPorStatusHandler>, IObterOrdemServicoPorStatusHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;

        public ObterOrdemServicoPorStatusHandler(
            IOrdemServicoGateway ordemServicoGateway,
            ILogGateway<ObterOrdemServicoPorStatusHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _ordemServicoGateway = ordemServicoGateway ?? throw new ArgumentNullException(nameof(ordemServicoGateway));
        }

        public async Task<IEnumerable<OrdemServico>> Handle(StatusOrdemServico status)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, status);

                var ordensServico = await _ordemServicoGateway.ObterOrdemServicoPorStatusAsync(status);

                LogFim(metodo, ordensServico);

                return ordensServico;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
