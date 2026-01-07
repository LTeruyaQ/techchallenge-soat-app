using Core.DTOs.Entidades.OrdemServicos;
using Core.Enumeradores;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.OrdemServico;

public class ObterOSOrcamentoExpiradoEspecificacao : EspecificacaoBase<OrdemServicoEntityDto>
{
    public ObterOSOrcamentoExpiradoEspecificacao()
    {
        AdicionarInclusao(os => os.InsumosOS, io => io.Estoque);
    }

    public override Expression<Func<OrdemServicoEntityDto, bool>> Expressao =>
        o => o.Status == StatusOrdemServico.AguardandoAprovacao &&
             o.DataEnvioOrcamento.HasValue &&
             o.DataEnvioOrcamento.Value.AddDays(3) <= DateTime.UtcNow;
}