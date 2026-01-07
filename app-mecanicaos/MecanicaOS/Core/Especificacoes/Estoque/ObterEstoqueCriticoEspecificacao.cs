using Core.DTOs.Entidades.Estoque;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Estoque;

public class ObterEstoqueCriticoEspecificacao : EspecificacaoBase<EstoqueEntityDto>
{
    public ObterEstoqueCriticoEspecificacao()
    {
        DefinirProjecao(e => new Entidades.Estoque()
        {
            Id = e.Id,
            Ativo = e.Ativo,
            DataCadastro = e.DataCadastro,
            DataAtualizacao = e.DataAtualizacao,
            QuantidadeMinima = e.QuantidadeMinima,
            QuantidadeDisponivel = e.QuantidadeDisponivel,
            Descricao = e.Descricao,
            Insumo = e.Insumo,
            Preco = e.Preco
        });
    }

    public override Expression<Func<EstoqueEntityDto, bool>> Expressao =>
         e => e.QuantidadeDisponivel <= e.QuantidadeMinima;
}