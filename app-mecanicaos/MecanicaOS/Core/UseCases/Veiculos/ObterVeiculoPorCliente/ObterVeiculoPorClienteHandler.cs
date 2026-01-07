using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Veiculos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Veiculos.ObterVeiculoPorCliente
{
    public class ObterVeiculoPorClienteHandler : UseCasesHandlerAbstrato<ObterVeiculoPorClienteHandler>, IObterVeiculoPorClienteHandler
    {
        private readonly IVeiculoGateway _veiculoGateway;

        public ObterVeiculoPorClienteHandler(
            IVeiculoGateway veiculoGateway,
            ILogGateway<ObterVeiculoPorClienteHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _veiculoGateway = veiculoGateway ?? throw new ArgumentNullException(nameof(veiculoGateway));
        }

        public async Task<IEnumerable<Veiculo>> Handle(Guid clienteId)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, clienteId);

                var veiculos = await _veiculoGateway.ObterVeiculoPorClienteAsync(clienteId);
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
