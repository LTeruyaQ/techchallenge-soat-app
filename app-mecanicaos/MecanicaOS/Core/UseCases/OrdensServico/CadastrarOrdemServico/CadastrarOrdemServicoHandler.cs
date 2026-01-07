using Core.DTOs.UseCases.OrdemServico;
using Core.Entidades;
using Core.Enumeradores;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.OrdensServico;
using Core.UseCases.Abstrato;

namespace Core.UseCases.OrdensServico.CadastrarOrdemServico
{
    public class CadastrarOrdemServicoHandler : UseCasesHandlerAbstrato<CadastrarOrdemServicoHandler>, ICadastrarOrdemServicoHandler
    {
        private readonly IOrdemServicoGateway _ordemServicoGateway;

        public CadastrarOrdemServicoHandler(
            IOrdemServicoGateway ordemServicoGateway,
            ILogGateway<CadastrarOrdemServicoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _ordemServicoGateway = ordemServicoGateway ?? throw new ArgumentNullException(nameof(ordemServicoGateway));
        }

        public async Task<OrdemServico> Handle(CadastrarOrdemServicoUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, request);

                // Controller já validou cliente e serviço
                var ordemServico = new OrdemServico
                {
                    ClienteId = request.ClienteId,
                    VeiculoId = request.VeiculoId,
                    ServicoId = request.ServicoId,
                    Descricao = request.Descricao,
                    Status = StatusOrdemServico.Recebida,
                    DataCadastro = DateTime.UtcNow,
                    Cliente = request.Cliente!, // Controller já resolveu
                    Servico = request.Servico!,
                    Veiculo = request.Veiculo!// Controller já resolveu
                };

                await _ordemServicoGateway.CadastrarAsync(ordemServico);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao cadastrar ordem de serviço");

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
