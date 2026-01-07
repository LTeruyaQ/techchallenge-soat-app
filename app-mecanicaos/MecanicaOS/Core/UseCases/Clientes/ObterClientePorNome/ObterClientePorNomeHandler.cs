using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Clientes.ObterClientePorNome
{
    public class ObterClientePorNomeHandler : UseCasesHandlerAbstrato<ObterClientePorNomeHandler>, IObterClientePorNomeHandler
    {
        private readonly IClienteGateway _clienteGateway;

        public ObterClientePorNomeHandler(
            IClienteGateway clienteGateway,
            ILogGateway<ObterClientePorNomeHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _clienteGateway = clienteGateway ?? throw new ArgumentNullException(nameof(clienteGateway));
        }

        public async Task<IEnumerable<Cliente>> Handle(string nome)
        {
            const string metodo = nameof(Handle);
            LogInicio(metodo, nome);

            try
            {
                if (string.IsNullOrWhiteSpace(nome))
                    throw new DadosInvalidosException("Deve ser informado o nome do cliente");

                var clientes = await _clienteGateway.ObterClientePorNomeAsync(nome);

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
