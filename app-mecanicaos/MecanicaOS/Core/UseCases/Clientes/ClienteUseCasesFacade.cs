using Core.DTOs.UseCases.Cliente;
using Core.Entidades;
using Core.Interfaces.Handlers.Clientes;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Clientes
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IClienteUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class ClienteUseCasesFacade : IClienteUseCases
    {
        private readonly ICadastrarClienteHandler _cadastrarClienteHandler;
        private readonly IAtualizarClienteHandler _atualizarClienteHandler;
        private readonly IObterClienteHandler _obterClienteHandler;
        private readonly IObterTodosClientesHandler _obterTodosClientesHandler;
        private readonly IRemoverClienteHandler _removerClienteHandler;
        private readonly IObterClientePorDocumentoHandler _obterClientePorDocumentoHandler;
        private readonly IObterClientePorNomeHandler _obterClientePorNomeHandler;

        public ClienteUseCasesFacade(
            ICadastrarClienteHandler cadastrarClienteHandler,
            IAtualizarClienteHandler atualizarClienteHandler,
            IObterClienteHandler obterClienteHandler,
            IObterTodosClientesHandler obterTodosClientesHandler,
            IRemoverClienteHandler removerClienteHandler,
            IObterClientePorDocumentoHandler obterClientePorDocumentoHandler,
            IObterClientePorNomeHandler obterClientePorNomeHandler)
        {
            _cadastrarClienteHandler = cadastrarClienteHandler ?? throw new ArgumentNullException(nameof(cadastrarClienteHandler));
            _atualizarClienteHandler = atualizarClienteHandler ?? throw new ArgumentNullException(nameof(atualizarClienteHandler));
            _obterClienteHandler = obterClienteHandler ?? throw new ArgumentNullException(nameof(obterClienteHandler));
            _obterTodosClientesHandler = obterTodosClientesHandler ?? throw new ArgumentNullException(nameof(obterTodosClientesHandler));
            _removerClienteHandler = removerClienteHandler ?? throw new ArgumentNullException(nameof(removerClienteHandler));
            _obterClientePorDocumentoHandler = obterClientePorDocumentoHandler ?? throw new ArgumentNullException(nameof(obterClientePorDocumentoHandler));
            _obterClientePorNomeHandler = obterClientePorNomeHandler ?? throw new ArgumentNullException(nameof(obterClientePorNomeHandler));
        }

        public async Task<Cliente> AtualizarUseCaseAsync(Guid id, AtualizarClienteUseCaseDto request)
        {
            return await _atualizarClienteHandler.Handle(id, request);
        }

        public async Task<Cliente> CadastrarUseCaseAsync(CadastrarClienteUseCaseDto request)
        {
            return await _cadastrarClienteHandler.Handle(request);
        }

        public async Task<Cliente> ObterPorDocumentoUseCaseAsync(string documento)
        {
            return await _obterClientePorDocumentoHandler.Handle(documento);
        }

        public async Task<IEnumerable<Cliente>> ObterPorNomeUseCaseAsync(string nome)
        {
            return await _obterClientePorNomeHandler.Handle(nome);
        }

        public async Task<Cliente> ObterPorIdUseCaseAsync(Guid id)
        {
            var cliente = await _obterClienteHandler.Handle(id);
            if (cliente is null)
                throw new InvalidOperationException($"Cliente com ID '{id}' nao encontrado.");
            return cliente;
        }

        public async Task<IEnumerable<Cliente>> ObterTodosUseCaseAsync()
        {
            return await _obterTodosClientesHandler.Handle();
        }

        public async Task<bool> RemoverUseCaseAsync(Guid id)
        {
            return await _removerClienteHandler.Handle(id);
        }
    }
}
