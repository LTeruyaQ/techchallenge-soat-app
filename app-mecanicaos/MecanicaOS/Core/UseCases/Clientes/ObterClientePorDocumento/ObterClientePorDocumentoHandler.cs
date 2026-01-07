using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Clientes.ObterClientePorDocumento
{
    public class ObterClientePorDocumentoHandler : UseCasesHandlerAbstrato<ObterClientePorDocumentoHandler>, IObterClientePorDocumentoHandler
    {
        private readonly IClienteGateway _clienteGateway;

        public ObterClientePorDocumentoHandler(
            IClienteGateway clienteGateway,
            ILogGateway<ObterClientePorDocumentoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _clienteGateway = clienteGateway ?? throw new ArgumentNullException(nameof(clienteGateway));
        }

        public async Task<Cliente> Handle(string documento)
        {
            const string metodo = nameof(Handle);
            LogInicio(metodo, documento);

            try
            {
                if (string.IsNullOrEmpty(documento))
                    throw new DadosInvalidosException("Deve ser informado o documento do usuario do cliente");

                if (await _clienteGateway.ObterClientePorDocumentoAsync(documento) is Cliente cliente)
                {
                    LogFim(metodo, cliente);
                    return cliente;
                }

                throw new DadosNaoEncontradosException($"Cliente de documento {documento} não encontrado");
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
