using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Estoques;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Estoques.DeletarEstoque
{
    public class DeletarEstoqueHandler : UseCasesHandlerAbstrato<DeletarEstoqueHandler>, IDeletarEstoqueHandler
    {
        private readonly IEstoqueGateway _estoqueGateway;

        public DeletarEstoqueHandler(
            IEstoqueGateway estoqueGateway,
            ILogGateway<DeletarEstoqueHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _estoqueGateway = estoqueGateway ?? throw new ArgumentNullException(nameof(estoqueGateway));
        }

        public async Task<bool> Handle(Guid id)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, id);

                var estoque = await _estoqueGateway.ObterPorIdAsync(id)
                    ?? throw new DadosNaoEncontradosException("Estoque não encontrado");

                await _estoqueGateway.DeletarAsync(estoque);
                var sucesso = await Commit();

                if (!sucesso)
                    throw new PersistirDadosException("Erro ao deletar estoque");

                LogFim(metodo, sucesso);

                return sucesso;
            }
            catch (Exception e)
            {
                LogErro(metodo, e);
                throw;
            }
        }
    }
}
