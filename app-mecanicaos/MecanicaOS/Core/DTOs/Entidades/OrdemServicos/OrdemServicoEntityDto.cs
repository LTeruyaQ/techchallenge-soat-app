using Core.DTOs.Entidades.Autenticacao;
using Core.DTOs.Entidades.Cliente;
using Core.DTOs.Entidades.Servico;
using Core.DTOs.Entidades.Veiculo;
using Core.Enumeradores;

namespace Core.DTOs.Entidades.OrdemServicos;

public class OrdemServicoEntityDto : EntityDto
{
    public Guid ClienteId { get; set; }
    public ClienteEntityDto Cliente { get; set; } = null!;
    public Guid VeiculoId { get; set; }
    public VeiculoEntityDto Veiculo { get; set; } = null!;
    public Guid ServicoId { get; set; }
    public ServicoEntityDto Servico { get; set; } = null!;
    public decimal? Orcamento { get; set; }
    public DateTime? DataEnvioOrcamento { get; set; }
    public string? Descricao { get; set; }
    public StatusOrdemServico Status { get; set; }
    public ICollection<InsumoOSEntityDto> InsumosOS { get; set; } = [];
}