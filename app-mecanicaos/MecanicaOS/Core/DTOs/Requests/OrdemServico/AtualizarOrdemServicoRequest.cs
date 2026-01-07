using Core.Enumeradores;
using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.OrdemServico;

public class AtualizarOrdemServicoRequest
{
    public Guid? ClienteId { get; set; }

    public Guid? VeiculoId { get; set; }

    public Guid? ServicoId { get; set; }

    [MaxLength(1000, ErrorMessage = "O campo deve ter no máximo 1000 caracteres.")]
    public string? Descricao { get; set; }
    
    public DateTime? DataEnvioOrcamento { get; set; }
    public StatusOrdemServico? Status { get; set; }
}
