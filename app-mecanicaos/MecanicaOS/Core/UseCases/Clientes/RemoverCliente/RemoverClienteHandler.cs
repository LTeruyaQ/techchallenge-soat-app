using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Clientes;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Clientes.RemoverCliente
{
    public class RemoverClienteHandler : UseCasesHandlerAbstrato<RemoverClienteHandler>, IRemoverClienteHandler
    {
        private readonly IClienteGateway _clienteGateway;

        public RemoverClienteHandler(
            IClienteGateway clienteGateway,
            ILogGateway<RemoverClienteHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _clienteGateway = clienteGateway ?? throw new ArgumentNullException(nameof(clienteGateway));
        }

        public async Task<bool> Handle(Guid id)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var cliente = await _clienteGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Cliente não encontrado");

                await _clienteGateway.DeletarAsync(cliente);
                var sucesso = await Commit();

                if (!sucesso)
                    throw new PersistirDadosException("Erro ao remover cliente");

                LogFim(metodo, sucesso);

                return sucesso;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
