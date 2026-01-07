using Core.DTOs.UseCases.OrdemServico;
using Core.Entidades;
using Core.Enumeradores;
using Core.Interfaces.Handlers.OrdensServico;
using Core.Interfaces.UseCases;

namespace Core.UseCases.OrdensServico
{
    /// <summary>
    /// Facade para manter compatibilidade com a interface IOrdemServicoUseCases
    /// enquanto utiliza os novos casos de uso individuais
    /// </summary>
    public class OrdemServicoUseCasesFacade : IOrdemServicoUseCases
    {
        private readonly ICadastrarOrdemServicoHandler _cadastrarOrdemServicoHandler;
        private readonly IAtualizarOrdemServicoHandler _atualizarOrdemServicoHandler;
        private readonly IObterOrdemServicoHandler _obterOrdemServicoHandler;
        private readonly IObterTodosOrdensServicoHandler _obterTodosOrdensServicoHandler;
        private readonly IObterOrdemServicoPorStatusHandler _obterOrdemServicoPorStatusHandler;
        private readonly IAceitarOrcamentoHandler _aceitarOrcamentoHandler;
        private readonly IRecusarOrcamentoHandler _recusarOrcamentoHandler;
        private readonly IListarOrdensServicoAtivasHandler _listarOrdensServicoAtivasHandler;
        private readonly IObterOrcamentosExpiradosHandler _obterOrcamentosExpiradosHandler;

        public OrdemServicoUseCasesFacade(
            ICadastrarOrdemServicoHandler cadastrarOrdemServicoHandler,
            IAtualizarOrdemServicoHandler atualizarOrdemServicoHandler,
            IObterOrdemServicoHandler obterOrdemServicoHandler,
            IObterTodosOrdensServicoHandler obterTodosOrdensServicoHandler,
            IObterOrdemServicoPorStatusHandler obterOrdemServicoPorStatusHandler,
            IAceitarOrcamentoHandler aceitarOrcamentoHandler,
            IRecusarOrcamentoHandler recusarOrcamentoHandler,
            IListarOrdensServicoAtivasHandler listarOrdensServicoAtivasHandler,
            IObterOrcamentosExpiradosHandler obterOrcamentosExpiradosHandler)
        {
            _cadastrarOrdemServicoHandler = cadastrarOrdemServicoHandler ?? throw new ArgumentNullException(nameof(cadastrarOrdemServicoHandler));
            _atualizarOrdemServicoHandler = atualizarOrdemServicoHandler ?? throw new ArgumentNullException(nameof(atualizarOrdemServicoHandler));
            _obterOrdemServicoHandler = obterOrdemServicoHandler ?? throw new ArgumentNullException(nameof(obterOrdemServicoHandler));
            _obterTodosOrdensServicoHandler = obterTodosOrdensServicoHandler ?? throw new ArgumentNullException(nameof(obterTodosOrdensServicoHandler));
            _obterOrdemServicoPorStatusHandler = obterOrdemServicoPorStatusHandler ?? throw new ArgumentNullException(nameof(obterOrdemServicoPorStatusHandler));
            _aceitarOrcamentoHandler = aceitarOrcamentoHandler ?? throw new ArgumentNullException(nameof(aceitarOrcamentoHandler));
            _recusarOrcamentoHandler = recusarOrcamentoHandler ?? throw new ArgumentNullException(nameof(recusarOrcamentoHandler));
            _listarOrdensServicoAtivasHandler = listarOrdensServicoAtivasHandler ?? throw new ArgumentNullException(nameof(listarOrdensServicoAtivasHandler));
            _obterOrcamentosExpiradosHandler = obterOrcamentosExpiradosHandler ?? throw new ArgumentNullException(nameof(obterOrcamentosExpiradosHandler));
        }

        public async Task<OrdemServico> CadastrarUseCaseAsync(CadastrarOrdemServicoUseCaseDto request)
        {
            return await _cadastrarOrdemServicoHandler.Handle(request);
        }

        public async Task<OrdemServico> AtualizarUseCaseAsync(Guid id, AtualizarOrdemServicoUseCaseDto request)
        {
            return await _atualizarOrdemServicoHandler.Handle(id, request);
        }

        public async Task<OrdemServico?> ObterPorIdUseCaseAsync(Guid id)
        {
            return await _obterOrdemServicoHandler.Handle(id);
        }

        public async Task<IEnumerable<OrdemServico>> ObterTodosUseCaseAsync()
        {
            return await _obterTodosOrdensServicoHandler.Handle();
        }

        public async Task<IEnumerable<OrdemServico>> ObterPorStatusUseCaseAsync(StatusOrdemServico status)
        {
            return await _obterOrdemServicoPorStatusHandler.Handle(status);
        }

        public async Task<bool> AceitarOrcamentoUseCaseAsync(Guid id)
        {
            return await _aceitarOrcamentoHandler.Handle(id);
        }

        public async Task RecusarOrcamentoUseCaseAsync(Guid id)
        {
            await _recusarOrcamentoHandler.Handle(id);
        }

        public async Task<IEnumerable<OrdemServico>> ObterOrcamentosExpiradosUseCaseAsync()
        {
            return await _obterOrcamentosExpiradosHandler.Handle();
        }

        public async Task<IEnumerable<OrdemServico>> ListarOrdensServicoAtivasUseCaseAsync()
        {
            return await _listarOrdensServicoAtivasHandler.Handle();
        }
    }
}
