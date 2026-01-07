using Core.DTOs.UseCases.Servico;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Servicos;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Servicos.EditarServico
{
    public class EditarServicoHandler : UseCasesHandlerAbstrato<EditarServicoHandler>, IEditarServicoHandler
    {
        private readonly IServicoGateway _servicoGateway;

        public EditarServicoHandler(
            IServicoGateway servicoGateway,
            ILogGateway<EditarServicoHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _servicoGateway = servicoGateway ?? throw new ArgumentNullException(nameof(servicoGateway));
        }

        public async Task<Servico> Handle(Guid id, EditarServicoUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, new { id, request });

                // Validar dados
                if (string.IsNullOrWhiteSpace(request.Nome))
                    throw new DadosInvalidosException("Nome é obrigatório");

                if (string.IsNullOrWhiteSpace(request.Descricao))
                    throw new DadosInvalidosException("Descrição é obrigatória");

                if (request.Valor <= 0)
                    throw new DadosInvalidosException("Valor deve ser maior que zero");

                var servico = await _servicoGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Serviço não encontrado");

                servico.Nome = request.Nome;
                servico.Descricao = request.Descricao;

                if (!request.Valor.HasValue)
                    throw new DadosInvalidosException("Valor é obrigatório");
                servico.Valor = request.Valor.Value;

                if (!request.Disponivel.HasValue)
                    throw new DadosInvalidosException("Disponível é obrigatório");
                servico.Disponivel = request.Disponivel.Value;

                servico.DataAtualizacao = DateTime.UtcNow;

                await _servicoGateway.EditarAsync(servico);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao editar serviço");

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
