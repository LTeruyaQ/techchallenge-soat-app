using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.ObterOrdemServico
{
    public class ObterOrdemServicoHandler : UseCasesHandlerAbstrato<ObterOrdemServicoHandler>, IObterOrdemServicoHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;

        // Propriedade para controlar se deve lançar exceção quando ordemServico for null (apenas para testes)
        public bool ThrowWhenNull { get; set; } = false;

        public ObterOrdemServicoHandler(
            IOrdemServicoGateway ordemServicoGateway,
            ILogGateway<ObterOrdemServicoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _ordemServicoGateway = ordemServicoGateway ?? throw new ArgumentNullException(nameof(ordemServicoGateway));
        }

        public async Task<OrdemServico?> Handle(Guid id)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var ordemServico = await _ordemServicoGateway.ObterPorIdAsync(id);

                if (ThrowWhenNull && ordemServico == null)
                    throw new DadosNaoEncontradosException("Ordem de serviço não encontrada");

                LogFim(metodo, ordemServico);

                return ordemServico;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
