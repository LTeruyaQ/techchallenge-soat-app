using Core.DTOs.Requests.Estoque;
using Core.DTOs.Responses.Estoque;
using Core.DTOs.UseCases.Estoque;
using Core.Entidades;
using Core.Interfaces.Presenters;

namespace Adapters.Presenters
{
    public class EstoquePresenter : IEstoquePresenter
    {
        public CadastrarEstoqueUseCaseDto? ParaUseCaseDto(CadastrarEstoqueRequest request)
        {
            if (request is null)
                return null;

            return new CadastrarEstoqueUseCaseDto
            {
                Insumo = request.Insumo,
                Descricao = request.Descricao,
                Preco = (decimal)request.Preco,
                QuantidadeDisponivel = request.QuantidadeDisponivel,
                QuantidadeMinima = request.QuantidadeMinima
            };
        }

        public AtualizarEstoqueUseCaseDto? ParaUseCaseDto(AtualizarEstoqueRequest request)
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

        public EstoqueResponse? ParaResponse(Estoque estoque)
        {
            if (estoque is null)
                return null;

            return new EstoqueResponse
            {
                Id = estoque.Id,
                Insumo = estoque.Insumo,
                Descricao = estoque.Descricao,
                Preco = (double)estoque.Preco,
                QuantidadeDisponivel = estoque.QuantidadeDisponivel,
                QuantidadeMinima = estoque.QuantidadeMinima,
                DataCadastro = estoque.DataCadastro,
                DataAtualizacao = estoque.DataAtualizacao
            };
        }

        public IEnumerable<EstoqueResponse?> ParaResponse(IEnumerable<Estoque> estoques)
        {
            if (estoques == null)
                return [];

            return estoques.Select(ParaResponse);
        }
    }
}
