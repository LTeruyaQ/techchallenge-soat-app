using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.OrdemServico;

public class CadastrarOrdemServicoRequest
{
    [Required]
    public Guid ClienteId { get; set; }

    [Required]
    public Guid VeiculoId { get; set; }

    [Required]
    public Guid ServicoId { get; set; }

    [MaxLength(1000, ErrorMessage = "O campo deve ter no máximo 1000 caracteres.")]
    public string? Descricao { get; set; }
}