using Core.DTOs.Entidades.OrdemServicos;
using Core.Enumeradores;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.OrdemServico;

public class ObterOSOrcamentoExpiradoProjetadoEspecificacao : EspecificacaoBase<OrdemServicoEntityDto>
{
    public ObterOSOrcamentoExpiradoProjetadoEspecificacao()
    {
        AdicionarInclusao(os => os.InsumosOS, io => io.Estoque);
        DefinirProjecao(os => new Entidades.OrdemServico()
        {
            Id = os.Id,
            Ativo = os.Ativo,
            DataCadastro = os.DataCadastro,
            DataAtualizacao = os.DataAtualizacao,
            Descricao = os.Descricao,
            VeiculoId = os.VeiculoId,
            Status = os.Status,
            DataEnvioOrcamento = os.DataEnvioOrcamento,
            Orcamento = os.Orcamento,
            ClienteId = os.ClienteId,
            ServicoId = os.ServicoId,
            InsumosOS = os.InsumosOS.Select(io => new Entidades.InsumoOS
            {
                Id = io.Id,
                Ativo = io.Ativo,
                DataCadastro = io.DataCadastro,
                DataAtualizacao = io.DataAtualizacao,
                Quantidade = io.Quantidade,
                OrdemServicoId = io.OrdemServicoId,
                EstoqueId = io.EstoqueId,
                Estoque = new Entidades.Estoque
                {
                    Id = io.Estoque.Id,
                    Ativo = io.Estoque.Ativo,
                    DataCadastro = io.Estoque.DataCadastro,
                    DataAtualizacao = io.Estoque.DataAtualizacao,
                    Descricao = io.Estoque.Descricao,
                    QuantidadeMinima = io.Estoque.QuantidadeMinima
                }
            })
        });
    }

    public override Expression<Func<OrdemServicoEntityDto, bool>> Expressao =>
        o => o.Status == StatusOrdemServico.AguardandoAprovacao &&
             o.DataEnvioOrcamento.HasValue &&
             o.DataEnvioOrcamento.Value.AddDays(3) <= DateTime.UtcNow;
}
