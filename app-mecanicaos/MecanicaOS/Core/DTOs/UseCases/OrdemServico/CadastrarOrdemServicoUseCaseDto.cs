namespace Core.DTOs.UseCases.OrdemServico;

public class CadastrarOrdemServicoUseCaseDto
{
    public Guid ClienteId { get; set; }
    public Guid VeiculoId { get; set; }
    public Guid ServicoId { get; set; }
    public string? Descricao { get; set; }
    public Core.Entidades.Cliente? Cliente { get; set; } 
    public Core.Entidades.Servico? Servico { get; set; } 
    public Core.Entidades.Veiculo? Veiculo { get; set; } 
}