using Core.DTOs.UseCases.Servico;
using Core.Entidades;
using Core.Interfaces.Handlers.Servicos;
using Core.Interfaces.UseCases;

namespace Core.UseCases.Servicos
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IServicoUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class ServicoUseCasesFacade : IServicoUseCases
    {
        private readonly ICadastrarServicoHandler _cadastrarServicoHandler;
        private readonly IEditarServicoHandler _editarServicoHandler;
        private readonly IDeletarServicoHandler _deletarServicoHandler;
        private readonly IObterServicoHandler _obterServicoHandler;
        private readonly IObterTodosServicosHandler _obterTodosServicosHandler;
        private readonly IObterServicoPorNomeHandler _obterServicoPorNomeHandler;
        private readonly IObterServicosDisponiveisHandler _obterServicosDisponiveisHandler;

        public ServicoUseCasesFacade(
            ICadastrarServicoHandler cadastrarServicoHandler,
            IEditarServicoHandler editarServicoHandler,
            IDeletarServicoHandler deletarServicoHandler,
            IObterServicoHandler obterServicoHandler,
            IObterTodosServicosHandler obterTodosServicosHandler,
            IObterServicoPorNomeHandler obterServicoPorNomeHandler,
            IObterServicosDisponiveisHandler obterServicosDisponiveisHandler)
        {
            _cadastrarServicoHandler = cadastrarServicoHandler ?? throw new ArgumentNullException(nameof(cadastrarServicoHandler));
            _editarServicoHandler = editarServicoHandler ?? throw new ArgumentNullException(nameof(editarServicoHandler));
            _deletarServicoHandler = deletarServicoHandler ?? throw new ArgumentNullException(nameof(deletarServicoHandler));
            _obterServicoHandler = obterServicoHandler ?? throw new ArgumentNullException(nameof(obterServicoHandler));
            _obterTodosServicosHandler = obterTodosServicosHandler ?? throw new ArgumentNullException(nameof(obterTodosServicosHandler));
            _obterServicoPorNomeHandler = obterServicoPorNomeHandler ?? throw new ArgumentNullException(nameof(obterServicoPorNomeHandler));
            _obterServicosDisponiveisHandler = obterServicosDisponiveisHandler ?? throw new ArgumentNullException(nameof(obterServicosDisponiveisHandler));
        }

        public async Task<Servico> CadastrarServicoUseCaseAsync(CadastrarServicoUseCaseDto request)
        {
            return await _cadastrarServicoHandler.Handle(request);
        }

        public async Task<bool> DeletarServicoUseCaseAsync(Guid id)
        {
            return await _deletarServicoHandler.Handle(id);
        }

        public async Task<Servico> EditarServicoUseCaseAsync(Guid id, EditarServicoUseCaseDto request)
        {
            return await _editarServicoHandler.Handle(id, request);
        }

        public async Task<Servico> ObterServicoPorIdUseCaseAsync(Guid id)
        {
            return await _obterServicoHandler.Handle(id);
        }

        public async Task<Servico?> ObterServicoPorNomeUseCaseAsync(string nome)
        {
            return await _obterServicoPorNomeHandler.Handle(nome);
        }

        public async Task<IEnumerable<Servico>> ObterServicosDisponiveisUseCaseAsync()
        {
            return await _obterServicosDisponiveisHandler.Handle();
        }

        public async Task<IEnumerable<Servico>> ObterTodosUseCaseAsync()
        {
            return await _obterTodosServicosHandler.Handle();
        }
    }
}
