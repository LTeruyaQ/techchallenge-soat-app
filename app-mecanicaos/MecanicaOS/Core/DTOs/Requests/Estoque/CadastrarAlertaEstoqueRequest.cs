using System.ComponentModel.DataAnnotations;

namespace Core.DTOs.Requests.Estoque
{
    public class CadastrarAlertaEstoqueRequest
    {
        [Required]
        public Guid EstoqueId { get; set; }
        
        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
    }
}
