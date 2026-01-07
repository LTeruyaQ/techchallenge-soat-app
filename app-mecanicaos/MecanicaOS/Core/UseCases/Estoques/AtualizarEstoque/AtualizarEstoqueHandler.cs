using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Estoques;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Estoques.AtualizarEstoque
{
    public class AtualizarEstoqueHandler : UseCasesHandlerAbstrato<AtualizarEstoqueHandler>, IAtualizarEstoqueHandler
    {
        private readonly IEstoqueGateway _estoqueGateway;

        public AtualizarEstoqueHandler(
            IEstoqueGateway estoqueGateway,
            ILogGateway<AtualizarEstoqueHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _estoqueGateway = estoqueGateway ?? throw new ArgumentNullException(nameof(estoqueGateway));
        }

        public async Task<Estoque> Handle(Guid id, AtualizarEstoqueUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, new { id, request });

                var estoque = await _estoqueGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Estoque não encontrado");

                if (request.Insumo != null) estoque.Insumo = request.Insumo;
                if (request.Descricao != null) estoque.Descricao = request.Descricao;
                if (request.Preco.HasValue) estoque.Preco = request.Preco.Value;
                if (request.QuantidadeDisponivel.HasValue) estoque.QuantidadeDisponivel = request.QuantidadeDisponivel.Value;
                if (request.QuantidadeMinima.HasValue) estoque.QuantidadeMinima = request.QuantidadeMinima.Value;

                estoque.DataAtualizacao = DateTime.UtcNow;

                await _estoqueGateway.EditarAsync(estoque);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao atualizar estoque");

                LogFim(metodo, estoque);

                return estoque;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
