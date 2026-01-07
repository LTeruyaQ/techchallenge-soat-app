using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Veiculos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Veiculos.ObterVeiculoPorPlaca
{
    public class ObterVeiculoPorPlacaHandler : UseCasesHandlerAbstrato<ObterVeiculoPorPlacaHandler>, IObterVeiculoPorPlacaHandler
    {
        private readonly IVeiculoGateway _veiculoGateway;

        public ObterVeiculoPorPlacaHandler(
            IVeiculoGateway veiculoGateway,
            ILogGateway<ObterVeiculoPorPlacaHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _veiculoGateway = veiculoGateway ?? throw new ArgumentNullException(nameof(veiculoGateway));
        }

        public async Task<Veiculo?> Handle(string placa)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, placa);

                var veiculos = await _veiculoGateway.ObterVeiculoPorPlacaAsync(placa);
                var veiculo = veiculos.FirstOrDefault();

                LogFim(metodo, veiculo);

                return veiculo;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
