using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Servicos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Servicos.ObterServico
{
    public class ObterServicoHandler : UseCasesHandlerAbstrato<ObterServicoHandler>, IObterServicoHandler
    {
        private readonly IServicoGateway _servicoGateway;

        public ObterServicoHandler(
            IServicoGateway servicoGateway,
            ILogGateway<ObterServicoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _servicoGateway = servicoGateway ?? throw new ArgumentNullException(nameof(servicoGateway));
        }

        public async Task<Servico> Handle(Guid id)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var servico = await _servicoGateway.ObterPorIdAsync(id);
                if (servico == null)
                    throw new DadosNaoEncontradosException("Serviço não encontrado");

                LogFim(metodo, servico);
                return servico;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
