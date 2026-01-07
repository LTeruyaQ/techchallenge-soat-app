using Core.DTOs.Entidades.OrdemServicos;
using Core.Especificacoes.Base;
using System.Linq.Expressions;

namespace Core.Especificacoes.OrdemServico;

public class ObterOrdemServicoPorIdComInsumosEspecificacao : EspecificacaoBase<OrdemServicoEntityDto>
{
    private readonly Guid _id;

    public ObterOrdemServicoPorIdComInsumosEspecificacao(Guid id)
    {
        _id = id;
        AdicionarInclusao(os => os.InsumosOS);
        DefinirProjecao(os => new Entidades.OrdemServico
        {
            Id = os.Id,
            Ativo = os.Ativo,
            DataCadastro = os.DataCadastro,
            DataAtualizacao = os.DataAtualizacao,
            Descricao = os.Descricao,
            Status = os.Status,
            DataEnvioOrcamento = os.DataEnvioOrcamento,
            ServicoId = os.ServicoId,
            VeiculoId = os.VeiculoId,
            ClienteId = os.ClienteId,
            InsumosOS = os.InsumosOS.Select(io => new Entidades.InsumoOS
            {
                Id = io.Id,
                Ativo = io.Ativo,
                DataCadastro = io.DataCadastro,
                DataAtualizacao = io.DataAtualizacao,
                Quantidade = io.Quantidade,
                OrdemServicoId = io.OrdemServicoId,
                EstoqueId = io.EstoqueId
            })
        });
    }

    public override Expression<Func<OrdemServicoEntityDto, bool>> Expressao => os => os.Id == _id;
}
