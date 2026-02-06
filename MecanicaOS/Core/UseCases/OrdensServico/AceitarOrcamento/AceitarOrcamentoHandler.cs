using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.AceitarOrcamento
{
    public class AceitarOrcamentoHandler : UseCasesHandlerAbstrato<AceitarOrcamentoHandler>, IAceitarOrcamentoHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;

        public AceitarOrcamentoHandler(
            IOrdemServicoGateway ordemServicoGateway,
            ILogGateway<AceitarOrcamentoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _ordemServicoGateway = ordemServicoGateway ?? throw new ArgumentNullException(nameof(ordemServicoGateway));
        }

        public async Task<bool> Handle(Guid id)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var ordemServico = await _ordemServicoGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Ordem de serviço não encontrada");

                if (ordemServico.Status != StatusOrdemServico.AguardandoAprovacao)
                    throw new DadosInvalidosException("Ordem de serviço não está aguardando aprovação do orçamento");

                if (ordemServico.DataEnvioOrcamento.HasValue &&
                    ordemServico.DataEnvioOrcamento.Value.AddDays(7) < DateTime.UtcNow)
                    throw new DadosInvalidosException("Orçamento expirado");

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
