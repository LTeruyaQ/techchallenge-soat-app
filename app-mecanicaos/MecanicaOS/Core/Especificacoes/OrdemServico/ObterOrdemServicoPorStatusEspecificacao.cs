using Core.DTOs.Entidades.OrdemServicos;
using Core.Enumeradores;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.OrdemServico;

public class ObterOrdemServicoPorStatusEspecificacao : EspecificacaoBase<OrdemServicoEntityDto>
{
    private readonly StatusOrdemServico _status;

    public ObterOrdemServicoPorStatusEspecificacao(StatusOrdemServico status)
    {
        _status = status;

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
        });
    }

    public override Expression<Func<OrdemServicoEntityDto, bool>> Expressao => os => os.Status == _status;
}