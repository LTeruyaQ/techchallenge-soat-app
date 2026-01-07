using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Exceptions;
using Core.Interfaces.Gateways;
using Core.Interfaces.Handlers.Estoques;
using Core.UseCases.Abstrato;

namespace Core.UseCases.Estoques.CadastrarEstoque
{
    public class CadastrarEstoqueHandler : UseCasesHandlerAbstrato<CadastrarEstoqueHandler>, ICadastrarEstoqueHandler
    {
        private readonly IEstoqueGateway _estoqueGateway;

        public CadastrarEstoqueHandler(
            IEstoqueGateway estoqueGateway,
            ILogGateway<CadastrarEstoqueHandler> logServicoGateway,
            IUnidadeDeTrabalhoGateway udtGateway,
            IUsuarioLogadoServicoGateway usuarioLogadoServicoGateway)
            : base(logServicoGateway, udtGateway, usuarioLogadoServicoGateway)
        {
            _estoqueGateway = estoqueGateway ?? throw new ArgumentNullException(nameof(estoqueGateway));
        }

        public async Task<Estoque> Handle(CadastrarEstoqueUseCaseDto request)
        {
            string metodo = nameof(Handle);

            try
            {
                LogInicio(metodo, request);

                // Verificar se já existe um estoque com o mesmo nome
                var estoques = await _estoqueGateway.ObterTodosAsync();
                if (estoques.Any(e => e.Insumo.Equals(request.Insumo, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new DadosJaCadastradosException("Produto já cadastrado");
                }

                Estoque estoque = new()
                {
                    Insumo = request.Insumo,
                    Descricao = request.Descricao,
                    QuantidadeDisponivel = request.QuantidadeDisponivel,
                    QuantidadeMinima = request.QuantidadeMinima,
                    Preco = request.Preco
                };

                await _estoqueGateway.CadastrarAsync(estoque);

                if (!await Commit())
                    throw new PersistirDadosException("Erro ao cadastrar estoque");

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
