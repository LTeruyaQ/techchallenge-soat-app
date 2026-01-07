using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Estoques;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Estoques.ObterTodosEstoques
{
    public class ObterTodosEstoquesHandler : UseCasesHandlerAbstrato<ObterTodosEstoquesHandler>, IObterTodosEstoquesHandler
    {
        private readonly IEstoqueGateway _estoqueGateway;

        public ObterTodosEstoquesHandler(
            IEstoqueGateway estoqueGateway,
            ILogGateway<ObterTodosEstoquesHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _estoqueGateway = estoqueGateway ?? throw new ArgumentNullException(nameof(estoqueGateway));
        }

        public async Task<IEnumerable<Estoque>> Handle()
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var estoques = await _estoqueGateway.ObterTodosAsync();

                LogFim(metodo, estoques);

                return estoques;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
