using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Clientes.ObterTodosClientes
{
    public class ObterTodosClientesHandler : UseCasesHandlerAbstrato<ObterTodosClientesHandler>, IObterTodosClientesHandler
    {
        private readonly IClienteGateway _clienteGateway;

        public ObterTodosClientesHandler(
            IClienteGateway clienteGateway,
            ILogGateway<ObterTodosClientesHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _clienteGateway = clienteGateway ?? throw new ArgumentNullException(nameof(clienteGateway));
        }

        public async Task<IEnumerable<Cliente>> Handle()
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo);

                var clientes = await _clienteGateway.ObterTodosClientesAsync();

                LogFim(metodo, clientes);

                return clientes;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
