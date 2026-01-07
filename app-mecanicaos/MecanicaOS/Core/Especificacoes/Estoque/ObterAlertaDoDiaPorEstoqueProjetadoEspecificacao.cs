using Core.DTOs.Entidades.Estoque;
using Core.Entidades;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Estoque;

public class ObterAlertaDoDiaPorEstoqueProjetadoEspecificacao : EspecificacaoBase<AlertaEstoqueEntityDto>
{
    private readonly Guid _estoqueId;
    private readonly DateTime _data;

    public ObterAlertaDoDiaPorEstoqueProjetadoEspecificacao(Guid estoqueId, DateTime? data = null)
    {
        _estoqueId = estoqueId;
        _data = (data ?? DateTime.UtcNow).Date;

        DefinirProjecao(a => new AlertaEstoque()
        {
            Id = a.Id,
            Ativo = a.Ativo,
            DataCadastro = a.DataCadastro,
            DataAtualizacao = a.DataAtualizacao,
            EstoqueId = a.EstoqueId,
            Estoque = new Entidades.Estoque()
            {
                Id = a.Estoque.Id,
                Ativo = a.Estoque.Ativo,
                DataCadastro = a.Estoque.DataCadastro,
                DataAtualizacao = a.Estoque.DataAtualizacao,
                QuantidadeMinima = a.Estoque.QuantidadeMinima,
                Descricao = a.Estoque.Descricao,
            }
        });
    }

    public override Expression<Func<AlertaEstoqueEntityDto, bool>> Expressao =>
       a => a.EstoqueId == _estoqueId && a.DataCadastro.Date == _data.Date;
}