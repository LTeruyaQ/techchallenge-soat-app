using Core.DTOs.UseCases.Eventos;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.RecusarOrcamento
{
    public class RecusarOrcamentoHandler : UseCasesHandlerAbstrato<RecusarOrcamentoHandler>, IRecusarOrcamentoHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;
        private readonly IEventoGateway _eventosGateway;

        public RecusarOrcamentoHandler(
            IOrdemServicoGateway ordemServicoGateway,
            IEventoGateway eventosGateway,
            ILogGateway<RecusarOrcamentoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _ordemServicoGateway = ordemServicoGateway ?? throw new ArgumentNullException(nameof(ordemServicoGateway));
            _eventosGateway = eventosGateway ?? throw new ArgumentNullException(nameof(eventosGateway));
        }

        public async Task<bool> Handle(Guid id)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var ordemServico = await _ordemServicoGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Ordem de serviço não encontrada");

                // Validar se está em status adequado para recusar orçamento
                if (ordemServico.Status != StatusOrdemServico.AguardandoAprovacao)
                    throw new DadosInvalidosException("Ordem de serviço não está aguardando aprovação do orçamento");

                ordemServico.Status = StatusOrdemServico.Cancelada;
                ordemServico.DataAtualizacao = DateTime.UtcNow;

                await _ordemServicoGateway.EditarAsync(ordemServico);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao recusar orçamento");

                await _eventosGateway.Publicar(ordemServico);

                LogFim(metodo, ordemServico);

                return true;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
