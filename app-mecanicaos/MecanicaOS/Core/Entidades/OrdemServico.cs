using Core.Entidades.Abstratos;
using Core.Enumeradores;

namespace Core.Entidades;

public class OrdemServico : Entidade
{
    public Guid ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public Guid VeiculoId { get; set; }
    public Veiculo Veiculo { get; set; } = null!;
    public Guid ServicoId { get; set; }
    public Servico Servico { get; set; } = null!;
    public decimal? Orcamento { get; set; }
    public DateTime? DataEnvioOrcamento { get; set; }
    public string? Descricao { get; set; }
    public StatusOrdemServico Status { get; set; }
    public IEnumerable<InsumoOS> InsumosOS { get; set; } = [];

    public OrdemServico() { }

    public void Atualizar(Guid? clienteId, Guid? veiculoId, Guid? servicoId, string? descricao, StatusOrdemServico? status)
    {
        if (clienteId != null) ClienteId = clienteId.Value;
        if (veiculoId != null) VeiculoId = veiculoId.Value;
        if (servicoId != null) ServicoId = servicoId.Value;
        if (!string.IsNullOrEmpty(descricao)) Descricao = descricao;
        if (status != null) Status = status.Value;
    }
}