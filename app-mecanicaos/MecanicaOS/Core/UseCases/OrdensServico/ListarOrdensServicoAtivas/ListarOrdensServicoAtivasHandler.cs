using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.ListarOrdensServicoAtivas
{
    public class ListarOrdensServicoAtivasHandler : UseCasesHandlerAbstrato<ListarOrdensServicoAtivasHandler>, IListarOrdensServicoAtivasHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;

        public ListarOrdensServicoAtivasHandler(
            IOrdemServicoGateway ordemServicoGateway,
            ILogGateway<ListarOrdensServicoAtivasHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _ordemServicoGateway = ordemServicoGateway ?? throw new ArgumentNullException(nameof(ordemServicoGateway));
        }

        public async Task<IEnumerable<OrdemServico>> Handle()
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var ordensServico = await _ordemServicoGateway.ObterOrdensServicoAtivasAsync();

                var ordensServicoOrdenadas = ordensServico
                    .OrderBy(os => ObterPrioridadeStatus(os.Status))
                    .ThenBy(os => os.DataCadastro)
                    .ToList();

                LogFim(metodo, ordensServicoOrdenadas);

                return ordensServicoOrdenadas;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }

        private static int ObterPrioridadeStatus(StatusOrdemServico status)
        {
            return status switch
            {
                StatusOrdemServico.EmExecucao => 1,
                StatusOrdemServico.AguardandoAprovacao => 2,
                StatusOrdemServico.EmDiagnostico => 3,
                StatusOrdemServico.Recebida => 4,
                _ => int.MaxValue
            };
        }
    }
}
