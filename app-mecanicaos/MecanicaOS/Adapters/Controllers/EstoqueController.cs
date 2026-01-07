using Adapters.Presenters;
using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.UseCases.Estoque;
using Core.Interfaces.Controllers;
using Core.Interfaces.Presenters;
using Core.Interfaces.root;
using Core.Interfaces.UseCases;

namespace Adapters.Controllers
{
    public class EstoqueController : IEstoqueController
    {
        private readonly IEstoqueUseCases _estoqueUseCases;
        private readonly IEstoquePresenter _estoquePresenter;

        public EstoqueController(ICompositionRoot compositionRoot)
        {
            _estoqueUseCases = compositionRoot.CriarEstoqueUseCases();
            _estoquePresenter = new EstoquePresenter();
        }

        public async Task<IEnumerable<EstoqueResponse>> ObterTodos()
        {
            return _estoquePresenter.ParaResponse(await _estoqueUseCases.ObterTodosUseCaseAsync());
        }

        public async Task<EstoqueResponse> ObterPorId(Guid id)
        {
            return _estoquePresenter.ParaResponse(await _estoqueUseCases.ObterPorIdUseCaseAsync(id));
        }

        public async Task<EstoqueResponse> Cadastrar(CadastrarEstoqueRequest request)
        {
            var useCaseDto = MapearParaCadastrarEstoqueUseCaseDto(request);
            var resultado = await _estoqueUseCases.CadastrarUseCaseAsync(useCaseDto);
            return _estoquePresenter.ParaResponse(resultado);
        }

        internal CadastrarEstoqueUseCaseDto MapearParaCadastrarEstoqueUseCaseDto(CadastrarEstoqueRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarEstoqueUseCaseDto
            {
                Insumo = request.Insumo,
                Descricao = request.Descricao,
                Preco = Convert.ToDecimal(request.Preco),
                QuantidadeDisponivel = request.QuantidadeDisponivel,
                QuantidadeMinima = request.QuantidadeMinima
            };
        }

        public async Task<EstoqueResponse> Atualizar(Guid id, AtualizarEstoqueRequest request)
        {
            var useCaseDto = MapearParaAtualizarEstoqueUseCaseDto(request);
            var resultado = await _estoqueUseCases.AtualizarUseCaseAsync(id, useCaseDto);
            return _estoquePresenter.ParaResponse(resultado);
        }

        internal AtualizarEstoqueUseCaseDto MapearParaAtualizarEstoqueUseCaseDto(AtualizarEstoqueRequest request)
        {
            if (request is null)
                return null;

            return new AtualizarEstoqueUseCaseDto
            {
                Insumo = request.Insumo,
                Descricao = request.Descricao,
                Preco = request.Preco,
                QuantidadeDisponivel = request.QuantidadeDisponivel,
                QuantidadeMinima = request.QuantidadeMinima
            };
        }

        public async Task<bool> Deletar(Guid id)
        {
            return await _estoqueUseCases.DeletarUseCaseAsync(id);
        }

        public async Task<IEnumerable<EstoqueResponse>> ObterEstoqueCritico()
        {
            var estoques = await _estoqueUseCases.ObterEstoqueCriticoUseCaseAsync();
            return _estoquePresenter.ParaResponse(estoques)
                .Where(response => response != null)!;
        }
    }
}
