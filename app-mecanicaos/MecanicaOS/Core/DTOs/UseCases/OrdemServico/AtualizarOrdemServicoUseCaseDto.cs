using Core.Enumeradores;

namespace Core.DTOs.UseCases.OrdemServico;

public class AtualizarOrdemServicoUseCaseDto
{
    public Guid? ClienteId { get; set; }
    public Guid? VeiculoId { get; set; }
    public Guid? ServicoId { get; set; }
    public string? Descricao { get; set; }
    public StatusOrdemServico? Status { get; set; }
    public decimal? Orcamento { get; set; }
    public DateTime? DataEnvioOrcamento { get; set; }
}