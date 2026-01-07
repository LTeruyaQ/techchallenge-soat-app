using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Servicos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Servicos.ObterTodosServicos
{
    public class ObterTodosServicosHandler : UseCasesHandlerAbstrato<ObterTodosServicosHandler>, IObterTodosServicosHandler
    {
        private readonly IServicoGateway _servicoGateway;

        public ObterTodosServicosHandler(
            IServicoGateway servicoGateway,
            ILogGateway<ObterTodosServicosHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _servicoGateway = servicoGateway ?? throw new ArgumentNullException(nameof(servicoGateway));
        }

        public async Task<IEnumerable<Servico>> Handle()
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var servicos = await _servicoGateway.ObterTodosAsync();

                LogFim(metodo, servicos);
                return servicos;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
