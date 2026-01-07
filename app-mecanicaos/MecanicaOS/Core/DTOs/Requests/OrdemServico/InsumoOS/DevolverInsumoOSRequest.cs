using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.OrdemServico.InsumoOS;

public class DevolverInsumoOSRequest
{
    [Required]
    public Guid EstoqueId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "O valor deve ser no mínimo 1.")]
    public int Quantidade { get; set; }
}