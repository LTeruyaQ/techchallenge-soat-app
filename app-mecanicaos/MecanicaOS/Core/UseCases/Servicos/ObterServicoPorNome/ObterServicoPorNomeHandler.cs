using Core.Entidades;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Servicos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Servicos.ObterServicoPorNome
{
    public class ObterServicoPorNomeHandler : UseCasesHandlerAbstrato<ObterServicoPorNomeHandler>, IObterServicoPorNomeHandler
    {
        private readonly IServicoGateway _servicoGateway;

        public ObterServicoPorNomeHandler(
            IServicoGateway servicoGateway,
            ILogGateway<ObterServicoPorNomeHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _servicoGateway = servicoGateway ?? throw new ArgumentNullException(nameof(servicoGateway));
        }

        public async Task<Servico?> Handle(string nome)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, nome);

                if (string.IsNullOrWhiteSpace(nome))
                {
                    LogFim(metodo, null);
                    return null;
                }

                var servico = await _servicoGateway.ObterServicosDisponiveisPorNomeAsync(nome);

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
