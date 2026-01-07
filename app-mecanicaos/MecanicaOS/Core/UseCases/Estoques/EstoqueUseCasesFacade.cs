using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Interfaces.Handlers.Estoques;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Estoques
{
    /// <summary>
    /// Facade completo para manter compatibilidade com a interface IEstoqueUseCases
    /// utilizando todos os novos casos de uso individuais
    /// </summary>
    public class EstoqueUseCasesFacade : IEstoqueUseCases
    {
        private readonly ICadastrarEstoqueHandler _cadastrarEstoqueHandler;
        private readonly IAtualizarEstoqueHandler _atualizarEstoqueHandler;
        private readonly IDeletarEstoqueHandler _deletarEstoqueHandler;
        private readonly IObterEstoqueHandler _obterEstoqueHandler;
        private readonly IObterTodosEstoquesHandler _obterTodosEstoquesHandler;
        private readonly IObterEstoqueCriticoHandler _obterEstoqueCriticoHandler;

        public EstoqueUseCasesFacade(
            ICadastrarEstoqueHandler cadastrarEstoqueHandler,
            IAtualizarEstoqueHandler atualizarEstoqueHandler,
            IDeletarEstoqueHandler deletarEstoqueHandler,
            IObterEstoqueHandler obterEstoqueHandler,
            IObterTodosEstoquesHandler obterTodosEstoquesHandler,
            IObterEstoqueCriticoHandler obterEstoqueCriticoHandler)
        {
            _cadastrarEstoqueHandler = cadastrarEstoqueHandler ?? throw new ArgumentNullException(nameof(cadastrarEstoqueHandler));
            _atualizarEstoqueHandler = atualizarEstoqueHandler ?? throw new ArgumentNullException(nameof(atualizarEstoqueHandler));
            _deletarEstoqueHandler = deletarEstoqueHandler ?? throw new ArgumentNullException(nameof(deletarEstoqueHandler));
            _obterEstoqueHandler = obterEstoqueHandler ?? throw new ArgumentNullException(nameof(obterEstoqueHandler));
            _obterTodosEstoquesHandler = obterTodosEstoquesHandler ?? throw new ArgumentNullException(nameof(obterTodosEstoquesHandler));
            _obterEstoqueCriticoHandler = obterEstoqueCriticoHandler ?? throw new ArgumentNullException(nameof(obterEstoqueCriticoHandler));
        }

        public async Task<Estoque> CadastrarUseCaseAsync(CadastrarEstoqueUseCaseDto request)
        {
            return await _cadastrarEstoqueHandler.Handle(request);
        }

        public async Task<Estoque> AtualizarUseCaseAsync(Guid id, AtualizarEstoqueUseCaseDto request)
        {
            return await _atualizarEstoqueHandler.Handle(id, request);
        }

        public async Task<bool> DeletarUseCaseAsync(Guid id)
        {
            return await _deletarEstoqueHandler.Handle(id);
        }

        public async Task<Estoque> ObterPorIdUseCaseAsync(Guid id)
        {
            return await _obterEstoqueHandler.Handle(id);
        }

        public async Task<IEnumerable<Estoque>> ObterTodosUseCaseAsync()
        {
            return await _obterTodosEstoquesHandler.Handle();
        }

        public async Task<IEnumerable<Estoque>> ObterEstoqueCriticoUseCaseAsync()
        {
            return await _obterEstoqueCriticoHandler.Handle();
        }
    }
}
