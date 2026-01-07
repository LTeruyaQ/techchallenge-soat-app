using Core.DTOs.Entidades.OrdemServicos;
using Core.Entidades;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.Insumo;

public class ObterInsumosOSPorOSEspecificacao : EspecificacaoBase<InsumoOSEntityDto>
{
    private readonly Guid _ordemServicoId;

    public ObterInsumosOSPorOSEspecificacao(Guid ordemServicoId)
    {
        _ordemServicoId = ordemServicoId;
        AdicionarInclusao(i => i.Estoque);
        DefinirProjecao(i => new InsumoOS()
        {
            Id = i.Id,
            Ativo = i.Ativo,
            DataCadastro = i.DataCadastro,
            DataAtualizacao = i.DataAtualizacao,
            Quantidade = i.Quantidade,
            OrdemServicoId = i.OrdemServicoId,
            EstoqueId = i.EstoqueId,
            Estoque = new Entidades.Estoque()
            {
                Id = i.Estoque.Id,
                Ativo = i.Estoque.Ativo,
                DataCadastro = i.Estoque.DataCadastro,
                DataAtualizacao = i.Estoque.DataAtualizacao,
                Descricao = i.Estoque.Descricao,
                QuantidadeMinima = i.Estoque.QuantidadeMinima
            }
        });
    }

    public override Expression<Func<InsumoOSEntityDto, bool>> Expressao =>
        i => i.OrdemServicoId == _ordemServicoId;
}
