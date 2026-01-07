using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.ObterTodosOrdensServico
{
    public class ObterTodosOrdensServicoHandler : UseCasesHandlerAbstrato<ObterTodosOrdensServicoHandler>, IObterTodosOrdensServicoHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;

        public ObterTodosOrdensServicoHandler(
            IOrdemServicoGateway ordemServicoGateway,
            ILogGateway<ObterTodosOrdensServicoHandler> logServicoGateway,
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

                var ordensServico = await _ordemServicoGateway.ObterTodosAsync();

                var statusParaExcluir = ObterStatusParaExcluir();

                var ordensServicoOrdenadas = ordensServico
                    .Where(os => !statusParaExcluir.Contains(os.Status))
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

        private static List<StatusOrdemServico> ObterStatusParaExcluir()
        {
            return
            [
                StatusOrdemServico.Finalizada,
                StatusOrdemServico.Entregue,
                StatusOrdemServico.Cancelada,
                StatusOrdemServico.OrcamentoExpirado
            ];
        }

        private static int ObterPrioridadeStatus(StatusOrdemServico status)
        {
            return status switch
            {
                StatusOrdemServico.EmExecucao => 1,
                StatusOrdemServico.AguardandoAprovacao => 2,
                StatusOrdemServico.EmDiagnostico => 3,
                StatusOrdemServico.Recebida => 4,

                StatusOrdemServico.Finalizada => 99,
                StatusOrdemServico.Entregue => 99,
                StatusOrdemServico.Cancelada => 99,
                StatusOrdemServico.OrcamentoExpirado => 99,

                _ => int.MaxValue
            };
        }
    }
}
