using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Veiculos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Veiculos.ObterTodosVeiculos
{
    public class ObterTodosVeiculosHandler : UseCasesHandlerAbstrato<ObterTodosVeiculosHandler>, IObterTodosVeiculosHandler
    {
        private readonly IVeiculoGateway _veiculoGateway;

        public ObterTodosVeiculosHandler(
            IVeiculoGateway veiculoGateway,
            ILogGateway<ObterTodosVeiculosHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _veiculoGateway = veiculoGateway ?? throw new ArgumentNullException(nameof(veiculoGateway));
        }

        public async Task<IEnumerable<Veiculo>> Handle()
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var veiculos = await _veiculoGateway.ObterTodosAsync();

                LogFim(metodo, veiculos);

                return veiculos;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
