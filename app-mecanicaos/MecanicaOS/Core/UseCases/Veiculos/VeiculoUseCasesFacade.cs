using Core.DTOs.UseCases.Veiculo;
using Core.Entidades;
using Core.Interfaces.Handlers.Veiculos;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Veiculos
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IVeiculoUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class VeiculoUseCasesFacade : IVeiculoUseCases
    {
        private readonly ICadastrarVeiculoHandler _cadastrarVeiculoHandler;
        private readonly IAtualizarVeiculoHandler _atualizarVeiculoHandler;
        private readonly IObterVeiculoHandler _obterVeiculoHandler;
        private readonly IObterTodosVeiculosHandler _obterTodosVeiculosHandler;
        private readonly IObterVeiculoPorClienteHandler _obterVeiculoPorClienteHandler;
        private readonly IObterVeiculoPorPlacaHandler _obterVeiculoPorPlacaHandler;
        private readonly IDeletarVeiculoHandler _deletarVeiculoHandler;

        public VeiculoUseCasesFacade(
            ICadastrarVeiculoHandler cadastrarVeiculoHandler,
            IAtualizarVeiculoHandler atualizarVeiculoHandler,
            IObterVeiculoHandler obterVeiculoHandler,
            IObterTodosVeiculosHandler obterTodosVeiculosHandler,
            IObterVeiculoPorClienteHandler obterVeiculoPorClienteHandler,
            IObterVeiculoPorPlacaHandler obterVeiculoPorPlacaHandler,
            IDeletarVeiculoHandler deletarVeiculoHandler)
        {
            _cadastrarVeiculoHandler = cadastrarVeiculoHandler ?? throw new ArgumentNullException(nameof(cadastrarVeiculoHandler));
            _atualizarVeiculoHandler = atualizarVeiculoHandler ?? throw new ArgumentNullException(nameof(atualizarVeiculoHandler));
            _obterVeiculoHandler = obterVeiculoHandler ?? throw new ArgumentNullException(nameof(obterVeiculoHandler));
            _obterTodosVeiculosHandler = obterTodosVeiculosHandler ?? throw new ArgumentNullException(nameof(obterTodosVeiculosHandler));
            _obterVeiculoPorClienteHandler = obterVeiculoPorClienteHandler ?? throw new ArgumentNullException(nameof(obterVeiculoPorClienteHandler));
            _obterVeiculoPorPlacaHandler = obterVeiculoPorPlacaHandler ?? throw new ArgumentNullException(nameof(obterVeiculoPorPlacaHandler));
            _deletarVeiculoHandler = deletarVeiculoHandler ?? throw new ArgumentNullException(nameof(deletarVeiculoHandler));
        }

        public async Task<Veiculo> AtualizarUseCaseAsync(Guid id, AtualizarVeiculoUseCaseDto request)
        {
            return await _atualizarVeiculoHandler.Handle(id, request);
        }

        public async Task<Veiculo> CadastrarUseCaseAsync(CadastrarVeiculoUseCaseDto request)
        {
            return await _cadastrarVeiculoHandler.Handle(request);
        }

        public async Task<Veiculo> ObterPorIdUseCaseAsync(Guid id)
        {
            return await _obterVeiculoHandler.Handle(id);
        }

        public async Task<IEnumerable<Veiculo>> ObterPorClienteUseCaseAsync(Guid clienteId)
        {
            return await _obterVeiculoPorClienteHandler.Handle(clienteId);
        }

        public async Task<Veiculo?> ObterPorPlacaUseCaseAsync(string placa)
        {
            return await _obterVeiculoPorPlacaHandler.Handle(placa);
        }

        public async Task<IEnumerable<Veiculo>> ObterTodosUseCaseAsync()
        {
            return await _obterTodosVeiculosHandler.Handle();
        }

        public async Task<bool> DeletarUseCaseAsync(Guid id)
        {
            return await _deletarVeiculoHandler.Handle(id);
        }
    }
}
